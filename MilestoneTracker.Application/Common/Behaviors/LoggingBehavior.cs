namespace MilestoneTracker.Application.Common.Behaviors;

using Serilog.Context;
using MediatR;
using Microsoft.Extensions.Logging;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : notnull
{

    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        using (LogContext.PushProperty("CommandName", requestName))
        {
            logger.LogInformation("Processing command {RequestName} with data: {@Request}", 
                requestName, request);

            try
            {
                var response = await next();
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Command {RequestName} failed", requestName);
                throw;
            }
        }
    }
}