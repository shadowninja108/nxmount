using System.Text.RegularExpressions;

namespace nxmount.Util
{
    public class PathUtils
    {
        private static readonly Regex InvalidFilenameCharsRegex = CreateRegexFromChars(Path.GetInvalidFileNameChars());
        private static readonly Regex InvalidPathCharsRegex = CreateRegexFromChars(Path.GetInvalidPathChars());
        private static readonly Regex SplitCharsRegex = CreateRegexFromChars("/\\");

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
            split = SplitCharsRegex.Split(path[1..]);
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
