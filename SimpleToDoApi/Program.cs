﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SimpleToDoApi.Data;
using System.Text;
using Serilog;
using SimpleToDoApi.Interfaces;
using SimpleToDoApi.Middleware;
using SimpleToDoApi.Services;

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
            builder.Services.AddDbContext<TodoContext>(options => options.UseSqlServer("Server=localhost;Database=TodoDb;Trusted_Connection=True;Encrypt=False"));

            //Настройка JWT аутентификации
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogError(context.Exception, "❌ Authentication failed");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogInformation("✅ Token validated");
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogWarning("⚠️ Challenge error: {Error}", context.ErrorDescription);
                            return Task.CompletedTask;
                        }
                    };
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "your_issuer",
                        ValidAudience = "your_audience",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key_which_is_long_enough")),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SimpleToDoApi", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
                                Id="Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });
            builder.Services.AddScoped<IDatabaseCleaner, DatabaseCleaner>();
            builder.Services.AddScoped<ITodoContext, TodoContext>();
            builder.Services.AddScoped<IToDoService, ToDoService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRoleService, RoleService>();

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
