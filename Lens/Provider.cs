using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Windows_Google_Lens.Lens
{
    public abstract class Provider
    {
        public readonly String ProviderName;

        public Provider(String providerName) => ProviderName = providerName;

        public override string ToString() => $"{{{ProviderName} image searching provider}}";
    }

    public class PUploadGResultProvider : Provider
    {
        public enum ImageEncodingType
        {
            Raw, Base64
        }

        public readonly String PostUrl;
        public readonly Dictionary<String, String> QueryParams;
        public readonly ImageEncodingType EncodingType;
        public readonly String TimestampQueryName;
        public readonly String ImageEntryName;

        public readonly Regex GetUrlRegex;
        private readonly String DomainName;
        public readonly bool GetIncludesDomain;

        public PUploadGResultProvider(
            String providerName, String postUrl,
            Dictionary<String, String> queryParams,
            ImageEncodingType encodingType,
            String timestampQueryName, String imageEntryName,
            Regex getUrlRegex, bool getIncludesDomain) : base(providerName)
        {
            PostUrl = postUrl;
            QueryParams = queryParams;
            EncodingType = encodingType;
            TimestampQueryName = timestampQueryName;
            ImageEntryName = imageEntryName;

            GetUrlRegex = getUrlRegex;
            DomainName = new Regex(@"http(|s):\/\/[^\/]+").Matches(PostUrl)[0].Value;
            GetIncludesDomain = getIncludesDomain;
        }

        public async Task<String> GetQueryString()
        {
            Dictionary<String, String> queryDictionary = new Dictionary<String, String>(QueryParams);
            if (TimestampQueryName != null) queryDictionary.Add(
                TimestampQueryName, DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString());
            return await new FormUrlEncodedContent(queryDictionary).ReadAsStringAsync();
        }

        public String GetGetLink(String responseText)
        {
            MatchCollection matches = GetUrlRegex.Matches(responseText);
            if (matches.Count < 1) return null;

            String returnLink = matches[0].Value;
            if (!GetIncludesDomain) returnLink = DomainName + returnLink;
            return returnLink;
        }
    }

    public static class Providers
    {
        public static PUploadGResultProvider GoogleLens = new PUploadGResultProvider(
            "Google Lens", "https://lens.google.com/upload",
            new Dictionary<String, String>{ { "ep", "ccm" }, { "s", "csp" } },
            PUploadGResultProvider.ImageEncodingType.Raw,
            "st", "encoded_image",
            new Regex(@"https:\/\/lens\.google\.com\/search\?p=[^\""]+", RegexOptions.Compiled),
            true
        );

        public static PUploadGResultProvider MicrosoftBing = new PUploadGResultProvider(
            "Microsoft Bing", "https://www.bing.com/images/detail/search",
            new Dictionary<String, String>{ { "iss", "sbiupload" }, { "FORM", "ANCMS1" } },
            PUploadGResultProvider.ImageEncodingType.Base64,
            null, "imageBin",
            new Regex(@"\/images\/search\?[^\""]+"), false
        );
    }
}
