using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

internal class Program
{
    private static void Main(string[] args)
    {
        // Runtime parameters TODO config or CLI
        var backupDirectory = @"./testdata";
        var dailySnapshotsToKeep = 7;
        var weeklySnapshotsToKeep = 3;
        var monthlySnapshotsToKeep = 4;
        var dryRun = true;

        //var today = DateTime.Now.Date; // Start from the newest file regardless of today

        var dailySnapshots = (new DirectoryInfo(backupDirectory)).EnumerateFiles();
        var sortedSnapshots = new SortedDictionary<DateTime, FileInfo>();
        var keeperSnapshots = new SortedDictionary<DateTime, FileInfo>();
        var deleteSnapshots = new SortedDictionary<DateTime, FileInfo>();

        foreach (var singleSnapshot in dailySnapshots)
        {
            var m = Regex.Match(singleSnapshot.Name, @"(\d{4}-\d{2}-\d{2})");
            if (m.Success)
            {
                var snapshotDate = DateTime.ParseExact(m.Groups[1].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                if (!sortedSnapshots.TryAdd(snapshotDate, singleSnapshot))
                {
                    // TODO - configurable logic. E.g. pick oldest or newest. 
                    Console.WriteLine($"Error - duplicate file for date {snapshotDate}. Please manually delete one.");
                    return;
                }
            }
        }

        // Get the newest snapshot date (last item in the collection)
        var newest = sortedSnapshots.Last().Key;
        var daysAgo = 0;
        
        // Look at window ranges - 1, 7, or 30 days
        // Keep the oldest file in each window - so count backwards 
        var windowSizes = new List<int>();
        for (int i = 0; i < dailySnapshotsToKeep; i++) windowSizes.Add(1);
        for (int i = 0; i < weeklySnapshotsToKeep; i++) windowSizes.Add(7);
        for (int i = 0; i < monthlySnapshotsToKeep; i++) windowSizes.Add(30);
        foreach (var windowSize in windowSizes)
        {
            var foundOne = false;
            for (int i = windowSize - 1; i >= 0; i--)
            {
                var dayToEvaluate = newest.AddDays(-1 * daysAgo - i);
                // Console.Write(dayToEvaluate.ToLongDateString());
                if (foundOne)
                {
                    FileInfo? toDelete;
                    if (sortedSnapshots.TryGetValue(dayToEvaluate, out toDelete))
                    {
                        deleteSnapshots.Add(dayToEvaluate, toDelete!);
                        sortedSnapshots.Remove(dayToEvaluate);
                    }
                    // Console.WriteLine();
                    continue;
                }
                FileInfo? toKeep;
                if (sortedSnapshots.TryGetValue(dayToEvaluate, out toKeep))
                {
                    keeperSnapshots.Add(dayToEvaluate, toKeep!);
                    sortedSnapshots.Remove(dayToEvaluate);
                    foundOne = true;
                }
                else
                {
                    // Console.WriteLine();
                }
            }
            daysAgo += windowSize;
            // Console.WriteLine();
        }

        Console.WriteLine("Snapshots to keep:");
        foreach (var toKeep in keeperSnapshots.Reverse())
        {
            Console.WriteLine(toKeep.Value.Name);
        }
        Console.WriteLine();
        Console.WriteLine("Snapshots to delete:");
        foreach (var toDelete in deleteSnapshots.Reverse())
        {
            Console.WriteLine(toDelete.Value.Name);
        }

        if(dryRun)
        {
            Console.WriteLine("Dry run. Exiting.");
            return;
        }

    }
}