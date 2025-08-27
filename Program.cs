using TimeMachine.Models;

namespace TimeMachine;

class Program
{
    static async Task Main(string[] args)
    {
        Repositories.Timer timer = new();
        Clock firstClock = new(true, 50), secondClock = new(false, 100);
        while (true)
        {
            string? input = Console.ReadLine();
            if (!String.IsNullOrEmpty(input))
            {
                if (input == "play")
                {
                    timer.Continue();
                }
                else if(input == "stop")
                {
                    timer.Pause();
                }
            }
        }
    }
}
