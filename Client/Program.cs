using Common.Clients;
using Common.Enums;
using Common.Statics;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var clientType = ClientType.Kiosk;
            var serviceProvider = Setup.ConfigureServices();

            if (args.Any())
                Enum.TryParse(args[0], out clientType);

            ConsoleClient client = clientType switch
            {
                ClientType.Master => new MasterClient(serviceProvider),
                ClientType.Manager => new ManagerClient(serviceProvider),
                ClientType.Kiosk => new KioskClient(serviceProvider),
                ClientType.Guide => new GuideClient(serviceProvider),
                _ => throw new ArgumentOutOfRangeException($"{clientType} has no associated client to instantiate.")
            };

            client.Run();
        }
    }
}
