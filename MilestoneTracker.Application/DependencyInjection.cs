namespace MilestoneTracker.Application;

using System.Reflection;
using AsyncKeyedLock;
using Common.Behaviors;
using Common.Features.Milestones.AddMilestone;
using Common.Features.Milestones.AddMilestone.Models;
using Common.Features.Milestones.AddMilestone.Steps;
using Common.Interfaces;
using Common.Shared.Interfaces.Services;
using Common.Shared.State;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(IAppDbContext).Assembly); });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        services.AddAutoMapper(cfg => { },
            Assembly.GetExecutingAssembly());
        
        services.AddSingleton<AsyncKeyedLocker<long>>();
        
        services.AddScoped<IMilestonePreviewService, MilestonePreviewService>();
        // process milestone step handler
        services.AddScoped<IUserFlowHandler, ProcessMilestoneStepHandler>();
        // add milestone steps
        services.AddScoped<IStepHandler<CreateMilestoneData>, StartedStepHandler>();
        services.AddScoped<IStepHandler<CreateMilestoneData>, ChildStepHandler>();
        services.AddScoped<IStepHandler<CreateMilestoneData>, CategoryStepHandler>();
        services.AddScoped<IStepHandler<CreateMilestoneData>, DateStepHandler>();
        services.AddScoped<IStepHandler<CreateMilestoneData>, TitleStepHandler>();
        services.AddScoped<IStepHandler<CreateMilestoneData>, DescriptionStepHandler>();
        services.AddScoped<IStepHandler<CreateMilestoneData>, MediaStepHandler>();
        services.AddScoped<IStepHandler<CreateMilestoneData>, ConfirmingStepHandler>();
        
        return services;
    }
}