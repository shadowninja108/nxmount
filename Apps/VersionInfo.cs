using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibHac.Fs.Fsa;
using LibHac.Ncm;
using LibHac.Ns;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using LibHac.Util;
using nxmount.Util;

namespace nxmount.Apps
{
    public class VersionInfo
    {
        public enum SourceType
        {
            Patch,
            Aoc,
        }

        private readonly ApplicationInfo App;
        private readonly int? SourceIndex;
        private readonly SourceType Source;

        public VersionInfo(ApplicationInfo app, int? sourceIndex, SourceType source)
        {
            App = app;
            SourceIndex = sourceIndex;
            Source = source;

            if (Source == SourceType.Aoc && sourceIndex == null)
                throw new Exception("Aoc must have a source index!");
        }

        private List<TitleInfo> TitleSources
        {
            get
            {
                return Source switch
                {
                    SourceType.Patch => App.Updates,
                    SourceType.Aoc => App.Aoc,

                    _ => throw new NotImplementedException()
                };
            }
        }

        private TitleInfo Title
        {
            get
            {
                if (SourceIndex == null)
                    return App.Base;

                return TitleSources[SourceIndex.Value];
            }
        }


        public string BaseTitle => App.DisplayTitle;
        public string DisplayVersion => Title.Control.DisplayVersionString.ToString();

        /*
        public IEnumerable<ContentType> ContentTypes
        {
            get
            {
                var baseContents = Base.Metadata.ContentEntries.Select(x => x.Type);
                if (Update != null)
                {
                    return Update.Metadata.ContentEntries.Select(x => x.Type).Union(baseContents);
                }
                else
                {
                    return baseContents;
                }
            }
        }

        public (SwitchFsNca, SwitchFsNca?)? GetNcas(ContentType type)
        {
            var baseEntry = Base.Metadata.ContentEntries.FirstOrDefault(x => x.Type == type);
            if (baseEntry == null)
                return null;

            var baseNca = Base.Ncas.First(x => x.NcaId == baseEntry.NcaId.ToHexString().ToLower());


            SwitchFsNca? updateNca = null;

            var updateEntry = Update?.Metadata.ContentEntries.FirstOrDefault(x => x.Type == type);

            if (updateEntry != null)
            {
                updateNca = Update?.Ncas.First(x => x.NcaId == updateEntry.NcaId.ToHexString().ToLower());
            }

            return (baseNca, updateNca);
        }

        public IEnumerable<NcaSectionType> GetSections(ContentType type)
        {
            var pair = GetNcas(type);
            if(pair == null)
                yield break;

            var (baseNca, updateNca) = pair!.Value;

            foreach (var section in Enum.GetValuesAsUnderlyingType<NcaSectionType>().Cast<NcaSectionType>())
            {
                var baseExists = baseNca.Nca.SectionExists(section);
                var updateExists = updateNca?.Nca.SectionExists(section) ?? false;
                if(!baseExists)
                    continue;

                // var baseIsSparse = baseNca.Nca.GetFsHeader((int)section).ExistsSparseLayer();
                // if (baseIsSparse  && !updateExists)
                //     continue;

                yield return section;
            }
        }

        public IFileSystem? OpenFilesystem(NcaSectionType section, ContentType type)
        {
            var pair = GetNcas(type);
            if (pair == null)
                return null;

            var (baseNca, updateNca) = pair!.Value;

            if (updateNca != null && !updateNca.Nca.SectionExists(section))
                updateNca = null;

            // var baseIsSparse = baseNca.Nca.GetFsHeader((int)section).ExistsSparseLayer();
            // if (baseIsSparse && updateNca == null)
            //     return null;

            if (updateNca != null)
                return baseNca.Nca.OpenFileSystemWithPatch(updateNca.Nca, section, IntegrityCheckLevel.ErrorOnInvalid);
            else
                return baseNca.Nca.OpenFileSystem(section, IntegrityCheckLevel.ErrorOnInvalid);
        }


        public string GetTitle(ApplicationLanguage desiredLanguage)
        {
            return UpdateOrBase.Control.Value.GetTitle(desiredLanguage).NameString.ToString();
        }
        
        */
        public override string ToString() => $"{BaseTitle} ({DisplayVersion})";
    }
}
