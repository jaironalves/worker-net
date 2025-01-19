using Amazon.SQS.Model;
using System.Collections.Concurrent;
using Worker.Service.Services.Interfaces;

namespace Worker.Service.IntegrationTests.Fixtures.Services
{
    internal class IntegrationMessageControl : IIntegrationMessageControl
    {
        public IntegrationMessageControl()
        {
            Id = Guid.NewGuid().ToString();    
        }

        //private readonly BlockingCollection<Message> messagesProcessed = new();
        private readonly BlockingCollection<string> messagesToProcess = new();

        private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> messageCompletionSources = new();

        public string Id { get; }

        public void AddMessage(string id, string messageBody)
        {
            messagesToProcess.Add(id);            

            var tcs = new TaskCompletionSource<bool>();
            messageCompletionSources.AddOrUpdate(id, tcs, (_, _) => tcs);
            messageCompletionSources[id] = tcs;
            Console.WriteLine($"Message {id} - {messageBody} CreateTaskSource - {Id}");
        }

        public Task NotifyAsync(Message message)
        {
            Console.WriteLine($"Message {message.MessageId} - {message.Body} notify - {Id}");
            //messagesProcessed.Add(message);
            if (messageCompletionSources.TryGetValue(message.MessageId, out var tcs))
            {
                Console.WriteLine($"Message {message.MessageId} - {message.Body} SetResult - True");
                if (!tcs.Task.IsCompleted)
                    tcs.SetResult(true);
                                
            }
            else
            {
                Console.WriteLine($"Message {message.MessageId} - {message.Body} Completation - NotFound");
            }
            return Task.CompletedTask;
        }

        public async Task WaitForMessagesToBeProcessedAsync(IEnumerable<string> messageIds, CancellationToken cancellationToken)
        {
            var tasks = messageIds.Select(messageId =>
            {
                if (!messageCompletionSources.TryGetValue(messageId, out var tcs))
                {
                    tcs = new TaskCompletionSource<bool>();
                    messageCompletionSources[messageId] = tcs;
                }
                return tcs.Task;
            }).ToArray();

            using (cancellationToken.Register(() =>
            {
                //foreach (var tcs in messageCompletionSources.Values)
                //{
                //    tcs.TrySetCanceled();
                //}
            }))
            {
                await Task.WhenAll(tasks);
            }
        }
    }
}
