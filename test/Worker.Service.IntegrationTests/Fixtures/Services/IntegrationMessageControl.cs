using Amazon.SQS;
using Amazon.SQS.Model;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Worker.Service.Services.Interfaces;

namespace Worker.Service.IntegrationTests.Fixtures.Services
{
    internal class IntegrationMessageControl : IIntegrationMessageControl
    {
        public IntegrationMessageControl()
        {
            Id = Guid.NewGuid().ToString();    
        }        

        private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> taskCompletationSources = new();

        public string Id { get; }

        private void AddMessageTaskCompletationSource(string id, string messageBody)
        {
            var tcs = new TaskCompletionSource<bool>();
            taskCompletationSources.AddOrUpdate(id, tcs, (_, _) => tcs);
            //taskCompletationSources[id] = tcs;
            Console.WriteLine($"Message {id} - {messageBody} CreateTaskSource - {Id}");
        }

        public (string, SendMessageRequest) CreateIntegrationMessage(
            string queueUrl, string messageBody, string groupId)
        {
            var integrationId = Guid.NewGuid().ToString();
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody,
                MessageGroupId = groupId,
                MessageDeduplicationId = Guid.NewGuid().ToString(),
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "IntegrationId", new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = integrationId
                        }
                    }
                }
            };
            AddMessageTaskCompletationSource(integrationId, messageBody);
            return (integrationId, sendMessageRequest);
        }

        public Task NotifyAsync(Message message)
        {
            Console.WriteLine($"Message {message.MessageId} - {message.Body} notify - {Id}");

            if (message.MessageAttributes.TryGetValue("IntegrationId", out var integrationIdAttribute))
            {
                var integrationId = integrationIdAttribute.StringValue;
                Console.WriteLine($"Extracted IntegrationId: {integrationId}");

                if (taskCompletationSources.TryGetValue(integrationId, out var tcs))
                {
                    Console.WriteLine($"Message {integrationId} - {message.Body} SetResult - True");
                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult(true);
                }
                else
                {
                    Console.WriteLine($"Message {integrationId} - {message.Body} Completation - NotFound");
                }
            }
            else
            {
                Console.WriteLine($"Message {message.MessageId} - {message.Body} IntegrationId not found");
            }

            return Task.CompletedTask;
        }

        public async Task WaitForMessagesProcessedAsync(IEnumerable<string> integrationIds
            , CancellationToken cancellationToken)
        {
            var taskSources = integrationIds.Select(integrationId =>
            {
                if (!taskCompletationSources.TryGetValue(integrationId, out var tcs))                
                    throw new InvalidOperationException($"Message {integrationId} not registred on integration");                

                return tcs;
            });

            using (cancellationToken.Register(() =>
            {
                foreach (var tcs in taskSources)                
                    tcs.TrySetCanceled();                
            }))
            {
                var tasks = taskSources.Select(tcs => tcs.Task).ToArray();
                await Task.WhenAll(tasks);
            }
        }
    }
}
