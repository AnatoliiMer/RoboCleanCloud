using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Application.Interfaces.Services;
using RoboCleanCloud.Infrastructure.Persistence;
using RoboCleanCloud.Infrastructure.Persistence.Repositories;

namespace RoboCleanCloud.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Добавляем DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Регистрируем репозитории
        services.AddScoped<IRobotRepository, RobotRepository>();
        services.AddScoped<ICleaningSessionRepository, CleaningSessionRepository>();
        services.AddScoped<ICleaningScheduleRepository, CleaningScheduleRepository>();
        services.AddScoped<IMaintenanceItemRepository, MaintenanceItemRepository>();

        // Регистрируем UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}