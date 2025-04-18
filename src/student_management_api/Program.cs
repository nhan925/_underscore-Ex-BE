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
using Serilog.Sinks.PostgreSQL;
using student_management_api.Middlewares;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using student_management_api.Models.DTO;
using DinkToPdf.Contracts;
using DinkToPdf;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;

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
        SqlMapper.AddTypeHandler(new JsonbTypeHandler<List<string>>());
        SqlMapper.AddTypeHandler(new JsonbTypeHandler<Dictionary<int, List<int>>>());

        Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Ignore most Microsoft logs below Warning
        .MinimumLevel.Override("System", LogEventLevel.Warning) // Ignore most System logs below Warning
        .Enrich.FromLogContext() // Important for logging scoped properties
        .Enrich.WithProperty("Application", "StudentManagementAPI") // Helps identify logs from this app
        .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] [User:{UserId}] {Scope} {Message}{NewLine}")
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] [User:{UserId}] {Scope} {Message}{NewLine}{Exception}")
        .WriteTo.PostgreSQL(
            connectionString: connectionString,
            tableName: "logs",
            needAutoCreateTable: true,
            columnOptions: new Dictionary<string, ColumnWriterBase>
            {
                { "Message", new RenderedMessageColumnWriter() },
                { "Level", new LevelColumnWriter() },
                { "Timestamp", new TimestampColumnWriter() },
                { "Exception", new ExceptionColumnWriter() },
                { "UserId", new SinglePropertyColumnWriter("UserId", PropertyWriteMethod.Raw) },
                { "RequestPath", new SinglePropertyColumnWriter("RequestPath", PropertyWriteMethod.Raw) },
                { "Method", new SinglePropertyColumnWriter("Method", PropertyWriteMethod.Raw) },
                { "CorrelationId", new SinglePropertyColumnWriter("CorrelationId", PropertyWriteMethod.Raw) },
                { "SourceContext", new SinglePropertyColumnWriter("SourceContext", PropertyWriteMethod.Raw) },
                { "ClientIp", new SinglePropertyColumnWriter("ClientIp", PropertyWriteMethod.Raw) },
                { "UserAgent", new SinglePropertyColumnWriter("UserAgent", PropertyWriteMethod.Raw) }
            }
        )
        .CreateLogger();

        builder.Host.UseSerilog(); // Replace default logging with Serilog

        var MyAllowSpecificOrigins = "_myCorsPolicy";

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                policy =>
                {
                    policy.WithOrigins("https://localhost:7088", "http://localhost:5048") // Change to your Blazor domain
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
        builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
        builder.Services.AddSingleton<ICountryPhoneCodeService, CountryPhoneCodeService>();
        builder.Services.AddSingleton<ICourseService, CourseService>();
        builder.Services.AddSingleton<IYearAndSemesterService, YearAndSemesterService>();
        builder.Services.AddSingleton<ICourseClassService, CourseClassService>();
        builder.Services.AddSingleton<ILecturersService, LecturersService>();
        builder.Services.AddSingleton<ICourseEnrollmentService, CourseEnrollmentService>();


        builder.Services.AddSingleton<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IStudentRepository, StudentRepository>();
        builder.Services.AddSingleton<IFacultyRepository, FacultyRepository>();
        builder.Services.AddSingleton<IStudentStatusRepository, StudentStatusRepository>();
        builder.Services.AddSingleton<IStudyProgramRepository, StudyProgramRepository>();
        builder.Services.AddSingleton<IConfigurationRepository, ConfigurationRepository>();
        builder.Services.AddSingleton<ICourseRepository, CourseRepository>();
        builder.Services.AddSingleton<IYearAndSemesterRepository, YearAndSemesterRepository>();
        builder.Services.AddSingleton<ICourseClassRepository, CourseClassRepository>();
        builder.Services.AddSingleton<ILecturersRepository, LecturersRepository>();
        builder.Services.AddSingleton<ICourseEnrollmentRepository, CourseEnrollmentRepository>();

        builder.Services.AddSingleton<IConverter>(new SynchronizedConverter(new PdfTools()));

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

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .SelectMany(e => e.Value.Errors.Select(err => err.ErrorMessage)) // Extract only messages
                    .ToList();

                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Validation failed: {Errors}", string.Join("; ", errors));

                return new BadRequestObjectResult(new
                {
                    title = "One or more validation errors occurred.",
                    status = 400,
                    errors = context.ModelState
                });
            };
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
        
        app.UseCors(MyAllowSpecificOrigins);
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseMiddleware<LoggingEnrichmentMiddleware>();
        app.UseSerilogRequestLogging();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
