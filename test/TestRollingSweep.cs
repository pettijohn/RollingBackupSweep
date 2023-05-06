using Microsoft.VisualBasic.FileIO;

namespace RollingBackupSweep;

[TestClass]
public class TestRollingSweep
{
    [TestMethod]
    public void TestCoreLogic()
    {
        var today = new DateTime(2023,05,01).Date;
        var dirLister = new DirectoryLister("", false);
        
        // so I don't have to ! everywhere else down the line 
        if(dirLister.MockFileList == null) 
            throw new ArgumentNullException("Test data setup incorrectly");

        Generate.BaselineTestData(today, dirLister.MockFileList);

        Assert.AreEqual(400, dirLister.MockFileList.Count);

        var p = new SweepParams
        {
            BackupDirectory = "",
            DailySnapshotsToKeep = 7,
            WeeklySnapshotsToKeep = 3,
            MonthlySnapshotsToKeep = 4,
            DryRun = false
        };

        var sweeper = new Sweeper(p, dirLister);
        sweeper.Sweep();
        Assert.AreEqual(14, dirLister.MockFileList.Count);

        // Check the 14 files. 7 daily, three weekly, and four monthly (28d)
        foreach (var d in new string[] {
            "2023-05-01",
            "2023-04-30",
            "2023-04-29",
            "2023-04-28",
            "2023-04-27",
            "2023-04-26",
            "2023-04-25",

            "2023-04-18",
            "2023-04-11",
            "2023-04-04",

            "2023-03-07",
            "2023-02-07",
            "2023-01-10",
            "2022-12-13"
        })
        {
            AssertContains(dirLister.MockFileList, d);
        }

        today = today.AddDays(1);
        Generate.SingleDay(today, dirLister.MockFileList);
        sweeper.Sweep();
        // Add 05-02
        // Delete 12-13
        // Note boundaries shift - what was daily before is now weekly (04-25)
        foreach (var d in new string[] {
            "2023-05-02",
            "2023-05-01",
            "2023-04-30",
            "2023-04-29",
            "2023-04-28",
            "2023-04-27",
            "2023-04-26",

            "2023-04-25",
            "2023-04-18",
            "2023-04-11",

            "2023-04-04",
            "2023-03-07",
            "2023-02-07",
            "2023-01-10",
        })
        {
            AssertContains(dirLister.MockFileList, d);
        }
        Assert.AreEqual(14, dirLister.MockFileList.Count);

        today = today.AddDays(1);
        Generate.SingleDay(today, dirLister.MockFileList);
        sweeper.Sweep();
        // Add 05-03
        // Delete 04-26 (drop a daily not needed, b/c it would slip into a week that already has a file)
        foreach (var d in new string[] {
            "2023-05-03",
            "2023-05-02",
            "2023-05-01",
            "2023-04-30",
            "2023-04-29",
            "2023-04-28",
            "2023-04-27",

            "2023-04-25",
            "2023-04-18",
            "2023-04-11",

            "2023-04-04",
            "2023-03-07",
            "2023-02-07",
            "2023-01-10",
        })
        {
            AssertContains(dirLister.MockFileList, d);
        }
        Assert.AreEqual(14, dirLister.MockFileList.Count);

        // Jump ahead a week and validate 
        for (int i = 0; i < 7; i++)
        {
            today = today.AddDays(1);
            Generate.SingleDay(today, dirLister.MockFileList);
            sweeper.Sweep();
        }
        foreach (var d in new string[] {
            "2023-05-10",
            "2023-05-09",
            "2023-05-08",
            "2023-05-07",
            "2023-05-06",
            "2023-05-05",
            "2023-05-04",
            
            "2023-05-02",
            "2023-04-25",
            "2023-04-18",

            "2023-04-04",
            "2023-03-07",
            "2023-02-07",
            "2023-01-10",
        })
        {
            AssertContains(dirLister.MockFileList, d);
        }
        Assert.AreEqual(14, dirLister.MockFileList.Count);

        // Jump ahead 28 days and validate 
        for (int i = 0; i < 28; i++)
        {
            today = today.AddDays(1);
            Generate.SingleDay(today, dirLister.MockFileList);
            sweeper.Sweep();
        }
        foreach (var d in new string[] {
            "2023-06-07",
            "2023-06-06",
            "2023-06-05",
            "2023-06-04",
            "2023-06-03",
            "2023-06-02",
            "2023-06-01",
            
            "2023-05-30",
            "2023-05-23",
            "2023-05-16",

            "2023-05-02",
            "2023-04-04",
            "2023-03-07",
            "2023-02-07",
        })
        {
            AssertContains(dirLister.MockFileList, d);
        }
        Assert.AreEqual(14, dirLister.MockFileList.Count);
    }

    private void AssertContains(List<IFileInfo> dir, string substring)
    {
        Assert.IsTrue(dir.Count(f => f.Name.Contains(substring)) > 0);
    }

    // Duplicate files for given day

    /// <summary>
    /// More than one file with given date string - abort the whole thing.
    /// </summary>
    [TestMethod]
    public void DuplicateFilesForDay()
    {
        var today = new DateTime(2023, 05, 01).Date;
        var dirLister = new DirectoryLister("", false);

        // so I don't have to ! everywhere else down the line 
        if (dirLister.MockFileList == null)
            throw new ArgumentNullException("Test data setup incorrectly");

        Generate.SingleDay(today, dirLister.MockFileList);
        Generate.SingleDay(today, dirLister.MockFileList);
        Generate.SingleDay(today, dirLister.MockFileList);

        var p = new SweepParams
        {
            BackupDirectory = "",
            DailySnapshotsToKeep = 7,
            WeeklySnapshotsToKeep = 3,
            MonthlySnapshotsToKeep = 4,
            DryRun = false
        };

        var sweeper = new Sweeper(p, dirLister);
        Assert.AreEqual(-1, sweeper.Sweep());
        Assert.AreEqual(3, dirLister.MockFileList.Count);
    }
    // Directory not found
    // Missed days 

}