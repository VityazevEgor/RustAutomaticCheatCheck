namespace Inspector
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            bool allowedToUse = await Requests.ValidateSuspect(Collectors.Steam.GetSteamIdFromCoPlay());
            if (!allowedToUse)
            {
                Console.WriteLine("You don't have access to this program!");
                return;
            }

            await Collectors.LaunchHistory.Collect();
        }
    }
}