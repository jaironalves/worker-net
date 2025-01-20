using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Worker.Service.IntegrationTests.Fixtures.Services;
using Worker.Service.Services.Interfaces;

namespace Worker.Service.IntegrationTests.Fixtures
{
    public class WorkerFixture : IAsyncLifetime
    {
        public Worker Worker { get; private set; }
        private IHost host;
        private IIntegrationMessageControl integrationMessageControl;
        private IAmazonSQS sqsClient;
        private string queueUrl;

        public async Task InitializeAsync()
        {
            host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    var startup = new Startup(context.Configuration);
                    startup.ConfigureServices(services);

                    // Remova o registro anterior de IMessageProcessed
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMessageProcessed));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Adicione WorkerTestMessageControl como a implementação de IMessageProcessed
                    services.AddSingleton<IntegrationMessageControl>();
                    services.AddSingleton<IMessageProcessed>(sp => sp.GetRequiredService<IntegrationMessageControl>());
                    services.AddSingleton<IIntegrationMessageControl>(sp => sp.GetRequiredService<IntegrationMessageControl>());
                })
                .Build();

            await host.StartAsync();

            var serviceProvider = host.Services;
            integrationMessageControl = serviceProvider.GetRequiredService<IIntegrationMessageControl>();
            sqsClient = serviceProvider.GetRequiredService<IAmazonSQS>();
            queueUrl = serviceProvider.GetRequiredService<IConfiguration>()["SQS:QueueUrl"];

            await PurgeQueueAsync();
        }

        private async Task PurgeQueueAsync()
        {
            await sqsClient.PurgeQueueAsync(new PurgeQueueRequest
            {
                QueueUrl = queueUrl
            });
        }

        public async Task<string> PostMessageAsync(string messageBody, string groupId)
        {
            var (integrationId, sendMessageRequest) = 
                integrationMessageControl
                    .CreateIntegrationMessage(
                        queueUrl, messageBody, groupId);            
            await sqsClient.SendMessageAsync(sendMessageRequest);            
            return integrationId;
        }

        public async Task WaitConsumeAsync(IEnumerable<string> postedIds, int timeoutInSeconds)
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds));
            var delayTask = Task.Delay(TimeSpan.FromSeconds(timeoutInSeconds), cancellationTokenSource.Token);
            var messageTasks = integrationMessageControl.WaitForMessagesProcessedAsync(postedIds, cancellationTokenSource.Token);

            var completedTask = await Task.WhenAny(messageTasks, delayTask);

            if (completedTask == delayTask)
            {
                throw new TimeoutException("The consume operation has timed out.");
            }

            cancellationTokenSource.Cancel();
            await completedTask; // Ensure any exceptions/cancellations are observed
        }

        public async Task DisposeAsync()
        {
            if (host != null)
            {
                await host.StopAsync();
                host.Dispose();
            }
        }
    }
}
