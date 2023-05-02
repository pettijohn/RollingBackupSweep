internal class Generate
{
    public static void TestData()
    {
        var today = DateTime.Now.Date;
        for (int i = 0; i > -400; i--)
        {
            var dateString = today.AddDays(i).ToString("yyyy-MM-dd");
            File.Create($"testdata/myfile-{dateString}-snapshot.bak").Close();
        }
    }
}