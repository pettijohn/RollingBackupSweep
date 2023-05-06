public record SweepParams
{
    public required string BackupDirectory { get; init; }
    public required int DailySnapshotsToKeep = 7;
    public required int WeeklySnapshotsToKeep = 3;
    public required int MonthlySnapshotsToKeep = 4;
    public required bool DryRun = false;
}