using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

internal class Program
{
    private static void Main(string[] args)
    {
        // Runtime parameters TODO config or CLI
        var backupDirectory = @"./testdata";
        var dailySnapshotsToKeep = 30;
        var weeklySnapshotsToKeep = 10;
        var monthlySnapshotsToKeep = 24;
        var dryRun = true;

        //var today = DateTime.Now.Date; // Start from the newest file regardless of today

        var dailySnapshots = (new DirectoryInfo(backupDirectory)).EnumerateFiles();
        var sortedSnapshots = new SortedDictionary<DateTime, FileInfo>();
        var keeperSnapshots = new SortedDictionary<DateTime, FileInfo>();

        foreach (var singleSnapshot in dailySnapshots)
        {
            var m = Regex.Match(singleSnapshot.Name, @"(\d{4}-\d{2}-\d{2})");
            if (m.Success)
            {
                var snapshotDate = DateTime.ParseExact(m.Groups[1].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                sortedSnapshots.Add(snapshotDate, singleSnapshot);
            }
        }

        // Get the newest snapshot date (last item in the collection)
        var newest = sortedSnapshots.Last().Key;
        var i = 0;
        // Keep the daily snapshots
        while (i < dailySnapshotsToKeep)
        {
            var dayToEvaluate = newest.AddDays(-1 * i);
            FileInfo? v;
            var found = sortedSnapshots.TryGetValue(dayToEvaluate, out v);
            if (found)
            {
                keeperSnapshots.Add(dayToEvaluate, v!);
                sortedSnapshots.Remove(dayToEvaluate);
            }
            i++;
        }

        // foreach (var sortedKey in sortedSnapshots.Reverse())
        // {
        //     Console.WriteLine(sortedKey.Key.ToLongDateString());
        //     Console.WriteLine(sortedKey.Value.FullName);
        //     Console.WriteLine();
        // }

    }
}