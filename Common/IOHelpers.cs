using System.Threading;

namespace System.IO
{
    public static class IOHelpers
    {
        public static FileInfo File(this DirectoryInfo dir, string fileName) => new FileInfo(Path.Combine(dir.FullName, fileName));

        public static DirectoryInfo SubDirectory(this DirectoryInfo dir, string dirName) => new DirectoryInfo(Path.Combine(dir.FullName, dirName));

        public static FileSystemEventHandler Handler<T>(Action<T> logic) where T : FileSystemInfo
        {
            return (object _, FileSystemEventArgs e) =>
            {
                if ((IO.File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory)
                    ? new DirectoryInfo(e.FullPath) as T
                    : new FileInfo(e.FullPath) as T) is { } file)
                    logic(file);
            };
        }

        public static FileInfo ChangeExt(this FileInfo file, string newExtension)
            => file.Directory!.File(file.Name.Replace(file.Extension, newExtension));

        public static StreamWriter? TryOpenWrite(this FileInfo file, IUserInterface? ui = null)
        {
            StreamWriter? writer = null;
            do
                try
                {
                    var stream = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                    writer = new StreamWriter(stream);
                }
                catch (IOException) { AlertAndWait(file.FullName, ui); }
            while (writer == null && ui?.WaitForContinue() != false);
            return writer;
        }

        public static StreamReader? TryOpenRead(this FileInfo file, IUserInterface? ui = null)
        {
            StreamReader? reader = null;
            do
                try
                {
                    var stream = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
                    reader = new StreamReader(stream);
                }
                catch (IOException) { AlertAndWait(file.FullName, ui); }
            while (reader == null && ui?.WaitForContinue() != false);
            return reader;
        }

        private static void AlertAndWait(string fileName, IUserInterface? ui)
        {
            if (ui != null)
                ui.Status($"File: {fileName} in use, please close it.");

            Thread.Sleep(100);
        }

        public static string GetDirectoryName(string path)
            => Path.GetDirectoryName(path) ?? throw new InvalidDataException($"File \"{path}\" may not be located on the root of a file system.");
    }
}
