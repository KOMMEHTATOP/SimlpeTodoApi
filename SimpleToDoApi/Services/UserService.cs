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

    public UserService(UserManager<User> userManager, RoleManager<Role> roleManager, ITodoContext context, IDatabaseCleaner dbCleaner)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _dbCleaner = dbCleaner;
    }
    
public async Task<List<UserDto>> GetAllUsers()
{
    // 1. Получаем всех пользователей (сущности User)
    var users = await _userManager.Users.ToListAsync();

    // 2. Если пользователей нет, сразу возвращаем пустой список DTO
    if (!users.Any())
    {
        return new List<UserDto>();
    }

    // 3. Собираем ID всех загруженных пользователей.
    var userIds = users.Select(u => u.Id).ToList();

    // 4. Загружаем все связи "пользователь-роль" для НАШИХ пользователей ОДНИМ запросом.
    // Каждая запись здесь - это пара { UserId, RoleId }
    var userRoleLinks = await _context.UserRoles // Это DbSet<IdentityUserRole<string>>
                                    .Where(ur => userIds.Contains(ur.UserId))
                                    .Select(ur => new { ur.UserId, ur.RoleId })
                                    .ToListAsync();

    // 5. Готовимся загружать информацию о ролях.
    // Если связей "пользователь-роль" не нашлось, значит ни у кого из пользователей нет ролей.
    // В этом случае нам не нужно делать запрос к таблице Roles.
    Dictionary<string, string> rolesMap = new Dictionary<string, string>();
    List<string> distinctRoleIdsAcrossAllUsers = new List<string>();

    if (userRoleLinks.Any())
    {
        // 6. Собираем УНИКАЛЬНЫЕ ID всех ролей, которые есть у наших пользователей.
        distinctRoleIdsAcrossAllUsers = userRoleLinks
            .Select(ur => ur.RoleId)
            .Distinct()
            .ToList();

        // 7. Загружаем информацию (Id и Name) только для этих УНИКАЛЬНЫХ ролей ОДНИМ запросом.
        // Превращаем результат в словарь для быстрого поиска имени роли по ее ID.
        rolesMap = await _context.Roles // Это DbSet<Role>
                                   .Where(r => distinctRoleIdsAcrossAllUsers.Contains(r.Id))
                                   .ToDictionaryAsync(r => r.Id, r => r.Name); // Словарь [RoleId -> RoleName]
    }

    // 8. Группируем связи "пользователь-роль" по UserId.
    // Для каждого UserId мы получим список всех RoleId, которые ему назначены.
    var userRolesGroupedByUserId = userRoleLinks.ToLookup(ur => ur.UserId, ur => ur.RoleId);

    // 9. Формируем итоговый список DTO.
    var usersListDto = users.Select(userEntity => // Идем по списку НАШИХ сущностей User
    {
        List<string> currentUserRoleNames = new List<string>();
        if (userRolesGroupedByUserId.Contains(userEntity.Id)) // Проверяем, есть ли у этого пользователя вообще роли
        {
            // userRolesGroupedByUserId[userEntity.Id] - это коллекция всех RoleId для данного userEntity.Id
            foreach (var roleIdAssignedToUser in userRolesGroupedByUserId[userEntity.Id])
            {
                if (rolesMap.TryGetValue(roleIdAssignedToUser, out var roleName)) // Ищем имя роли в нашем словаре
                {
                    currentUserRoleNames.Add(roleName);
                }
                // Если roleIdAssignedToUser нет в rolesMap (маловероятно, если данные консистентны),
                // то эта роль просто не будет добавлена в список имен.
            }
        }
        // 10. Маппим сущность User и собранный список имен ролей в UserDto.
        return UserMapper.ToDto(userEntity, currentUserRoleNames);
    }).ToList();

    // 11. Возвращаем результат.
    return usersListDto;
}

    public Task<UserDto?> GetUserById(string id) // Убедись, что в IUserService тоже string
    {
        // TODO: Реализовать с использованием _userManager.FindByIdAsync(id)
        // и затем смапить в UserDto
        throw new NotImplementedException();
    }

    public Task<CreateUserResult> CreateAsync(CreateUserDto createUserDto)
    {
        // TODO: Реализовать с использованием _userManager.CreateAsync(user, password)
        // и _userManager.AddToRolesAsync(user, roles)
        throw new NotImplementedException();
    }

    public Task<UpdateUserResult> UpdateAsync(UpdateUserDto updateUserDto) // Убедись, что в IUserService тоже string
    {
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
