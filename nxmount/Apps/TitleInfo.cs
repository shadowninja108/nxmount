using System.Collections.Immutable;
using LibHac.Fs.Fsa;
using LibHac.Ns;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using LibHac.Util;
using nxmount.Util;
using ContentType = LibHac.Ncm.ContentType;

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

        public struct ContentKey(ContentType type, byte id)
        {
            public ContentType Type = type;
            public byte Id = id;
        }

        internal Title Impl;

        private Dictionary<string, SwitchFsNca> NcaIdLookup = new();
        private Dictionary<ContentType, byte[]> OffsetLookup = new();
        private Dictionary<ContentKey, SwitchFsNca> ContentAndIdLookup = new();
        private List<SwitchFsNca> DeltaNcasImpl = [];
        public TitleType Type { get; internal init;  }

        public TitleInfo(Title title, TitleType titleType)
        {
            Impl = title;
            Type = titleType;

            foreach (var nca in title.Ncas)
            {
                if(!NcaIdLookup.TryAdd(nca.NcaId, nca))
                    Console.WriteLine($"Warning: Duplicate NCA ID {nca.NcaId}");
            }

            var types = new HashSet<ContentType>();
            foreach (var entry in Impl.Metadata.ContentEntries)
            {
                var nca = GetNcaById(entry.NcaId);

                if (entry.Type == ContentType.DeltaFragment)
                {
                    DeltaNcasImpl.Add(nca);
                    continue;
                }

                if (!ContentAndIdLookup.TryAdd(new ContentKey(entry.Type, entry.IdOffset), nca))
                {
                    Console.WriteLine($"Warning: Duplicate NCA Content in {title}?");
                }
                types.Add(entry.Type);
            }

            foreach(var type in types)
                OffsetLookup[type] = ContentAndIdLookup
                    .Where(x => x.Key.Type == type)
                    .Select(x => x.Key.Id)
                    .ToArray();
        }

        public string DisplayVersion
        {
            get
            {
                if (Type == TitleType.Aoc)
                {
                    var id = (Impl.Id - Impl.Metadata.ApplicationTitleId - 0x1000);
                    return $"{id} v{Impl.Version}";
                }

                return StringUtils.Utf8ZToString(Impl.Control.Value.DisplayVersion);
            }
        }

        public string GetTitle(ApplicationLanguage desiredLanguage)
        {
            return Impl.Control.Value.GetTitle(desiredLanguage).NameString.ToString();
        }

        private byte[]? GetOffsetsByContentType(ContentType type)
        {
            return OffsetLookup.GetValueOrDefault(type);
        }

        public SwitchFsNca? GetNcaByContentKey(ContentKey key)
        {
            return ContentAndIdLookup.GetValueOrDefault(key);
        }
        public bool HaveKeysForNca(ContentKey content)
        {
            /* Try to get the NCAs for this content type. */
            var pair = GetNcaByContentKey(content);
            if (pair == null)
                return false;

            var (baseNca, updateNca) = pair.Unwrap();

            return baseNca.HasTitleKey() && updateNca.HasTitleKey();
        }

        public bool IsSectionMissingBase(ContentKey content, NcaSectionType section)
        {
            /* Try to get the NCAs for this content type. */
            var pair = GetNcaByContentKey(content);
            if (pair == null)
                return false;

            var (baseNca, updateNca) = pair.Unwrap();

            /* See if this section exists in base and what the section index would be. */
            var baseExists = baseNca.SectionExists(section);
            baseExists &= Nca.TryGetSectionIndexFromType(section, baseNca.Header.ContentType, out var baseSectionIndex);

            /* Check if the update exists and if the section exists in it. */
            var updateExists = updateNca?.SectionExists(section) ?? false;

            /* Check if the base's section is sparse, if it exists. */
            NcaFsHeader? baseHeader = baseExists ? baseNca.GetFsHeader(baseSectionIndex) : null;
            var baseIsPatch = baseHeader?.IsPatchSection() ?? false;
            var baseIsSparse = baseHeader?.ExistsSparseLayer() ?? false;

            return baseIsPatch || (!baseExists && updateExists);
        }

        private bool IsSectionValidToMount(ContentKey content, NcaSectionType section)
        {
            if (!HaveKeysForNca(content))
                return false;

            /* Try to get the NCAs for this content type. */
            var pair = GetNcaByContentKey(content);
            if(pair == null) 
                return false;

            var (baseNca, updateNca) = pair.Unwrap();

            /* See if this section exists in base and what the section index would be. */
            var baseExists = baseNca.SectionExists(section);
            baseExists &= Nca.TryGetSectionIndexFromType(section, baseNca.Header.ContentType, out var baseSectionIndex);

            /* Check if the update exists and if the section exists in it. */
            var updateExists = updateNca?.SectionExists(section) ?? false;

            /* Check if the base's section is sparse, if it exists. */
            NcaFsHeader? baseHeader = baseExists ? baseNca.GetFsHeader(baseSectionIndex) : null;
            var baseIsPatch = baseHeader?.IsPatchSection() ?? false;
            var baseIsSparse = baseHeader?.ExistsSparseLayer() ?? false;

            var valid = true;
            /* Either the base or update must have the section. */
            valid &= baseExists || updateExists;
            /* Base can't be a patch. */
            valid &= !baseIsPatch;
            /* If the base's section is sparse, we must have the update for it. */
            if(baseIsSparse)
                valid &= updateExists;

            return valid;
        }

        public ImmutableArray<byte> GetContentIdsByType(ContentType type)
        {
            if (type != ContentType.DeltaFragment)
                return OffsetLookup[type].ToImmutableArray();

            return Enumerable.Range(0, DeltaNcas.Count).Select(x => (byte) x).ToImmutableArray();
        }

        public IEnumerable<NcaSectionType> GetSections(ContentKey content)
        {
            if (!HaveKeysForNca(content))
                yield break;

            foreach (var section in Enum.GetValuesAsUnderlyingType<NcaSectionType>().Cast<NcaSectionType>())
            {
                /* If we are missing the base, we just always want to show it (to indicate it exists, but we can't mount it). */
                /* Otherwise, hide it if we can't otherwise mount it. */
                if(!IsSectionMissingBase(content, section) && !IsSectionValidToMount(content, section))
                    continue;

                yield return section;
            }
        }

        public IFileSystem? OpenFilesystem(ContentKey content, NcaSectionType section)
        {
            SwitchFsNca? nca;
            if (content.Type != ContentType.DeltaFragment)
            {
                nca = GetNcaByContentKey(content);
                if (nca == null)
                    return null;
            }
            else
            {
                nca = GetNcaById(Impl.Metadata.ExtendedData.DeltaContents[content.Id].NcaId);
            }


            var (baseNca, updateNca) = nca.Unwrap();
            var baseExists = baseNca.SectionExists(section);
            var updateExists = updateNca?.SectionExists(section) ?? false;

            /* Does the update exist and have this section? */
            if (updateExists)
            {
                /* Handle case where only the update has this section. */
                if (!baseExists)
                    return updateNca!.OpenFileSystem(section, IntegrityCheckLevel.ErrorOnInvalid);

                /* Open patched filesystem. */
                return baseNca.OpenFileSystemWithPatch(updateNca, section, IntegrityCheckLevel.ErrorOnInvalid);
            }
            /* Does the section only exist in the base? */
            if(baseExists)
            {
                /* Open base filesystem. */
                return baseNca.OpenFileSystem(section, IntegrityCheckLevel.ErrorOnInvalid);
            }

            /* Section doesn't exist at all. */
            return null;
        }

        public IEnumerable<ContentType> ContentTypes
        {
            get
            {
                if (DeltaNcas.IsEmpty)
                    return OffsetLookup.Keys;
                else
                    return OffsetLookup.Keys.Append(ContentType.DeltaFragment);
            }
        }
        public IEnumerable<ContentKey> ContentKeys => ContentAndIdLookup.Keys;
        public ImmutableList<SwitchFsNca> DeltaNcas => DeltaNcasImpl.ToImmutableList();
        public ref ApplicationControlProperty Control => ref Impl.Control.Value;

        public SwitchFsNca? GetNcaById(string id) => NcaIdLookup.GetValueOrDefault(id);
        public SwitchFsNca? GetNcaById(byte[] id) => GetNcaById(id.ToHexString().ToLower());
    }
}
