using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace RollingBackupSweep;

public static class Sweeper
{
    public static void Sweep()
    {
        // Runtime parameters TODO config or CLI
        var backupDirectory = @"./testdata";
        var dailySnapshotsToKeep = 7;
        var weeklySnapshotsToKeep = 3;
        var monthlySnapshotsToKeep = 4;
        var dryRun = false;;

        //var today = DateTime.Now.Date; // Start from the newest file regardless of today

        var dailySnapshots = (new DirectoryInfo(backupDirectory)).EnumerateFiles();
        var foundSnapshots = new SortedDictionary<DateTime, FileInfo>();
        var keeperSnapshots = new SortedDictionary<DateTime, FileInfo>();
        // var deleteSnapshots = new SortedDictionary<DateTime, FileInfo>();

        // Parse date from filenames 
        foreach (var singleSnapshot in dailySnapshots)
        {
            var m = Regex.Match(singleSnapshot.Name, @"(\d{4}-\d{2}-\d{2})");
            if (m.Success)
            {
                var snapshotDate = DateTime.ParseExact(m.Groups[1].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                if (!foundSnapshots.TryAdd(snapshotDate, singleSnapshot))
                {
                    // TODO - configurable logic. E.g. pick oldest or newest. 
                    Console.WriteLine($"Error - duplicate file for date {snapshotDate}. Please manually delete one.");
                    return;
                }
            }
        }

        // Get the newest snapshot date (last item in the collection)
        var newest = foundSnapshots.Last().Key;
        Console.WriteLine($"Today is {newest}");
        var daysAgo = 0;

        // Look at window ranges - 1, 7, or 30 days
        // Keep the oldest file in each window - so count backwards 
        var windowSizes = new WindowSizer(
            new SingleWindow(1, dailySnapshotsToKeep),
            new SingleWindow(7, weeklySnapshotsToKeep),
            new SingleWindow(30, monthlySnapshotsToKeep)
        ).Windows;

        foreach (var windowSize in windowSizes)
        {
            daysAgo += windowSize;

            var foundOne = false;
            for (int i = windowSize; i > 0; i--)
            {
                var dayToEvaluate = newest.AddDays(-1 * daysAgo + i);
                // Console.Write(dayToEvaluate.ToLongDateString());
                if (foundOne)
                {
                    FileInfo? toDelete;
                    if (foundSnapshots.TryGetValue(dayToEvaluate, out toDelete))
                    {
                        Console.WriteLine($"DELE WINDOW SIZE {windowSize}: {dayToEvaluate}");
                    }
                    continue;
                }
                FileInfo? toKeep;
                if (foundSnapshots.TryGetValue(dayToEvaluate, out toKeep))
                {
                    Console.WriteLine($"keep window size {windowSize}: {dayToEvaluate}");
                    keeperSnapshots.Add(dayToEvaluate, toKeep!);
                    foundSnapshots.Remove(dayToEvaluate);
                    foundOne = true;
                }
                else
                {
                    // Console.WriteLine();
                }
            }
            // Console.WriteLine();
        }

        // Console.WriteLine("Snapshots to keep:");
        // foreach (var toKeep in keeperSnapshots.Reverse())
        // {
        //     Console.WriteLine(toKeep.Value.Name);
        // }
        // Console.WriteLine();
        // Console.WriteLine("Snapshots to delete:");
        // foreach (var toDelete in foundSnapshots.Reverse())
        // {
        //     Console.WriteLine(toDelete.Value.Name);
        // }

        if (dryRun)
        {
            Console.WriteLine("Dry run. Exiting.");
            return;
        }
        else 
        {
            foreach (var fileToDelete in foundSnapshots)
            {
                fileToDelete.Value.Delete();
            }
        }
    }
}