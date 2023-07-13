namespace Inspector
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //DateTime a = DateTime.Now;
            //DateTime b = DateTime.Now.AddMinutes(-200);

            //Console.WriteLine(b - a  < TimeSpan.FromMinutes(10) && b-a>TimeSpan.FromSeconds(20));
            //return;

            bool allowedToUse = await Requests.ValidateSuspect(Collectors.Steam.GetSteamIdFromCoPlay());
            if (!allowedToUse)
            {
                Console.WriteLine("You don't have access to this program!");
                return;
            }

            await Collectors.LaunchHistory.Collect();
            await Collectors.Steam.Collect();
            await Collectors.USBDevices.Collect();
        }
    }
}