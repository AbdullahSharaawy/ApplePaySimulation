using ApplePaySimulation.Database.Context;
using ApplePaySimulation.Database.Entities;
using ApplePaySimulation.Infrastructure.Implementations;
using ApplePaySimulation.Models.SettingsModels;
using ApplePaySimulation.Repository.Abstracts;
using ApplePaySimulation.Services.Abstracts;
using ApplePaySimulation.Services.Implementations;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ApplePaySimulation
{
    public static class ModuleDependencies
    {


        public static IServiceCollection AddDatabaseDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // database connect
            services.AddDbContext<ApplePaySimulationContext>(options =>
               options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));


            return services;
        }
        public static IServiceCollection AddHangFireDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(x => x.UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));
            services.AddHangfireServer();
   

            return services;
        }
        public static IServiceCollection AddRepositoryDependencies(this IServiceCollection services)
        {
            // database connect
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }
        public static IServiceCollection AddEmailSettingsDependencies(this IServiceCollection services, IConfiguration configuration)
        {
           
            services.Configure<EmailSettings>(
   configuration.GetSection("EmailSettings")
);

            return services;
        }
        public static IServiceCollection AddEmailServiceDependencies(this IServiceCollection services)
        {
            
            services.AddTransient<IEmailSenderService, EmailSenderService>();

            return services;
        }
        public static IServiceCollection AddIdentityDependencies(this IServiceCollection services)
        {
           
            services.AddIdentity<User, IdentityRole>(option =>
            {
                option.Password.RequiredLength = 8;
                option.Password.RequireDigit = false;
                option.Password.RequireNonAlphanumeric = false;
                option.Password.RequireUppercase = false;
                option.SignIn.RequireConfirmedAccount = true;
               


            }).AddEntityFrameworkStores<ApplePaySimulationContext>().AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/User/Login";

                // You can also update the Access Denied and Logout paths while you're at it
                
                options.LogoutPath = "/User/Logout";
            });
            return services;
        }
    }
}
