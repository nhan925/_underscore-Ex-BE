using dotenv;
using dotenv.net;
using dotenv.net.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Serilog.Events;
using Serilog;
using student_management_api.Repositories;
using student_management_api.Services;
using System;
using System.Data;
using System.Text;
using Microsoft.OpenApi.Models;
using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using Dapper;
using student_management_api.Helpers;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace student_management_api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        // Load environment variables
        DotEnv.Load();
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? throw new Exception("JWT_SECRET is missing");

        // Register custom type handlers
        SqlMapper.AddTypeHandler(new JsonbTypeHandler<Dictionary<string, string>>());

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information) // Ignore noisy logs
            .Enrich.FromLogContext()
            .WriteTo.Console() // Logs to Console
            .WriteTo.File("Logs/api_log.txt", rollingInterval: RollingInterval.Day) // Logs to a file
            .CreateLogger();

        builder.Host.UseSerilog(); // Replace default logging with Serilog

        var MyAllowSpecificOrigins = "_myCorsPolicy";

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                policy =>
                {
                    policy.WithOrigins("https://localhost:7088") // Change to your Blazor domain
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
        });

        // Add services to the container.
        builder.Services.AddSingleton<IDbConnection>(sp => new NpgsqlConnection(connectionString));
        builder.Services.AddSingleton<IJwtService, JwtService>();
        builder.Services.AddSingleton<IFacultyService, FacultyService>();
        builder.Services.AddSingleton<IStudentService, StudentService>();
        builder.Services.AddSingleton<IStudentStatusService, StudentStatusService>();
        builder.Services.AddSingleton<IStudyProgramService, StudyProgramService>();


        builder.Services.AddSingleton<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IStudentRepository, StudentRepository>();
        builder.Services.AddSingleton<IFacultyRepository, FacultyRepository>();
        builder.Services.AddSingleton<IStudentStatusRepository, StudentStatusRepository>();
        builder.Services.AddSingleton<IStudyProgramRepository, StudyProgramRepository>();

        builder.Services.AddControllers();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false, 
                ValidateAudience = false, 
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
            };
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Student Management API",
                Version = "v1",
                Description = "API for managing students in the system"
            });

            // Enable JWT Authentication in Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer {your JWT token}'"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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


        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.Configure<JsonOptions>(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.WriteIndented = true;
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Student Management API v1");
                options.RoutePrefix = string.Empty; // Access Swagger at root URL (/)
            });
        }
        
        app.UseSerilogRequestLogging();

        app.UseCors(MyAllowSpecificOrigins);

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
