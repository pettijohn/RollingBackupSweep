// Interfaces to wrap FileInfo for unit testing


public class DirectoryLister
{
    public DirectoryLister(DirectoryInfo directory, bool concrete = true)
    {
        Concrete = concrete;
        Directory = directory;
        if(!Concrete)
            MockFileList = new List<IFileInfo>();
    }
    public bool Concrete { get; init; }
    public DirectoryInfo Directory { get; init; }
    public List<IFileInfo>? MockFileList { get; init;}

    public IEnumerable<IFileInfo> EnumerateFiles()
    {
        if(Concrete)
            return Directory.EnumerateFiles().Select(f => new ConcreteFileInfo(f));
        else
            return MockFileList!; //will always have value if Concrete is false
    }
}

public interface IFileInfo
{
    string Name { get; }
    void Delete();
}

public class MockFileInfo : IFileInfo
{
    public MockFileInfo(string name, List<IFileInfo> containingDirectory)
    {
        Name = name;
        ContainingDirectory = containingDirectory;
    }
    
    public string Name { get; init; }
    public List<IFileInfo> ContainingDirectory { get; init; }
    public void Delete()
    {
        ContainingDirectory.Remove(this);
    }
}

public class ConcreteFileInfo : IFileInfo
{
    public ConcreteFileInfo(FileInfo fileInfo)
    {
        Value = fileInfo;
    }
    public FileInfo Value { get; init; }

    public string Name => Value.Name;
    public void Delete() => Value.Delete();
}