
namespace RollingBackupSweep; 

internal class Program
{
    private static void Main(string[] args)
    {
        var today = DateTime.Now.Date;
        Generate.BaselineTestData(today);
        Sweeper.Sweep();

        for (int i = 1; i <= 80; i++)
        {
            Console.Write("Press any key to continue");
            Console.ReadLine();

            Generate.SingleDay(today.AddDays(i));
            Sweeper.Sweep();
        }
        
    }
}