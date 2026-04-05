using Gci409.Application.Generation;

namespace Gci409.Worker;

public sealed class GenerationWorker(IServiceProvider serviceProvider, ILogger<GenerationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = serviceProvider.CreateAsyncScope();
                var generationService = scope.ServiceProvider.GetRequiredService<GenerationService>();
                var processed = await generationService.ProcessNextQueuedAsync(stoppingToken);

                if (processed is null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                logger.LogInformation("Processed generation request {GenerationRequestId} with status {Status}.", processed.Id, processed.Status);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Generation worker cycle failed.");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
