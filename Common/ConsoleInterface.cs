using System.Timers;

namespace System.IO
{
    using static Console;

    public interface IUserInterface
    {
        string? GetInfo(string prompt);

        FileInfo? GetFile(string prompt, string ext);

        T? GetValid<T>(string prompt, Func<string, (T? result, string? msg)> validator) where T : class;

        T? GetValid<T>(string prompt, Func<string, (T? result, string? msg)> validator) where T : struct;

        void Event(string message);

        void Status(string message);

        void WaitForCancel(string message);

        bool WaitForContinue();
    }

    public class ConsoleInterface : IUserInterface
    {
        private readonly string wait = @"|/-\";
        private int waitCycle;
        private const char nbsp = ' ';
        private int statusLine = 0;
        private static string? hintMessage;

        public ConsoleInterface()
        {
        }

        private static string EmptyLine => new string(' ', WindowWidth - CursorLeft);

        private static bool ReadAnythingExceptEsc => ReadKey(true).Key != ConsoleKey.Escape;

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

        private static void Hint(string? msg = null)
        {
            var left = CursorLeft;
            var top = CursorTop;
            WriteLine(); // Move to next line
            if (msg != null && hintMessage != null && msg != hintMessage)
                CursorLeft = hintMessage.Length + 1;
            var newMsg = msg ?? EmptyLine;
            Write(newMsg);
            WriteLine(EmptyLine); // Clear current line after hint
            Write(EmptyLine); // Clear following line (in case hint moved up)
            SetCursorPosition(left, top);
            hintMessage = msg;
        }

        private string? Request(string message, string? initial)
        {
            int index = 0;
            string text = initial ?? string.Empty;
            ClearLine();
            Status($"Please enter {message}:");
            if (text != "")
            {
                printRemaining();
                OffsetCursor(text.Length);
            }
            Hint("Press Escape to cancel");

            index = text.Length;

            void printRemaining()
            {
                CursorVisible = false;
                var left = CursorLeft;
                var top = CursorTop;
                Write(text[index..]);
                Write(EmptyLine); // Overwrite hint if overflow to new line
                Hint(hintMessage); // Reprint hint
                SetCursorPosition(left, top);
                CursorVisible = true;
            }

            void RemoveChar()
            {
                text = text.Remove(index, 1);
                printRemaining();
            }

            bool OffsetCursor(int offset)
            {
                var newIdx = index + offset;
                if (newIdx < 0 || newIdx > text.Length)
                    return false;
                index = newIdx;
                var left = (offset + CursorLeft) % WindowWidth;
                CursorTop += (offset + CursorLeft) / WindowWidth;
                if (left < 0)
                {
                    left += WindowWidth;
                    CursorTop--;
                }
                CursorLeft = left;
                return true;
            }

            CursorVisible = true;
            var stroke = ReadKey(true);
            while (stroke.Key is not ConsoleKey.Escape and not ConsoleKey.Enter)
            {
                switch (stroke.Key)
                {
                    case ConsoleKey.LeftArrow:
                        OffsetCursor(-1); break;
                    case ConsoleKey.RightArrow:
                        OffsetCursor(1); break;
                    case ConsoleKey.Home:
                        OffsetCursor(-index); break;
                    case ConsoleKey.End:
                        OffsetCursor(text.Length - index); break;
                    case ConsoleKey.Backspace:
                        if (OffsetCursor(-1))
                            RemoveChar();
                        break;
                    case ConsoleKey.Delete:
                        if (index < text.Length)
                            RemoveChar();
                        break;
                    default:
                        if (!char.IsControl(stroke.KeyChar))
                        {
                            text = text.Insert(index, $"{stroke.KeyChar}");
                            var atEnd = CursorLeft == WindowWidth - 1;
                            Write(stroke.Key == ConsoleKey.Spacebar ? nbsp : stroke.KeyChar);
                            if (atEnd)
                            {
                                CursorTop += 1;
                                CursorLeft = 0;
                            }
                            index++;
                            printRemaining();
                        }
                        break;
                }
                stroke = ReadKey(true);
            }
            OffsetCursor(text.Length - index); // Put cursor to end
            Hint(); // Clear hint
            return stroke.Key != ConsoleKey.Escape ? text : null;
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

        public string? GetInfo(string message) => Request(message, null);

        public T? GetValid<T>(string prompt, Func<string, (T? result, string? msg)> validator) where T : class
        {
            ClearLine();
            string? input = null;
            while ((input = Request(prompt, input)) is not null)
            {
                var (result, msg) = validator(input.Trim());
                if (result != null && msg == null)
                {
                    Hint();
                    return result;
                }
                Hint($"{msg}, please try again.");
            }
            return default;
        }

        public T? GetValid<T>(string prompt, Func<string, (T? result, string? msg)> validator) where T : struct
        {
            ClearLine();
            string? input = null;
            while ((input = Request(prompt, input)) is not null)
            {
                var (result, msg) = validator(input.Trim());
                if (result != null && msg == null)
                {
                    Hint();
                    return result;
                }
                Hint($"{msg}, please try again.");
            }
            return default;
        }

        public FileInfo? GetFile(string prompt, string ext)
        {
            return GetValid<FileInfo>($"path of {prompt}", path =>
            {
                try { return (IOHelpers.ValidatePath(path, ext), null); }
                catch (InvalidDataException ide) { return (null, ide.Message); }
            });
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
