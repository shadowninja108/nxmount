using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DokanNet;
using nxmount.Driver.Interfaces;
using nxmount.Util;

namespace nxmount.Driver
{
    public abstract class CachedFsNodeHandler : IDirectoryHandler
    {
        public abstract string FileName { get; }
        public IFsNodeHandler? Parent { get; }

        public CachedFsNodeHandler(IFsNodeHandler? parent)
        {
            Parent = parent;
        }


        private ConcurrentDictionary<string, IFsNodeHandler> HandlerCache = new();

        public virtual string FilterFileName(string name)
        {
            return PathUtils.CleanupFilename(name);
        }

        private IFsNodeHandler? GetDirectoryHandler(string key)
        {
            if(HandlerCache.TryGetValue(key, out var handler))
                return handler;

            handler = GetNodeHandlerImpl(key);
            if (handler == null)
                return handler;

            HandlerCache[key] = handler;
            return handler;
        }

        public bool QueryFileInfo(Context context, ref FileInformation info)
        {
            info.PopulateFileInfo(true);
            info.FileName = PathUtils.CleanupFilename(FileName);
            return true;
        }

        public bool QueryChildren(Context context, ReadOnlySpan<string> pathComponents, ref IList<FileInformation> files)
        {
            if (pathComponents.IsEmpty)
            {
                return QueryThisDirectory(context, ref files);
            }

            var nextPath = pathComponents[1..];
            var handler = GetDirectoryHandler(pathComponents[0]);

            return handler switch
            {
                IDirectoryHandler file => file.QueryChildren(context, nextPath, ref files),
                _ => false
            };
        }

        public bool QueryNode(Context context, ReadOnlySpan<string> pathComponents, out IFsNodeHandler? info)
        {
            info = null;

            if (pathComponents.IsEmpty)
            {
                info = this;
                return true;
            }

            var nextPath = pathComponents[1..];
            var handler = GetDirectoryHandler(pathComponents[0]);
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

        public bool TryGetFile(Context context, ReadOnlySpan<string> pathComponents, out IFileNodeHandler? file)
        {
            throw new NotImplementedException();
        }

        public abstract bool QueryThisDirectory(Context context, ref IList<FileInformation> files);
        public abstract IFsNodeHandler? GetNodeHandlerImpl(string name);
    }
}
