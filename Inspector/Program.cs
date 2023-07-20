namespace Inspector
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("By running this program, you agree that your data about running programs, USB devices and browser history will be transferred to third parties. (Press on any button to continue)");
            Console.ReadKey();

            bool allowedToUse = await Requests.ValidateSuspect(Collectors.Steam.GetSteamIdFromCoPlay());
            if (!allowedToUse)
            {
                Console.WriteLine("You don't have access to this program!");
                return;
            }

            await Collectors.LaunchHistory.Collect();
            await Collectors.Steam.Collect();
            await Collectors.USBDevices.Collect();
            await Collectors.Chromium.Collect();
            await Collectors.RegistryС.Collect();
        }
    }
}