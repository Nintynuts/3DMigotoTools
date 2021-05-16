using System.IO;
using System.Threading;

namespace Migoto.Log.Converter
{
    static class IOHelpers
    {
        public static StreamWriter? TryWriteFile(string fileName, IUserInterface? ui = null)
        {
            StreamWriter? writer = null;
            do
                try
                {
                    var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                    writer = new StreamWriter(stream);
                }
                catch (IOException) { AlertAndWait(fileName, ui); }
            while (writer == null && ui?.WaitForContinue() != false);
            return writer;
        }

        public static StreamReader? TryReadFile(string fileName, IUserInterface? ui = null)
        {
            StreamReader? reader = null;
            do
                try
                {
                    var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
                    reader = new StreamReader(stream);
                }
                catch (IOException) { AlertAndWait(fileName, ui); }
            while (reader == null && ui?.WaitForContinue() != false);
            return reader;
        }

        private static void AlertAndWait(string fileName, IUserInterface? ui)
        {
            if (ui != null)
                ui.Status($"File: {fileName} in use, please close it.");

            Thread.Sleep(100);
        }
    }
}
