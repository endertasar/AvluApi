using System.Text;
using AvluApi.Auth;
using AvluApi.Infrastructure.Database;
using AvluApi.Repositories.Implementations;
using AvluApi.Repositories.Interfaces;
using AvluApi.Services.Implementations;
using AvluApi.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace AvluApi.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddHttpContextAccessor();
        services.AddScoped<ISiteContext, SiteContext>();

        services.AddHttpClient("OneSignal", client =>
        {
            client.BaseAddress = new Uri("https://onesignal.com");
            client.DefaultRequestHeaders.Add("Authorization",
                $"Basic {configuration["OneSignal:RestApiKey"]}");
        });
        services.AddScoped<IOneSignalService, OneSignalService>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IPropertyBalanceRepository, PropertyBalanceRepository>();
        services.AddScoped<IResidentRepository, ResidentRepository>();
        services.AddScoped<IDuesRepository, DuesRepository>();
        services.AddScoped<IChargeRepository, ChargeRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IPhoneMapRepository, PhoneMapRepository>();
        services.AddScoped<IExtraPaymentRepository, ExtraPaymentRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<IResidentService, ResidentService>();
        services.AddScoped<IDuesService, DuesService>();
        services.AddScoped<IChargeService, ChargeService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IExtraPaymentService, ExtraPaymentService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly",    p => p.RequireClaim("user_type", "admin"));
            options.AddPolicy("OwnerOnly",    p => p.RequireClaim("user_type", "admin").RequireClaim("role", "Owner"));
            options.AddPolicy("ResidentOnly", p => p.RequireClaim("user_type", "resident"));
        });

        return services;
    }

    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "AvluApi", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Bearer token. Örnek: 'Bearer {token}'",
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
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });
        return services;
    }
}
