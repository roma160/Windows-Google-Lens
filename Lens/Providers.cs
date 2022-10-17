using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Windows_Google_Lens.Lens
{
    public static class Providers
    {
        public static PUploadGResultProvider GoogleLens = new PUploadGResultProvider(
            "Google Lens", "https://lens.google.com/upload",
            new Dictionary<String, String> { { "ep", "ccm" }, { "s", "csp" } },
            PUploadGResultProvider.ImageEncodingType.Raw,
            "st", "encoded_image", 200,
            new Regex(@"https:\/\/lens\.google\.com\/search\?p=[^\""]+", RegexOptions.Compiled),
            true
        );

        public static PUploadGResultProvider MicrosoftBing = new PUploadGResultProvider(
            "Microsoft Bing", "https://www.bing.com/images/detail/search",
            new Dictionary<String, String> { { "iss", "sbiupload" }, { "FORM", "ANCMS1" } },
            PUploadGResultProvider.ImageEncodingType.Base64,
            null, "imageBin", 302,
            new Regex(@"\/images\/search\?[^\""]+"), false
        );
    }
}