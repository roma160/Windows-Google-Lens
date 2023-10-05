using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Windows_Google_Lens.Lens
{
    public static class Providers
    {
        public static PUploadGResultProvider GoogleLens = new PUploadGResultProvider(
            "Google Lens", "https://lens.google.com/v3/upload",
            new Dictionary<String, String> { { "ep", "subb" }, {"re", "df"} },
            PUploadGResultProvider.ImageEncodingType.Raw,
            "stcs", "encoded_image", 200,
            (self, text) =>
            {
                MatchCollection matches = Regex.Matches(text,
                    @"\/search\?(.(?!\\u0026p\\u003d))*.\\u0026p\\u003d((.(?!\\u0026))+.)");
                if (matches.Count < 1) return null;
                return $"https://lens.google.com/search?p={matches[0].Groups[2].Value}";
            }
        );

        public static PUploadGResultProvider MicrosoftBing = new PUploadGResultProvider(
            "Microsoft Bing", "https://www.bing.com/images/detail/search",
            new Dictionary<String, String> { { "iss", "sbiupload" }, { "FORM", "ANCMS1" } },
            PUploadGResultProvider.ImageEncodingType.Base64,
            null, "imageBin", 302,
            (self, text) =>
            {
                MatchCollection matches = Regex.Matches(text, @"\/images\/search\?[^\""]+");
                if (matches.Count < 1) return null;
                return $"{self.ConnectionType}://{self.PostDomain}{matches[0].Value.Replace("&amp;", "&")}";
            }
        );
    }
}