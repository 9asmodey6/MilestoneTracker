using MilestoneTracker.Domain.Enums;

namespace MilestoneTracker.Application.Common.Shared.Models;

public record MediaItem(string FileId, MediaType Type, string? Caption = null);
