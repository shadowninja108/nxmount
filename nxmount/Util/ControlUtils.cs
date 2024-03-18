using LibHac.Ns;

namespace nxmount.Util
{
    public static class ControlUtils
    {
        public static bool HasLanguage(this ref ApplicationControlProperty control, ApplicationLanguage language)
        {
            return !control.Title[(int)language].NameString.IsEmpty();
        }

        public static ref ApplicationControlProperty.ApplicationTitle GetTitle(this ref ApplicationControlProperty control, ApplicationLanguage desiredLanguage)
        {
            /* Try to get desired language. */
            if (control.HasLanguage(desiredLanguage))
            {
                return ref control.Title[(int)desiredLanguage];
            }

            /* Otherwise, just find *some* title. */
            for (var i = 0; i < (int)ApplicationLanguage.End; i++)
            {
                if(control.HasLanguage((ApplicationLanguage) i))
                    return ref control.Title[i];
            }

            throw new Exception("Control has no titles?");
        }
    }
}
