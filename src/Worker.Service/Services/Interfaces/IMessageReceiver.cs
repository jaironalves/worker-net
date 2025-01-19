using Amazon.SQS.Model;

namespace Worker.Service.Services.Interfaces
{
    internal interface IMessageReceiver
    {
        Task<IEnumerable<Message>> ReceiveAsync();
    }
}
