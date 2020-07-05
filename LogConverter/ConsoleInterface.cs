using System;
using System.IO;
using System.Timers;

namespace Migoto.Log.Converter
{
    using static Console;

    public interface IUserInterface
    {
        bool GetInfo(string prompt, out string info);

        bool GetFile(string prompt, ref string file);

        void Event(string message);

        void Status(string message);

        void WaitForCancel(string message);

        bool WaitForContinue();
    }

    internal class ConsoleInterface : IUserInterface
    {
        private readonly string wait = @"|/-\";
        private int waitCycle;
        private const char nbsp = ' ';
        private int statusLine = 0;
        private static string hintMessage;

        public ConsoleInterface()
        {
        }

        private string EmptyLine => new string(' ', WindowWidth - CursorLeft);

        private bool ReadAnythingExceptEsc => ReadKey(true).Key != ConsoleKey.Escape;

        private void ClearLine()
        {
            int currentLine = CursorTop;
            while (currentLine >= statusLine)
            {
                SetCursorPosition(0, currentLine);
                Write(EmptyLine);
                currentLine--;
            }
            SetCursorPosition(0, statusLine);
        }

        private void Hint(string msg = null)
        {
            var currentColumn = CursorLeft;
            WriteLine();
            if (msg != null && hintMessage != null && msg != hintMessage)
                SetCursorPosition(hintMessage.Length + 1, CursorTop);
            var newMsg = msg ?? EmptyLine;
            Write(newMsg);
            SetCursorPosition(currentColumn, CursorTop - 1);
            hintMessage = msg;
        }

        private bool Request(string message, ref string result, ref int curIndex, bool skipPrompt)
        {
            if (!skipPrompt)
            {
                Status($"Please enter {message}:");
                if (result != "")
                {
                    //int right = WindowWidth - CursorLeft;
                    var toPrint = result;
                    //if (toPrint.Length > right)
                    //    toPrint = ".." + toPrint.Substring(toPrint.IndexOf("\\", toPrint.Length - (right - 2)));
                    Write(toPrint);
                    curIndex = toPrint.Length;
                }
                Hint("Press Escape to cancel");
            }

            CursorVisible = true;
            var stroke = ReadKey(true);
            while (stroke.Key != ConsoleKey.Escape && stroke.Key != ConsoleKey.Enter)
            {
                switch (stroke.Key)
                {
                    case ConsoleKey.Backspace:
                        if (curIndex > 0)
                        {
                            result = result.Remove(result.Length - 1);
                            Write(stroke.KeyChar);
                            Write(' ');
                            Write(stroke.KeyChar);
                            curIndex--;
                        }
                        break;
                    default:
                        if (!char.IsControl(stroke.KeyChar))
                        {
                            result += stroke.KeyChar;
                            Write(stroke.Key == ConsoleKey.Spacebar ? nbsp : stroke.KeyChar);
                            curIndex++;
                        }
                        break;
                }
                stroke = ReadKey(true);
            }
            curIndex = result.Length;
            Hint();
            return stroke.Key != ConsoleKey.Escape;
        }

        public void Event(string message)
        {
            ClearLine();
            WriteLine(message);
            statusLine = CursorTop;
            ClearLine();
            Hint(hintMessage);
        }

        public void Status(string message)
        {
            ClearLine();
            statusLine = CursorTop;
            Write(message + nbsp);
        }

        public bool GetInfo(string message, out string result)
        {
            int curIndex = 0;
            result = "";
            return Request(message, ref result, ref curIndex, false);
        }

        public bool GetFile(string prompt, ref string path)
        {
            ClearLine();
            bool checkedInput = false;
            int curIndex = 0;
            while ((path != "" && !checkedInput) || Request($"path of {prompt}", ref path, ref curIndex, curIndex > 0))
            {
                path = path.Replace("\"", "").Trim();
                if (File.Exists(path))
                {
                    Hint();
                    return true;
                }
                checkedInput = true;
                Hint("File doesn't exist, please try again.");
            }
            path = null;
            return false;
        }

        public void WaitForCancel(string message)
        {
            var timer = new Timer(200) { AutoReset = true };
            timer.Elapsed += WaitSymbol;
            timer.Start();
            CursorVisible = false;
            Hint($"{message}. Press Escape to cancel");
            while (ReadAnythingExceptEsc)
                continue;
            Hint();
            CursorVisible = true;
            timer.Stop();
            timer.Elapsed -= WaitSymbol;
        }

        private void WaitSymbol(object sender, ElapsedEventArgs e)
        {
            SetCursorPosition(0, CursorTop);
            waitCycle++;
            waitCycle %= 4;
            ClearLine();
            Write(wait[waitCycle]);
        }

        public bool WaitForContinue()
        {
            Hint("Press Escape to cancel or any other key to continue...");
            var result = ReadAnythingExceptEsc;
            Hint();
            return result;
        }
    }
}
