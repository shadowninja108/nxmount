using System.Collections.Concurrent;
using nxmount.Driver.Interfaces;
using nxmount.Util;

namespace nxmount.Driver.Handlers
{
    public abstract class CachedFsNodeHandler : IDirectoryHandler
    {
        public abstract string FileName { get; }
        public IDirectoryHandler? Parent { get; }

        public CachedFsNodeHandler(IDirectoryHandler? parent)
        {
            Parent = parent;
        }

        private ConcurrentDictionary<string, IFsNodeHandler> HandlerCache = new();

        public virtual string FilterFileName(string name)
        {
            return PathUtils.CleanupFilename(name);
        }

        private IFsNodeHandler? GetHandler(string key)
        {
            if (HandlerCache.TryGetValue(key, out var handler))
                return handler;

            handler = GetNodeHandlerImpl(key);
            if (handler == null)
                return handler;

            HandlerCache[key] = handler;
            return handler;
        }

        public bool QueryFileInfo(Context context, out IFileInfo info)
        {
            info = context.CreateNewFileInfo(true);
            info.FileName = PathUtils.CleanupFilename(FileName);
            return true;
        }

        public IEnumerable<IFileInfo> QueryChildren(Context context, ReadOnlySpan<string> pathComponents)
        {
            if (pathComponents.IsEmpty)
            {
                return QueryThisDirectory(context);
            }

            var nextPath = pathComponents[1..];
            var handler = GetHandler(pathComponents[0]);

            return handler switch
            {
                IDirectoryHandler dir => dir.QueryChildren(context, nextPath),
                _ => throw new QueryFailedException()
            };
        }

        public bool QueryNode(Context context, ReadOnlySpan<string> pathComponents, out IFsNodeHandler? info)
        {
            Console.WriteLine($"Query: {string.Join(',', pathComponents.ToArray())}");
            info = null;

            if (pathComponents.IsEmpty)
            {
                info = this;
                return true;
            }

            var nextPath = pathComponents[1..];
            var handler = GetHandler(pathComponents[0]);
            if (handler == null)
                return false;

            if (handler is IDirectoryHandler && nextPath.IsEmpty)
            {
                info = handler;
                return true;
            }

            if (handler is IDirectoryHandler dir)
            {
                return dir.QueryNode(context, nextPath, out info);
            }
            if (handler is IFileNodeHandler file)
            {
                info = file;
                return true;
            }

            return false;
        }

        public void DisposeChild(IFsNodeHandler handlerToDispose)
        {
            string? foundKey = null;
            foreach (var (key, handler) in HandlerCache)
            {
                if (handler == handlerToDispose)
                {
                    foundKey = key;
                    break;
                }
            }

            if (foundKey != null)
                HandlerCache.TryRemove(foundKey, out _);
        }

        public void Dispose()
        {
            HandlerCache.Clear();
        }

        public abstract IEnumerable<IFileInfo> QueryThisDirectory(Context context);
        public abstract IFsNodeHandler? GetNodeHandlerImpl(string name);

    }
}
