using Amazon.SQS;
using Amazon.SQS.Model;
using Worker.Service.Services.Interfaces;

namespace Worker.Service.Services
{
    internal class MessageReceiver : IMessageReceiver
    {
        private readonly IAmazonSQS sqsClient;
        private readonly string queueUrl;

        public MessageReceiver(IAmazonSQS sqsClient, string queueUrl)
        {
            this.sqsClient = sqsClient;
            this.queueUrl = queueUrl;
        }

        public async Task<IEnumerable<Message>> ReceiveAsync()
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 10, // Adjust as needed
                //WaitTimeSeconds = 20, // Long polling
                MessageAttributeNames = new List<string> { "All" }
            };

            var response = await sqsClient.ReceiveMessageAsync(request);
            return response.Messages;
        }
    }
}