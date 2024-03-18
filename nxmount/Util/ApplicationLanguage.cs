using System.ComponentModel;

namespace nxmount.Util
{
    public enum ApplicationLanguage
    {
        [Description("American English")]
        AmericanEnglish         = 0,
        [Description("British English")]
        BritishEnglish          = 1,
        [Description("Japanese")]
        Japanese                = 2,
        [Description("French")]
        French                  = 3,
        [Description("German")]
        German                  = 4,
        [Description("Latin American Spanish")]
        LatinAmericanSpanish    = 5,
        [Description("Spanish")]
        Spanish                 = 6,
        [Description("Italian")]
        Italian                 = 7,
        [Description("Dutch")]
        Dutch                   = 8,
        [Description("Canadian French")]
        CanadianFrench          = 9,
        [Description("Portuguese")]
        Portuguese              = 10,
        [Description("Russian")]
        Russian                 = 11,
        [Description("Korean")]
        Korean                  = 12,
        [Description("Traditional Chinese")]
        TraditionalChinese      = 13,
        [Description("Simplified Chinese")]
        SimplifiedChinese       = 14,
        End = 15,
    }
}
