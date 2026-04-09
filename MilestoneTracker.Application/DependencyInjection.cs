namespace MilestoneTracker.Application;

using System.Reflection;
using Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(IAppDbContext).Assembly); });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddAutoMapper(cfg => { },
            Assembly.GetExecutingAssembly());

        return services;
    }   
}