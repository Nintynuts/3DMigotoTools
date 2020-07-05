using System.IO;
using System.Threading;

namespace Migoto.Log.Converter
{
    static class IOHelpers
    {
        public static StreamWriter TryWriteFile(string fileName, IUserInterface ui = null)
        {
            StreamWriter writer = null;
            do
                try { writer = new StreamWriter(fileName); } catch (IOException) { AlertOrWait(fileName, ui); }
            while (writer == null && ui?.WaitForContinue() != false);
            return writer;
        }

        public static StreamReader TryReadFile(string fileName, IUserInterface ui = null)
        {
            StreamReader reader = null;
            do
                try { reader = new StreamReader(fileName); } catch (IOException) { AlertOrWait(fileName, ui); }
            while (reader == null && ui?.WaitForContinue() != false);
            return reader;
        }

        private static void AlertOrWait(string fileName, IUserInterface ui)
        {
            if (ui != null)
                ui.Status($"File: {fileName} in use, please close it.");
            else
                Thread.Sleep(100);
        }
    }
}
