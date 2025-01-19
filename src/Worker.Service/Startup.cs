using Amazon.SQS;
using Worker.Service.Services;
using Worker.Service.Services.Interfaces;

namespace Worker.Service
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //var awsOptions = configuration.GetAWSOptions("localstack");
            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddAWSService<IAmazonSQS>();           

            //services.AddSingleton<IMessageReceiver, MessageReceiver>();
            services.AddSingleton<IMessageReceiver, MessageReceiver>(sp =>
            {
                var sqsClient = sp.GetRequiredService<IAmazonSQS>();
                var queueUrl = configuration["SQS:QueueUrl"];
                return new MessageReceiver(sqsClient, queueUrl);
            });

            
            services.AddScoped<IMessageProcessor, MessageProcessor>();

            services.AddSingleton<IMessageProcessed, MessageProcessedNoOp>();

            services.AddHostedService<Worker>();
        }
    }
}
