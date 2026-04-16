namespace MilestoneTracker.Application.Common.Features.Children.AddChild;

using Domain.Entities;
using Infrastructure.Models;
using MediatR;

public record ProcessChildStepCommand(
    BotContext Context,
    UserState UserState);