using System;
using System.IO;

namespace Migoto.Log.Converter
{
    static class IOHelpers
    {
        public static StreamWriter TryOpenFile(string fileName)
        {
            StreamWriter output = null;
            do
            {
                try
                {
                    output = new StreamWriter(fileName);
                }
                catch (IOException)
                {
                    Console.WriteLine($"File: {fileName} in use, please close it and press any key to continue");
                }
            } while (output == null && Console.ReadKey() != null);
            return output;
        }

        public static bool GetValidPath(string prompt, Func<string, bool> validate, string invalidMsg, out string validPath, string path = "")
        {
            while (path != "" || ConsoleEx.ReadLineOrEsc($"Please enter path of {prompt}: ", out path))
            {
                path = path.Replace("\"", "");
                if (validate(path))
                {
                    validPath = path;
                    return true;
                }
                Console.WriteLine($"{invalidMsg}, please try again...");
                path = "";
            }
            validPath = null;
            return false;
        }
    }

    static class ConsoleEx
    {
        public static bool ReadLineOrEsc(string message, out string result)
        {
            Console.Write(message);

            result = "";
            int curIndex = 0;
            var stroke = Console.ReadKey(true);
            while (stroke.Key != ConsoleKey.Escape && stroke.Key != ConsoleKey.Enter)
            {
                switch (stroke.Key)
                {
                    case ConsoleKey.Backspace:
                        if (curIndex > 0)
                        {
                            result = result.Remove(result.Length - 1);
                            Console.Write(stroke.KeyChar);
                            Console.Write(' ');
                            Console.Write(stroke.KeyChar);
                            curIndex--;
                        }
                        break;
                    default:
                        if (!char.IsControl(stroke.KeyChar))
                        {
                            result += stroke.KeyChar;
                            Console.Write(stroke.KeyChar);
                            curIndex++;
                        }
                        break;
                }
                stroke = Console.ReadKey(true);
            }
            ClearLine();
            return stroke.Key != ConsoleKey.Escape;
        }

        public static void ClearLine()
        {
            int currentLine = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLine);
        }
    }
}
