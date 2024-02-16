using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace nxmount.Util
{
    public class PathUtils
    {
        private static Regex InvalidFilenameCharsRegex = CreateRegexFromChars(Path.GetInvalidFileNameChars());
        private static Regex InvalidPathCharsRegex = CreateRegexFromChars(Path.GetInvalidPathChars());
        private static Regex SplitCharsRegex = CreateRegexFromChars("/\\");

        private static Regex CreateRegexFromChars(char[] chars)
        {
            return new Regex($"[{Regex.Escape(new string(chars))}]");
        }
        private static Regex CreateRegexFromChars(string chars)
        {
            return new Regex($"[{Regex.Escape(chars)}]");
        }

        public static void Split(out Span<string> split, string path)
        {
            split = Regex.Split(path[1..], $"[{Regex.Escape("/\\")}]");
            if (split.Length == 1 && split[0].Length == 0)
                split = Span<string>.Empty;
        }

        public static string CleanupPath(string str)
        {
            str = InvalidPathCharsRegex.Replace(str, "");
            var split = str.Split('/');
            if (split.Length > 0 && split[0].Length == 0)
                split = split[1..];

            return string.Join('/', split.Select(x => InvalidFilenameCharsRegex.Replace(x, "")));
        }

        public static string CleanupFilename(string str)
        {
            return InvalidFilenameCharsRegex.Replace(str, "");
        }
    }
}
