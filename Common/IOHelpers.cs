namespace System.IO;

public static class IOHelpers
{
    public static FileInfo File(this DirectoryInfo dir, string fileName) => new(Path.Combine(dir.FullName, fileName));

    public static DirectoryInfo SubDirectory(this DirectoryInfo dir, string dirName) => new(Path.Combine(dir.FullName, dirName));

    public static FileSystemEventHandler Handler<T>(Action<T> logic) where T : FileSystemInfo
    {
        return (object _, FileSystemEventArgs e) =>
        {
            if ((IO.File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory) ? new DirectoryInfo(e.FullPath) as T : new FileInfo(e.FullPath) as T) is T file)
                logic(file);
        };
    }

    public static FileInfo SuffixName(this FileInfo file, string suffix)
        => file.Directory!.File($"{Path.GetFileNameWithoutExtension(file.FullName)}{suffix}{file.Extension}");

    public static FileInfo ChangeExt(this FileInfo file, string newExtension)
        => file.Directory!.File(file.Name.Replace(file.Extension, newExtension));

    public static FileInfo? ValidatePath(string path, string ext, bool @throw = true)
    {
        var illegal = new Regex("[\"*/<>?|]");

        path = illegal.Replace(path, "").Trim();
        var file = new FileInfo(path);
        return !file.Exists ? @throw ? throw new InvalidDataException("File doesn't exist") : null
             : !path.EndsWith(ext) ? @throw ? throw new InvalidDataException("File has wrong extension") : null
             : file;
    }

    public static StreamWriter? TryOpenWrite(this FileInfo file, IUserInterface? ui = null)
    {
        StreamWriter? writer = null;
        do
        {
            try
            {
                var stream = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                writer = new StreamWriter(stream);
            }
            catch (IOException) { AlertAndWait(file.FullName, ui); }
        }
        while (writer == null && ui?.WaitForContinue() != false);
        return writer;
    }

    public static StreamReader? TryOpenRead(this FileInfo file, IUserInterface? ui = null)
    {
        FileStream? stream = null;
        StreamReader? reader = null;
        do
        {
            try
            {
                stream = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
                reader = new StreamReader(stream);
            }
            catch (IOException) { AlertAndWait(file.FullName, ui); }
        }
        while (!(stream?.Length > 0) && ui?.WaitForContinue() != false);

        return reader;
    }

    private static void AlertAndWait(string fileName, IUserInterface? ui)
    {
        ui?.Status($"File: {fileName} in use, please close it.");

        Thread.Sleep(500);
    }

    public static string GetDirectoryName(string path)
        => Path.GetDirectoryName(path) ?? throw new InvalidDataException($"File \"{path}\" may not be located on the root of a file system.");
}
