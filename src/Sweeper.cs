using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace RollingBackupSweep;

public class Sweeper
{
    /// <summary>
    /// Create the sweeper with user params. Optionally, provide a mock directory lister for testing.
    /// </summary>
    /// <param name="sweepParams"></param>
    /// <param name="mockDirectoryLister"></param>
    public Sweeper(SweepParams sweepParams, DirectoryLister? mockDirectoryLister = null)
    {
        SweepParams = sweepParams;
        if(mockDirectoryLister == null)
            DirectoryLister = new DirectoryLister(SweepParams.BackupPath, true);
        else
            DirectoryLister = mockDirectoryLister;
    }

    public SweepParams SweepParams { get; init; }
    public DirectoryLister DirectoryLister { get; init; }

    private void Log(string val, bool force = false)
    {
        if(force || SweepParams.Verbose)
            Console.WriteLine(val);
    }
    public int Sweep()
    {
        Log($"About to sweep {SweepParams.BackupPath.FullName}");
        var dailySnapshots = DirectoryLister.EnumerateFiles();
        var foundSnapshots = new SortedDictionary<DateTime, IFileInfo>();
        var keeperSnapshots = new SortedDictionary<DateTime, IFileInfo>();
        bool foundDupes = false;


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
                    Log($"Error - duplicate file for date {m.Groups[1].Value}. Please manually delete one.", true);
                    foundDupes = true;
                }
            }
        }
        if(foundDupes)
            return -1;

        if (foundSnapshots.Count == 0)
        {
            Log("No backups found, exiting. Check your path.", true);
            return 0;
        }
        else
        {
            Log($"Found {foundSnapshots.Count} backup snapshots with yyyy-MM-dd in filename.");
        }

        // Get the newest snapshot date (last item in the collection)
        var newest = foundSnapshots.Last().Key;
        Log($"Today is {newest.ToString("yyyy-MM-dd")}");
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
            new SingleWindow(1, SweepParams.DailySnapshotsToKeep),
            new SingleWindow(7, SweepParams.WeeklySnapshotsToKeep),
            new SingleWindow(28, SweepParams.MonthlySnapshotsToKeep)
        ).Windows;

        foreach (var windowSize in windowSizes)
        {
            windowOldest = dayToEvaluate.AddDays(-1 * (windowSize - 1));
            windowMostRecent = dayToEvaluate;
            Log($"Window {windowSize} " + windowOldest.ToString("yyyy-MM-dd")
                + " to " + windowMostRecent.ToString("yyyy-MM-dd"));
            //dayToEvaluate = windowOldest;

            var foundOne = false;
            for (int i = 0; i < windowSize; i++)
            {
                dayToEvaluate = windowOldest.AddDays(i);
                if (foundOne)
                {
                    IFileInfo? toDelete;
                    if (foundSnapshots.TryGetValue(dayToEvaluate, out toDelete))
                    {
                        Log($"DELE WINDOW SIZE {windowSize}: {dayToEvaluate.ToString("yyyy-MM-dd")}");
                    }
                    continue;
                }
                IFileInfo? toKeep;
                if (foundSnapshots.TryGetValue(dayToEvaluate, out toKeep))
                {
                    Log($"keep window size {windowSize}: {dayToEvaluate.ToString("yyyy-MM-dd")}");
                    keeperSnapshots.Add(dayToEvaluate, toKeep!);
                    foundSnapshots.Remove(dayToEvaluate);
                    foundOne = true;
                }
                else
                {
                    // pass
                }
            }

            // Done with this window, advance (older) to the next window
            dayToEvaluate = windowOldest.AddDays(-1);
        }

        if (SweepParams.DryRun)
        {
            Log("Dry run. Exiting.", true);
            return 0;
        }
        else 
        {
            foreach (var fileToDelete in foundSnapshots)
            {
                fileToDelete.Value.Delete();
            }
            return 0;
        }
    }
}