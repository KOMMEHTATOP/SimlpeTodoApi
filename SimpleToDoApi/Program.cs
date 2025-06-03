using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SimpleToDoApi.Data;
using Serilog;
using SimpleToDoApi.Interfaces;
using SimpleToDoApi.Interfaces.Auth;
using SimpleToDoApi.Middleware;
using SimpleToDoApi.Models;
using SimpleToDoApi.Services;
using System.Text;

namespace SimpleToDoApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Настройка Serilog: логировать начиная с Information, писать в файл с ротацией по дням
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    path: "logs/app.log",           // Путь к файлу с логами
                    rollingInterval: RollingInterval.Day, // Новый файл на каждый день
                    retainedFileCountLimit: 7,           // Храним логи за 7 дней
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
            
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Host.UseSerilog(); // Использовать Serilog вместо стандартного логгера

            // Регистрация TodoContext
            builder.Services.AddDbContext<TodoContext>(options => options.UseSqlServer(
                "Server=localhost;Database=ToDoDb;Trusted_Connection=True;Encrypt=False"));
            // Регистрация ITodoContext, чтобы он разрешался в тот же экземпляр TodoContext

            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    options.User.RequireUniqueEmail = true; //уникальный email
                    options.Password.RequireDigit = false; //числа в пароле
                    options.Password.RequiredLength = 4; // Минимальная длина пароля
                    options.Password.RequireNonAlphanumeric = false; // Требовать не буквенно-цифровой символ
                    options.Password.RequireUppercase = false; // Требовать заглавную букву
                    options.Password.RequireLowercase = false; // Требовать строчную букву
                })
                .AddEntityFrameworkStores<TodoContext>() //указываем EFcore использовать мой TodoContext для хранения данных Identity
                .AddDefaultTokenProviders(); // Добавляет провайдеры для генерации токенов (например, для сброса пароля, подтверждения email)
            
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    };
                });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SimpleToDoApi", Version = "v1" });
                // Секция для JWT
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });
            
            builder.Services.AddScoped<IDatabaseCleaner, DatabaseCleaner>();
            builder.Services.AddScoped<ITodoContext>(provider => provider.GetRequiredService<TodoContext>());
            builder.Services.AddScoped<IToDoService, ToDoService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<ITokenService, TokenService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            app.UseHttpsRedirection();
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseAuthentication();  
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
