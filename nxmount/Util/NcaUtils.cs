using LibHac.Fs;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem.NcaUtils;

namespace nxmount.Util
{
    public static class NcaUtils
    {
        public static (Nca, Nca?) Unwrap(this SwitchFsNca nca)
        {
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

        public static bool HasTitleKey(this Nca? nca)
        {
            /* This is counterintuitive, but this simplifies the logic for validation of mounting. */
            if (nca == null)
                return true;

            /* If this NCA doesn't have a rights ID, we have all the keys we need. */
            var header = nca.Header;
            if (!header.HasRightsId)
                return true;

            /* Finally, check if we have the title key for this rights ID. */
            var hasKey = Keyset.Instance.ExternalKeySet.Contains(new RightsId(header.RightsId));
            return hasKey;
        }


        public static bool HasTitleKey(this SwitchFsNca fsnca)
        {
            var basee = fsnca.BaseNca;
            var nca = fsnca.Nca;

            return basee.HasTitleKey() && nca.HasTitleKey();
        }
    }
}
