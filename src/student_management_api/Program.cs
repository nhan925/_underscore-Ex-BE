using Dapper;
using DinkToPdf;
using DinkToPdf.Contracts;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using dotenv;
using dotenv.net;
using dotenv.net.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using student_management_api.Contracts.IRepositories;
using student_management_api.Contracts.IServices;
using student_management_api.Helpers;
using student_management_api.Localization;
using student_management_api.Localization.AiTranslation;
using student_management_api.Middlewares;
using student_management_api.Models.DTO;
using student_management_api.Repositories;
using student_management_api.Services;
using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

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
        var AiServiceUrl = Environment.GetEnvironmentVariable("AI_SERVICE_URL")
            ?? throw new Exception("AI_SERVICE_URL is missing");
        var AiModel = Environment.GetEnvironmentVariable("AI_MODEL")
            ?? throw new Exception("AI_MODEL is missing");
        var AiApiKey = Environment.GetEnvironmentVariable("AI_API_KEY")
            ?? throw new Exception("AI_API_KEY is missing");

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
        builder.Services.AddTransient<IDbConnection>(sp => new NpgsqlConnection(connectionString));
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<IFacultyService, FacultyService>();
        builder.Services.AddScoped<IStudentService, StudentService>();
        builder.Services.AddScoped<IStudentStatusService, StudentStatusService>();
        builder.Services.AddScoped<IStudyProgramService, StudyProgramService>();
        builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
        builder.Services.AddScoped<ICountryPhoneCodeService, CountryPhoneCodeService>();
        builder.Services.AddScoped<ICourseService, CourseService>();
        builder.Services.AddScoped<IYearAndSemesterService, YearAndSemesterService>();
        builder.Services.AddScoped<ICourseClassService, CourseClassService>();
        builder.Services.AddScoped<ILecturersService, LecturersService>();
        builder.Services.AddScoped<ICourseEnrollmentService, CourseEnrollmentService>();


        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IStudentRepository, StudentRepository>();
        builder.Services.AddScoped<IFacultyRepository, FacultyRepository>();
        builder.Services.AddScoped<IStudentStatusRepository, StudentStatusRepository>();
        builder.Services.AddScoped<IStudyProgramRepository, StudyProgramRepository>();
        builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        builder.Services.AddScoped<ICourseRepository, CourseRepository>();
        builder.Services.AddScoped<IYearAndSemesterRepository, YearAndSemesterRepository>();
        builder.Services.AddScoped<ICourseClassRepository, CourseClassRepository>();
        builder.Services.AddScoped<ILecturersRepository, LecturersRepository>();
        builder.Services.AddScoped<ICourseEnrollmentRepository, CourseEnrollmentRepository>();

        builder.Services.AddSingleton<IConverter>(new SynchronizedConverter(new PdfTools()));
        builder.Services.AddSingleton<IExternalTranslationService>(new GeminiTranslationService(AiApiKey, AiModel));

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

            // Add custom "language" header to all endpoints
            options.OperationFilter<LanguageHeaderOperationFilter>();
        });


        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // Add localization services
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

        // Set up supported cultures
        var supportedCultures = new[] { "en", "vi" }; // English, Vietnamese

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var cultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
            options.DefaultRequestCulture = new RequestCulture("en");
            options.SupportedCultures = cultures;
            options.SupportedUICultures = cultures;

            // Clear default providers to control order
            options.RequestCultureProviders.Clear();

            // Priority order: 1. Query string  2. Accept-Language header
            options.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
            options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
        });


        builder.Services.Configure<JsonOptions>(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.WriteIndented = true;
        });


        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<Messages>>();

                var errors = context.ModelState
                    .Where(e => e.Value!.Errors.Count > 0)
                    .SelectMany(e => e.Value!.Errors.Select(err => err.ErrorMessage)) // Extract only messages
                    .ToList();

                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Validation failed: {Errors}", string.Join("; ", errors));

                return new BadRequestObjectResult(new ErrorResponse<List<string>>
                (
                    status: 400,
                    message: localizer["invalid_input"],
                    details: errors
                ));
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
        app.UseRequestLocalization();
        app.UseAuthentication();
        app.UseMiddleware<LoggingEnrichmentMiddleware>();
        app.UseSerilogRequestLogging();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
