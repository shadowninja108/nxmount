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
    public class TitleInfo
    {
        public enum TitleType
        {
            Base,
            Patch,
            Aoc
        }

        internal Title Impl;

        private Dictionary<string, SwitchFsNca> NcaIdLookup = new();
        private Dictionary<ContentType, SwitchFsNca> ContentTypeLookup = new();
        private List<SwitchFsNca> DeltaNcas = new List<SwitchFsNca>();
        public TitleType Type { get; internal init;  }

        public TitleInfo(Title title, TitleType titleType)
        {
            Impl = title;
            Type = titleType;

            foreach (var nca in title.Ncas)
            {
                NcaIdLookup[nca.NcaId] = nca;
            }

            foreach (var entry in Impl.Metadata.ContentEntries)
            {
                var nca = GetNcaById(entry.NcaId);
                if (entry.Type == ContentType.DeltaFragment)
                {
                    DeltaNcas.Add(nca!);
                    continue;
                }

                ContentTypeLookup.Add(entry.Type, GetNcaById(entry.NcaId)!);
            }
        }

        public string DisplayVersion
        {
            get
            {
                return StringUtils.Utf8ZToString(Impl.Control.Value.DisplayVersion);
            }
        }

        public string GetTitle(ApplicationLanguage desiredLanguage)
        {
            return Impl.Control.Value.GetTitle(desiredLanguage).NameString.ToString();
        }

        private (Nca, Nca?)? GetNcasByContentType(ContentType content)
        {
            var nca = ContentTypeLookup.GetValueOrDefault(content);
            if (nca == null)
                return null;

            Nca baseNca;
            Nca? updateNca;

            if (nca.BaseNca == null)
            {
                baseNca = nca.Nca;
                updateNca = null;
            }
            else
            {
                baseNca = nca.BaseNca;
                updateNca = nca.Nca;
            }

            return (baseNca, updateNca);
        }

        private bool IsSectionValidToMount(ContentType content, NcaSectionType section)
        {
            var pair = GetNcasByContentType(content);
            if(pair == null) 
                return false;

            var (baseNca, updateNca) = pair.Value;
            var baseExists = baseNca.SectionExists(section);
            // var updateExists = updateNca?.SectionExists(section) ?? false;
            // var baseIsSparse = baseNca.GetFsHeader((int)section).ExistsSparseLayer();

            var valid = true;
            valid &= baseExists;
            // valid &= baseIsSparse && !updateExists;
            return valid;
        }

        public IEnumerable<NcaSectionType> GetSections(ContentType content)
        {
            foreach (var section in Enum.GetValuesAsUnderlyingType<NcaSectionType>().Cast<NcaSectionType>())
            {
                if(!IsSectionValidToMount(content, section))
                    continue;

                yield return section;
            }
        }

        public IFileSystem? OpenFilesystem(ContentType content, NcaSectionType section)
        {
            var nca = ContentTypeLookup.GetValueOrDefault(content);
            if (nca == null)
                return null;

            return nca.OpenFileSystem(section, IntegrityCheckLevel.ErrorOnInvalid);
        }

        public IEnumerable<ContentType> ContentTypes => ContentTypeLookup.Keys;
        public ref ApplicationControlProperty Control => ref Impl.Control.Value;

        public SwitchFsNca? GetNcaById(string id) => NcaIdLookup.GetValueOrDefault(id);
        public SwitchFsNca? GetNcaById(byte[] id) => GetNcaById(id.ToHexString().ToLower());
    }
}
