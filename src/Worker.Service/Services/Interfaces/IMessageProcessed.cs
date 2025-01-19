using Amazon.SQS.Model;

namespace Worker.Service.Services.Interfaces
{
    public interface IMessageProcessed
    {
        Task NotifyAsync(Message message);
    }
}
