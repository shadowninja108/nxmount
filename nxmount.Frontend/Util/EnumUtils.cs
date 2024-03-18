using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace nxmount.Frontend.Util
{
    public static class EnumUtils
    {
        public static EnumDescription ToDescription(this Enum value)
        {
            string description;
            string help = null;

            var attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Any())
            {
                description = (attributes.First() as DescriptionAttribute)?.Description;
            }
            else
            {
                TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
                description = ti.ToTitleCase(ti.ToLower(value.ToString().Replace("_", " ")));
            }

            if (description.IndexOf(';') is var index && index != -1)
            {
                help = description.Substring(index + 1);
                description = description.Substring(0, index);
            }

            return new EnumDescription() { Value = value, Description = description, Help = help };
        }

        public static T? Parse<T>(string input) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("Generic Type 'T' must be an Enum.");
            }
            if (string.IsNullOrEmpty(input)) return null;
            if (Enum.GetNames(typeof(T)).Any(
                    e => e.Trim().ToUpperInvariant() == input.Trim().ToUpperInvariant()))
            {
                return (T)Enum.Parse(typeof(T), input, true);
            }
            return null;
        }
    }
}
