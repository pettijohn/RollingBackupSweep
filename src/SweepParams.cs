
public class SweepParams
{
    public DirectoryInfo BackupPath { get; set; }

    public int DailySnapshotsToKeep { get; set; } = Int32.MaxValue;
    
    public int WeeklySnapshotsToKeep { get; set; } = Int32.MaxValue;

    public int MonthlySnapshotsToKeep{ get; set; } = Int32.MaxValue;
    
    public bool DryRun { get; set; }
    public bool Verbose { get; set; }
}