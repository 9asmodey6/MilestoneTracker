namespace MilestoneTracker.Application.Common.Shared.Models;

using Domain.Enums;

public record StepResult<T>(UserStateType NextState, T? UpdatedData);