using Amazon.SQS.Model;
using Worker.Service.Services.Interfaces;

namespace Worker.Service.Services
{
    internal class MessageProcessedNoOp : IMessageProcessed
    {
        public Task NotifyAsync(Message message)
        {   
            return Task.CompletedTask;
        }
    }
}
