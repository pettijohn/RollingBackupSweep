using System.CommandLine;

namespace RollingBackupSweep; 

internal class Program
{
    private static int Main(string[] args)
    {

        //var sweepParams = new SweepParams();
        var pathOption = new Option<DirectoryInfo>(name: "--path", description: "Path to sweep of backup snapshots.") { IsRequired = true };
        var daysOption = new Option<int>(name: "--days", description: "Number of daily backup snapshots to retain.") { IsRequired = true };
        var weeksOption = new Option<int>(name: "--weeks", description: "Number of weekly backup snapshots to retain.") { IsRequired = true };
        var monthsOption = new Option<int>(name: "--months", description: "Number of monthly backup snapshots to retain.") { IsRequired = true };
        var dryRunOption = new Option<bool>(name: "--dry-run", description: "Dry run; don't delete anything.");
        var verboseOption = new Option<bool>(name: "--verbose", description: "Verbose output.");


        var rootCommand = new RootCommand("Sweep backup snapshots with yyyy-MM-dd in the filename, retaining the specified number of daily, weekly, and monthly snapshots.");
        rootCommand.AddOption(pathOption);
        rootCommand.AddOption(daysOption);
        rootCommand.AddOption(weeksOption);
        rootCommand.AddOption(monthsOption);
        rootCommand.AddOption(dryRunOption);
        rootCommand.AddOption(verboseOption);

        rootCommand.SetHandler<DirectoryInfo, int, int, int, bool, bool>((path, days, weeks, months, dryRun, verbose) => 
        {
            if(path == null || !path.Exists)
            {
                Console.WriteLine($"Error - Path {path} does not exist.");
                Environment.Exit(-1);
            }
            var sweepParams = new SweepParams()
            {
                BackupPath = path,
                DailySnapshotsToKeep = days,
                WeeklySnapshotsToKeep = weeks,
                MonthlySnapshotsToKeep = months,
                DryRun = dryRun,
                Verbose = verbose
        };
            new Sweeper(sweepParams).Sweep();
        }, 
        pathOption, daysOption, weeksOption, monthsOption, dryRunOption, verboseOption);

        return rootCommand.Invoke(args);

        // .WithParsed(RunOptions)
        // .WithNotParsed(HandleParseError);

        //var sweepParams = result.Value;
        // sweepParams.BackupDirectory = Environment.GetEnvironmentVariable("RBS_PATH") ?? throw new MissingFieldException("Must provide path");
        // if(!Int32.TryParse(Environment.GetEnvironmentVariable("RBS_KEEPDAILY"), out sweepParams.DailySnapshotsToKeep)) throw new MissingFieldException("Must provide daily");
        // if(!Int32.TryParse(Environment.GetEnvironmentVariable("RBS_KEEPWEEKLY"), out sweepParams.WeeklySnapshotsToKeep)) throw new MissingFieldException("Must provide weekly");
        // if(!Int32.TryParse(Environment.GetEnvironmentVariable("RBS_KEEPMONTHLY"), out sweepParams.MonthlySnapshotsToKeep)) throw new MissingFieldException("Must provide monthly");
        // if(!Boolean.TryParse(Environment.GetEnvironmentVariable("RBS_DRYRUN"), out sweepParams.DryRun)) throw new MissingFieldException("Must provide dryrun");
        
        // var d = new DirectoryInfo(sweepParams.BackupPath);
        // if(!d.Exists)
        // {
        //     Console.WriteLine($"Error - directory {sweepParams.BackupPath} does not exist.");
        // }
            
        // new Sweeper(sweepParams).Sweep();
    }
}