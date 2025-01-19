namespace Worker.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    var startup = new Startup(context.Configuration);
                    startup.ConfigureServices(services);
                })
                .Build();

            host.Run();
        }        
    }
}