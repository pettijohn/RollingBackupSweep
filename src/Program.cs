
namespace RollingBackupSweep; 

internal class Program
{
    private static void Main(string[] args)
    {
        var sweepParams = new SweepParams
        {
            BackupDirectory = @"./testdata",
            DailySnapshotsToKeep = 7,
            WeeklySnapshotsToKeep = 3,
            MonthlySnapshotsToKeep = 4,
            DryRun = false
        };
        var d = new DirectoryInfo(sweepParams.BackupDirectory);
        if(!d.Exists)
        {
            Console.WriteLine($"Error - directory {sweepParams.BackupDirectory} does not exist.");
        }
            
        new Sweeper(sweepParams).Sweep();
    }
}