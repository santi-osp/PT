using DataGo.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DataGo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<DataGoDbContext>(options => options.UseNpgsql(connectionString));
        services.AddSingleton(NpgsqlDataSource.Create(connectionString));
        services.AddScoped<IResidentialCustomerRepository, ResidentialCustomerRepository>();
        services.AddScoped<ICenterRepository, CenterRepository>();
        services.AddScoped<INeighborhoodRepository, NeighborhoodRepository>();
        services.AddScoped<IModernChannelCustomerRepository, ModernChannelCustomerRepository>();
        return services;
    }
}
