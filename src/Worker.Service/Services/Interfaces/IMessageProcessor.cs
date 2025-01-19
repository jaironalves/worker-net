using Amazon.SQS.Model;

namespace Worker.Service.Services.Interfaces
{
    internal interface IMessageProcessor
    {
        Task ProcessAsync(Message message);
    }
}
