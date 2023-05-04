namespace RollingBackupSweep; 

internal static class Generate
{
    public static void BaselineTestData(DateTime today)
    {
        for (int i = 0; i < 400; i++)
        {
            Generate.SingleDay(today.AddDays(-1 * i));
        }
    }

    public static void SingleDay(DateTime forDate)
    {
        var dateString = forDate.ToString("yyyy-MM-dd");
        File.Create($"testdata/myfile-{dateString}-snapshot.bak").Close();
    }
}