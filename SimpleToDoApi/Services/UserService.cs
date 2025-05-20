using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO;
using SimpleToDoApi.DTO.User;
using SimpleToDoApi.Interfaces;
using SimpleToDoApi.Mappers;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ITodoContext _context;
    private readonly IDatabaseCleaner _dbCleaner;

    public UserService(UserManager<User> userManager, RoleManager<Role> roleManager, ITodoContext context,
        IDatabaseCleaner dbCleaner)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _dbCleaner = dbCleaner;
    }

    public async Task<UserPagedResult<UserDto>> GetAllUsers(UserQueryParameters parameters)
    {
        // 1. Начальный запрос к пользователям
        var query = _userManager.Users.AsQueryable();

        // 2. Применение фильтров
        if (!string.IsNullOrWhiteSpace(parameters.EmailContains))
        {
            query = query.Where(u => u.Email != null && u.Email.Contains(parameters.EmailContains));
        }

        if (!string.IsNullOrWhiteSpace(parameters.UserNameContains))
        {
            query = query.Where(u => u.UserName != null && u.UserName.Contains(parameters.UserNameContains));
        }

        if (!string.IsNullOrWhiteSpace(parameters.RoleName))
        {
            var role = await _roleManager.FindByNameAsync(parameters.RoleName);

            if (role != null)
            {
                // Находим UserId всех пользователей с этой ролью
                // Используем AsNoTracking для запросов только на чтение, если не планируем изменять эти сущности
                var userIdsInRole = await _context.UserRoles
                    .AsNoTracking()
                    .Where(ur => ur.RoleId == role.Id)
                    .Select(ur => ur.UserId)
                    .Distinct()
                    .ToListAsync();

                if (userIdsInRole.Any())
                {
                    query = query.Where(u => userIdsInRole.Contains(u.Id));
                }
                else // Роль существует, но ни у кого ее нет
                {
                    query = query.Where(u => false); // Вернет 0 пользователей
                }
            }
            else // Роль с таким именем не найдена
            {
                query = query.Where(u => false); // Вернет 0 пользователей
            }
        }

        // 3. Получение общего количества отфильтрованных пользователей (для пагинации)
        var totalCount = await query.CountAsync();

        // 4. Применение пагинации
        var usersOnPage = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        // 5. Если на текущей странице нет пользователей (после пагинации),
        // но общее количество > 0 (значит, запросили слишком большую страницу),
        // или если пользователей вообще нет по фильтрам.
        if (!usersOnPage.Any())
        {
            return new UserPagedResult<UserDto>(new List<UserDto>(), totalCount, parameters.PageNumber, parameters.PageSize);
        }

        // 6. Собираем ID пользователей текущей страницы
        var userIdsOnPage = usersOnPage.Select(u => u.Id).ToList();

        // 7. Загружаем связи "пользователь-роль" ТОЛЬКО для пользователей текущей страницы
        var usersRolesLinks = await _context.UserRoles
            .AsNoTracking()
            .Where(ur => userIdsOnPage.Contains(ur.UserId))
            .Select(ur => new
            {
                ur.UserId, ur.RoleId
            })
            .ToListAsync();

        // 8. Готовим словарь для имен ролей
        var rolesMap = new Dictionary<string, string>(); // string? на случай если Name в Role может быть null

        if (usersRolesLinks.Any())
        {
            var distinctRoleIdsInLinks = usersRolesLinks
                .Select(ur => ur.RoleId)
                .Distinct()
                .ToList();

            if (distinctRoleIdsInLinks.Any())
            {
                // Загружаем только те роли, которые реально используются пользователями на текущей странице
                rolesMap = await _context.Roles
                    .AsNoTracking()
                    .Where(r => distinctRoleIdsInLinks.Contains(r.Id))
                    .ToDictionaryAsync(r => r.Id, r => r.Name!);
            }
        }

        // 9. Группируем связи "пользователь-роль" по UserId
        var userRolesGroupedByUserId = usersRolesLinks.ToLookup(ur => ur.UserId, ur => ur.RoleId);

        // 10. Формируем итоговый список DTO для пользователей текущей страницы
        var usersListDto = usersOnPage.Select(userEntity =>
        {
            var currentUserRoleNames = new List<string>();

            if (userRolesGroupedByUserId.Contains(userEntity.Id))
            {
                foreach (var roleIdFromLookup in userRolesGroupedByUserId[userEntity.Id])
                {
                    if (rolesMap.TryGetValue(roleIdFromLookup, out var roleName) && roleName != null)
                    {
                        currentUserRoleNames.Add(roleName);
                    }
                }
            }

            return
                UserMapper.ToDto(userEntity,
                    currentUserRoleNames); // Предполагаем, что UserMapper.ToDto(User user, IList<string> roles) существует
        }).ToList();

        // 11. Возвращаем результат с пагинацией
        return new UserPagedResult<UserDto>(usersListDto, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    // МОЙ МЕТОД РЕШЕНИЯ N+1
    // public async Task<List<UserDto>> GetAllUsers()
    // {
    //     var users = await _userManager.Users.ToListAsync();
    //     var usersListDto = new List<UserDto>();
    //     foreach (var user in users)
    //     {
    //         var userRoles = await _userManager.GetRolesAsync(user);
    //         usersListDto.Add(UserMapper.ToDto(user, userRoles));
    //     }
    //     return usersListDto;
    // }

    public async Task<UserDto?> GetUserById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user != null)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return UserMapper.ToDto(user, roles.ToList());
        }

        return null;
    }
    public async Task<CreateUserResult> CreateAsync(CreateUserDto createUserDto)
    {
        var userEntity = UserMapper.FromDto(createUserDto);

        var creationIdentityResult = await _userManager.CreateAsync(userEntity, createUserDto.Password);

        // 2. Обработка результата создания пользователя
        if (!creationIdentityResult.Succeeded)
        {
            // Пытаемся определить причину ошибки из стандартных кодов Identity
            if (creationIdentityResult.Errors.Any(e => e.Code == "DuplicateUserName"))
            {
                return new CreateUserResult
                {
                    Error = CreateUserResult.CreateUserError.UserNameExists
                };
            }

            if (creationIdentityResult.Errors.Any(e => e.Code == "DuplicateEmail"))
            {
                return new CreateUserResult
                {
                    Error = CreateUserResult.CreateUserError.EmailExists
                };
            }

            return new CreateUserResult
            {
                Error = null
            }; // UserDto тоже будет null, что означает ошибку
        }

        if (createUserDto.RoleIds == null || createUserDto.RoleIds.Count == 0)
        {
            await _userManager.AddToRolesAsync(userEntity, "DefaultRole");
        }

        // 3. Пользователь успешно создан. Теперь назначаем роли.
        //    userEntity.Id теперь должен быть заполнен.
        if (createUserDto.RoleIds.Count > 0)
        {
            foreach (var roleIdString in createUserDto.RoleIds)
            {
                var role = await _roleManager.FindByIdAsync(roleIdString);

                if (role == null || role.Name == null)
                {
                    return new CreateUserResult
                    {
                        Error = CreateUserResult.CreateUserError.RoleNotFound
                    };
                }

                // Роль найдена, пытаемся добавить пользователя в эту роль по ИМЕНИ
                IdentityResult roleAssignmentResult = await _userManager.AddToRoleAsync(userEntity, role.Name);

                if (!roleAssignmentResult.Succeeded)
                {
                    return new CreateUserResult
                    {
                        Error = CreateUserResult.CreateUserError.RoleNotFound
                    };
                }
            }
        }

        // 4. Все успешно! Пользователь создан, роли назначены.
        //    Готовим UserDto для ответа.
        var assignedRoles = await _userManager.GetRolesAsync(userEntity); // Получаем список имен назначенных ролей
        var userDto = UserMapper.ToDto(userEntity, assignedRoles.ToList());

        return new CreateUserResult
        {
            User = userDto
        };
    }
    public async Task<UpdateUserResult> UpdateAsync(string id, UpdateUserDto updateUserDto)
    {
        var userToUpdate = await _userManager.FindByIdAsync(id);

        if (userToUpdate == null)
        {
            return new UpdateUserResult()
            {
                Error = UpdateUserResult.UpdateUserError.UserNotFound
            };
        }

        if (updateUserDto.UserName != userToUpdate.UserName && string.IsNullOrEmpty(userToUpdate.UserName))
        {
            var setUserNameResult = await _userManager.SetUserNameAsync(userToUpdate, userToUpdate.UserName);

            if (!setUserNameResult.Succeeded)
            {
                return new UpdateUserResult()
                {
                    Error = UpdateUserResult.UpdateUserError.UserNameExists
                };
            }
        }

        if (updateUserDto.Email != userToUpdate.Email && string.IsNullOrEmpty(userToUpdate.Email))
        {
            var setEmailResult = await _userManager.SetEmailAsync(userToUpdate, userToUpdate.Email);

            if (!setEmailResult.Succeeded)
            {
                return new UpdateUserResult()
                {
                    Error = UpdateUserResult.UpdateUserError.UserEmailExists
                };
            }
        }
        
        var currentRoleNames = await _userManager.GetRolesAsync(userToUpdate);
        var newWantedRoleNames = new List<string>();

        foreach (var roleId in updateUserDto.RoleIds) 
        {
            var role = await _roleManager.FindByIdAsync(roleId); 

            if (role?.Name == null) 
            {
                return new UpdateUserResult
                {
                    Error = UpdateUserResult.UpdateUserError.RoleNotFound
                };
            }

            newWantedRoleNames.Add(role.Name);
        }

        // Этап 3б: Определение ролей для добавления и удаления
        var rolesToRemove = currentRoleNames.Except(newWantedRoleNames).ToList();
        var rolesToAdd = newWantedRoleNames.Except(currentRoleNames).ToList();

        // Этап 3в: Применение изменений ролей
        if (rolesToRemove.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(userToUpdate, rolesToRemove);

            if (!removeResult.Succeeded)
            {
                // Логируем ошибки из removeResult.Errors
                return new UpdateUserResult
                {
                    Error = UpdateUserResult.UpdateUserError.RoleNotFound
                }; // Упрощенно
            }
        }

        if (rolesToAdd.Any())
        {
            var addResult = await _userManager.AddToRolesAsync(userToUpdate, rolesToAdd);

            if (!addResult.Succeeded)
            {
                // Логируем ошибки из addResult.Errors
                return new UpdateUserResult
                {
                    Error = UpdateUserResult.UpdateUserError.RoleNotFound
                }; // Упрощенно
            }
        }


        // TODO: Реализовать с использованием _userManager.FindByIdAsync(id) для поиска,
        // затем _userManager.UpdateAsync(user),
        // _userManager.ChangePasswordAsync (если нужно),
        // مدیریت ролей (_userManager.GetRolesAsync, RemoveFromRolesAsync, AddToRolesAsync)
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(string id) // Убедись, что в IUserService тоже string
    {
        // TODO: Реализовать с использованием _userManager.FindByIdAsync(id) и _userManager.DeleteAsync(user)
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAllAsync()
    {
        // TODO: Подумать, нужна ли эта операция с Identity.
        // Если да, то _userManager.Users.ToList() и затем цикл с _userManager.DeleteAsync(user)
        // ОСТОРОЖНО: очень разрушительная операция!
        throw new NotImplementedException();
    }
}
