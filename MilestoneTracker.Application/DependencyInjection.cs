namespace MilestoneTracker.Application;

using System.Reflection;
using AsyncKeyedLock;
using Common.Behaviors;
using Common.Features.Children.GainAccess;
using Common.Features.Children.GainAccess.Steps;
using Common.Features.Children.ProvideAccess;
using Common.Features.Children.ProvideAccess.Steps;
using Common.Features.Milestones.AddMilestone;
using Common.Features.Milestones.AddMilestone.Models;
using Common.Features.Milestones.AddMilestone.Steps;
using Common.Features.Milestones.DeleteMilestone;
using Common.Features.Milestones.DeleteMilestone.Models;
using Common.Features.Milestones.DeleteMilestone.Steps;
using Common.Features.Milestones.GetMilestone;
using Common.Features.Milestones.GetMilestone.Models;
using Common.Features.Milestones.GetMilestone.Steps;
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
        services.AddScoped<IMilestoneViewService, MilestoneViewService>();
        // process milestone step handler
        services.AddScoped<IUserFlowHandler, ProcessMilestoneStepHandler>();
        // add milestone steps
        services.AddScoped<IStepHandler<CreateMilestoneData>, StartedStepCreateHandler>();
        services.AddScoped<IStepHandler<CreateMilestoneData>, ChildStepCreateHandler>();
        services.AddScoped<IStepHandler<CreateMilestoneData>, CategoryStepCreateHandler>();
        services.AddScoped<IStepHandler<CreateMilestoneData>, DateStepCreateHandler>();
        services.AddScoped<IStepHandler<CreateMilestoneData>, TitleStepCreateHandler>();
        services.AddScoped<IStepHandler<CreateMilestoneData>, DescriptionStepCreateHandler>();
        services.AddScoped<IStepHandler<CreateMilestoneData>, MediaStepCreateHandler>();
        services.AddScoped<IStepHandler<CreateMilestoneData>, ConfirmingStepCreateHandler>();
        // process get milestone step handler
        services.AddScoped<IUserFlowHandler, ProcessGetMilestoneStepHandler>();
        // get milestone steps
        services.AddScoped<IStepHandler<GetMilestoneData>, ChildStepGetHandler>();
        services.AddScoped<IStepHandler<GetMilestoneData>, ModeStepGetHandler>();
        services.AddScoped<IStepHandler<GetMilestoneData>, CategoryStepGetHandler>();
        services.AddScoped<IStepHandler<GetMilestoneData>, DateStepGetHandler>();
        services.AddScoped<IStepHandler<GetMilestoneData>, ListStepGetHandler>();
        services.AddScoped<IStepHandler<GetMilestoneData>, ViewItemStepGetHandler>();
        
        // Delete milestone
        services.AddScoped<IUserFlowHandler, ProcessDeleteMilestoneStepHandler>();
        services.AddScoped<IStepHandler<DeleteMilestoneData>, ConfirmDeleteStepHandler>();
        services.AddScoped<IStepHandler<DeleteMilestoneData>, UndoDeleteStepHandler>();

        // Provide access
        services.AddScoped<IUserFlowHandler, ProcessProvideStepAccessStepHandler>();
        services.AddScoped<IStepHandler<ProvideAccessData>, ChildProvideStepHandler>();
        services.AddScoped<IStepHandler<ProvideAccessData>, ConfirmProvideStepHandler>();

        // Gain access
        services.AddScoped<IUserFlowHandler, ProcessGainByTokenStepHandler>();
        services.AddScoped<IStepHandler<GainAccessByTokenCommand>, EnteringGainStepHandler>();
        services.AddScoped<IStepHandler<GainAccessByTokenCommand>, ConfirmingGainStepHandler>();
        
        return services;
    }
}