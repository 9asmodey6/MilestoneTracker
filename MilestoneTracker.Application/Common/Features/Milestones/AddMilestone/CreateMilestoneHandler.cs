namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone;

using System.Text;
using System.Text.Json;
using Domain.Entities.Milestones;
using Domain.Enums;
using Exceptions;
using Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Bot.Keyboards;
using Shared.Interfaces.Repositories;
using Telegram.Bot.Types;

public class CreateMilestoneHandler(
    IMilestoneRepository repository,
    IUserStateService userStateService,
    ITelegramMessageService messageService,
    ILogger<CreateMilestoneHandler> logger) : IRequestHandler<CreateMilestoneCommand, int>
{
    public async Task<int> Handle(CreateMilestoneCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Creating milestone. Title: {Title}, CreatorId: {CreatorId}, ParentId: {ChildId}",
            command.Title,
            command.CreatorId,
            command.ChildId);
        int milestoneId = 0;
        
        try
        {
            var milestone = new Milestone
            {
                Title = command.Title,
                ChildId = command.ChildId,
                Description = command.Description,
                CreatorId = command.CreatorId,
                OccurredAt = command.OccuredAt.ToDateTime(TimeOnly.MinValue),
                CreatedAt = DateTime.UtcNow,
                MediaFiles = command.MediaFiles ?? new List<MilestoneMedia>(),
            };

            milestoneId = await repository.AddAsync(milestone, cancellationToken);
        }
        catch (ValidationException ex)
        {
            var errorMessage = new StringBuilder("⚠️ <b>Ошибки валидации:</b>\n\n");

            foreach (var errorGroup in ex.Errors)
            {
                foreach (var error in errorGroup.Value)
                {
                    errorMessage.AppendLine($"• {error}");
                }
            }

            errorMessage.AppendLine("\nПожалуйста, исправьте данные и попробуйте снова.");

            await messageService.SendTextMessageAsync(command.ChatId, errorMessage.ToString(), ct: cancellationToken);
            await messageService.SendMessageWithInlineKeyboardAsync(
                command.ChatId,
                "📋 <b>Воспоминание не прошло проверку.</b>\n\n" +
                "Пожалуйста, посмотри на ошибки выше, нажми на нужный пункт меню и исправь данные. " +
                "Как закончишь — попробуй сохранить еще раз! 👇",
                BotKeyboards.MilestoneConfirmationKeyboard(),
                cancellationToken);
            
            var data = new CreateMilestoneData(
                ChatId: command.ChatId,
                CreatorId: command.CreatorId,
                ChildId: command.ChildId,
                ChildName: command.ChildName,
                Category: command.Category,
                Date: command.OccuredAt,
                Title: command.Title,
                Description: command.Description,
                MediaGroup: MapToTelegramMedia(command.MediaFiles)
                );
            
            await userStateService.UpdateAsync(command.ChatId ,UserStateType.AddMilestoneConfirming, data, cancellationToken);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize StateData for ChatId: {ChatId}", command.CreatorId);

            await messageService.SendTextMessageAsync(
                command.ChatId,
                "❌ Произошла ошибка. Попробуй начать заново с кнопки '➕ Добавить ребёнка'",
                ct: cancellationToken);

            await userStateService.ResetAsync(command.ChatId, cancellationToken);
        }

        return milestoneId;
    }
    
    private List<IAlbumInputMedia> MapToTelegramMedia(List<MilestoneMedia>? domainMedia)
    {
        if (domainMedia == null || domainMedia.Count == 0) 
            return new List<IAlbumInputMedia>();

        return domainMedia.Select(m => 
        {
            var inputFile = InputFile.FromFileId(m.FileId);

            return m.Type switch
            {
                MediaType.Photo => (IAlbumInputMedia)new InputMediaPhoto(inputFile)
                {
                    Caption = m.Caption,
                    ParseMode = Telegram.Bot.Types.Enums.ParseMode.Html
                },
                MediaType.Video => (IAlbumInputMedia)new InputMediaVideo(inputFile)
                {
                    Caption = m.Caption,
                    ParseMode = Telegram.Bot.Types.Enums.ParseMode.Html
                },
                _ => throw new InvalidOperationException($"Тип медиа {m.Type} не поддерживается для отправки")
            };
        }).ToList();
    }
}

