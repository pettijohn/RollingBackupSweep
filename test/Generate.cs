namespace RollingBackupSweep; 

internal static class Generate
{
    public static void BaselineTestData(DateTime today, List<IFileInfo> directory)
    {
        for (int i = 0; i < 400; i++)
        {
            Generate.SingleDay(today.AddDays(-1 * i), directory);
        }
    }

    public static void SingleDay(DateTime forDate, List<IFileInfo> directory)
    {
        var dateString = forDate.ToString("yyyy-MM-dd");
        directory.Add(new MockFileInfo($"test-service-{dateString}-snapshot.bak", directory));
    }
}