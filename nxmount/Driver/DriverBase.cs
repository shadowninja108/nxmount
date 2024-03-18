using System.Text.RegularExpressions;
using LibHac.Util;
using nxmount.Driver.Handlers;
using nxmount.Driver.Interfaces;
using nxmount.Util;
using Path = LibHac.Fs.Path;

namespace nxmount.Driver
{
    public abstract class DriverBase<TResult>
    {
        public Context Context;
        public NodeListHandler Root;

        public abstract TResult ResultSuccess { get; }
        public abstract TResult ResultFileNotFound { get; }
        public abstract TResult ResultPathNotFound { get; }
        public abstract TResult ResultNotImplemented { get; }

        public DriverBase(Context context)
        {
            Context = context;
            Root = new NodeListHandler(null);
            Root.Initialize([
                new GamesHandler(Root, Context)
            ]);
        }

        protected string? TryNormalizePath(string path)
        {
            path = path.Replace("\\", "/");
            var libhacPath = new Path();
            var normalizationResult = libhacPath.InitializeWithNormalization(StringUtils.StringToUtf8(path));
            if (normalizationResult.IsFailure())
            {
                Console.WriteLine($"Error: {normalizationResult}");
                return null;
            }

            var str = libhacPath.ToString();
            var regex = new Regex($"[{Regex.Escape(new string(System.IO.Path.GetInvalidPathChars()))}]");
            str = regex.Replace(str, "");
            return str;
        }

        public TResult TryGetHandler(out Span<string> split, string fileName, out IFsNodeHandler? handler)
        {
            split = Span<string>.Empty;
            handler = null;

            var path = TryNormalizePath(fileName);
            Console.WriteLine($"Path: {path}");
            if (path == null)
                return ResultPathNotFound;

            PathUtils.Split(out split, path);
            if (!Root.QueryNode(Context, split, out handler))
                return ResultPathNotFound;
            if (handler == null)
                return ResultPathNotFound;

            var depth = 0;
            var search = handler;
            while (search != null)
            {
                depth++;
                search = search.Parent;
            }

            split = split[(depth - 1)..];

            return ResultSuccess;
        }

    }
}
