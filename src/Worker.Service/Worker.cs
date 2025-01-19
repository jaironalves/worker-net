using Amazon.SQS.Model;
using Worker.Service.Services.Interfaces;

namespace Worker.Service
{
    public class Worker : BackgroundService
    {        
        private readonly IServiceProvider serviceProvider;
        private readonly IMessageReceiver messageReceiver;
        private readonly ILogger<Worker> logger;      

        public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
        {
            this.serviceProvider = serviceProvider;
            messageReceiver = serviceProvider.GetRequiredService<IMessageReceiver>();
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var messages = await messageReceiver.ReceiveAsync();

                    if (messages.Any())
                    {
                        Console.WriteLine($"Processing {messages.Count()} messages");
                        await ProcessMessagesAsync(messages);
                    }
                    else
                    {
                        await Task.Delay(1000, stoppingToken);
                    }

                    logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                catch (Exception ex) 
                {
                    logger.LogError(ex, "An error occurred while processing messages.");
                }                
            }
        }

        private async Task ProcessMessagesAsync(IEnumerable<Message> messages)
        {
            using var scope = serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IMessageProcessor>();
            var processed = scope.ServiceProvider.GetRequiredService<IMessageProcessed>();

            foreach (var message in messages)
            {
                try
                {
                    await processor.ProcessAsync(message);
                }
                catch (Exception)
                {

                    
                }
                finally
                {                    
                    await processed.NotifyAsync(message);
                }                
            }
        }
    }
}
