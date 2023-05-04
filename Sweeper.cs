using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
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
        var windowOldest = newest;
        var dayToEvaluate = newest;
        var windowMostRecent = newest;

        // Now that we've found the files, run the algorithm. 
        /*  
            Assume keep 1 day, keep 1 week, keep 1 month. 
            Today is 2023-05-04. Keep 1 of 1 day. 
            Look back 2023-05-03 to 2023-04-27. Keep 4/27.
            Look back 2023-04-26 to 2023-03-30. (using 28 days for month). Keep 3/30
        */
        // Look at window ranges - 1, 7, or 30 days
        // Keep the oldest file in each window - so count backwards 
        var windowSizes = new WindowSizer(
            new SingleWindow(1, dailySnapshotsToKeep),
            new SingleWindow(7, weeklySnapshotsToKeep),
            new SingleWindow(28, monthlySnapshotsToKeep)
        ).Windows;

        foreach (var windowSize in windowSizes)
        {
            windowOldest = dayToEvaluate.AddDays(-1 * (windowSize - 1));
            windowMostRecent = dayToEvaluate;
            Console.WriteLine($"Window {windowSize} " + windowOldest.ToString("yyyy-MM-dd")
                + " to " + windowMostRecent.ToString("yyyy-MM-dd"));
            //dayToEvaluate = windowOldest;

            var foundOne = false;
            for (int i = 0; i < windowSize; i++)
            {
                dayToEvaluate = windowOldest.AddDays(i);
                if (foundOne)
                {
                    FileInfo? toDelete;
                    if (foundSnapshots.TryGetValue(dayToEvaluate, out toDelete))
                    {
                        // Console.WriteLine($"DELE WINDOW SIZE {windowSize}: {dayToEvaluate.ToString("yyyy-MM-dd")}");
                    }
                    continue;
                }
                FileInfo? toKeep;
                if (foundSnapshots.TryGetValue(dayToEvaluate, out toKeep))
                {
                    Console.WriteLine($"keep window size {windowSize}: {dayToEvaluate.ToString("yyyy-MM-dd")}");
                    keeperSnapshots.Add(dayToEvaluate, toKeep!);
                    foundSnapshots.Remove(dayToEvaluate);
                    foundOne = true;
                }
                else
                {
                    // Console.WriteLine();
                }
            }

            // Done with this window, advance (older) to the next window
            dayToEvaluate = windowOldest.AddDays(-1);
        }

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