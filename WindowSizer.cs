
namespace RollingBackupSweep {
    public record SingleWindow(int WindowSize, int Repeats);

    /// <summary>
    /// Flatten a Window Expression (sequence of SingleWindows)
    /// into a list of window sizes. 
    /// </summary>
    public class WindowSizer
    {
        // TODO - parse a string from CLI
        public WindowSizer(params SingleWindow[] windows)
        {
            this.Windows = new List<int>();
            foreach (var window in windows)
            {
                for (int i = 0; i < window.Repeats; i++)
                {
                    this.Windows.Add(window.WindowSize);
                }
            }
        }

        public List<int> Windows { get; private set; }
    }
}