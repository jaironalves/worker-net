using Worker.Service.Services.Interfaces;

namespace Worker.Service.IntegrationTests.Fixtures.Services
{
    internal interface IIntegrationMessageControl : IMessageProcessed
    {
        void AddMessage(string messageId, string messageBody);
        Task WaitForMessagesToBeProcessedAsync(IEnumerable<string> messageIds, CancellationToken cancellationToken);
    }
}
