using System.CommandLine;
using System.Net.Quic;

namespace RollingBackupSweep; 

public enum VerbosityOptions
{
    Quiet = 1,
    Minimal = 2,
    Normal = 3,
    Detailed = 4,
    Diagnostic = 5
}

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
        var verbosityOption = new Option<VerbosityOptions>(name: "--verbosity", description: "Output detail level.");
        verbosityOption.SetDefaultValue(VerbosityOptions.Normal);
        var verboseOption = new Option<bool>(name: "--verbose", description: "Verbose output. Equivalent to --verbosity Diagnostic.");


        var rootCommand = new RootCommand("Sweep backup snapshots with yyyy-MM-dd in the filename, retaining the specified number of daily, weekly, and monthly snapshots.");
        rootCommand.AddOption(pathOption);
        rootCommand.AddOption(daysOption);
        rootCommand.AddOption(weeksOption);
        rootCommand.AddOption(monthsOption);
        rootCommand.AddOption(dryRunOption);
        rootCommand.AddOption(verbosityOption);
        rootCommand.AddOption(verboseOption);

        rootCommand.SetHandler<DirectoryInfo, int, int, int, bool, VerbosityOptions, bool>((path, days, weeks, months, dryRun, verbosityOption, verbose) => 
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
                Verbosity = verbose ? VerbosityOptions.Diagnostic : verbosityOption
        };
            new Sweeper(sweepParams).Sweep();
        }, 
        pathOption, daysOption, weeksOption, monthsOption, dryRunOption, verbosityOption, verboseOption);

        return rootCommand.Invoke(args);
    }
}