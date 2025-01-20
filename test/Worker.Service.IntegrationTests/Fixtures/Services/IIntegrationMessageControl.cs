using Amazon.SQS.Model;
using Worker.Service.Services.Interfaces;

namespace Worker.Service.IntegrationTests.Fixtures.Services
{
    internal interface IIntegrationMessageControl : IMessageProcessed
    { 
        (string, SendMessageRequest) CreateIntegrationMessage(string queueUrl, string messageBody, string groupId);

        Task WaitForMessagesProcessedAsync(IEnumerable<string> messageIds, CancellationToken cancellationToken);
    }
}
