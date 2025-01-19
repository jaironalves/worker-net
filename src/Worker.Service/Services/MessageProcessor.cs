
using Amazon.SQS.Model;
using Worker.Service.Services.Interfaces;

namespace Worker.Service.Services
{
    internal class MessageProcessor : IMessageProcessor
    {
        public Task ProcessAsync(Message message)
        {
            Console.WriteLine($"Message {message.MessageId} - {message.Body} processed");
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }        
    }
}
