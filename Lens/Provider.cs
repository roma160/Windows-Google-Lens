using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

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
        public readonly String ConnectionType;
        public readonly String PostDomain;
        public readonly String PostPath;

        public readonly Dictionary<String, String> QueryParams;
        public readonly ImageEncodingType EncodingType;
        public readonly String TimestampQueryName;
        public readonly String ImageEntryName;
        public readonly int PostSuccessStatusCode;

        public readonly Regex GetUrlRegex;
        public readonly bool GetIncludesDomain;

        public PUploadGResultProvider(
            String providerName, String postUrl,
            Dictionary<String, String> queryParams,
            ImageEncodingType encodingType,
            String timestampQueryName, String imageEntryName,
            int postSuccessStatusCode,
            Regex getUrlRegex, bool getIncludesDomain) : base(providerName)
        {
            PostUrl = postUrl;
            QueryParams = queryParams;
            EncodingType = encodingType;
            TimestampQueryName = timestampQueryName;
            ImageEntryName = imageEntryName;
            PostSuccessStatusCode = postSuccessStatusCode;

            GetUrlRegex = getUrlRegex;
            GroupCollection groups = new Regex(
                @"(http(|s)):\/\/([^\/]+)(.*)").Match(PostUrl).Groups;
            ConnectionType = groups[1].Value;
            PostDomain = groups[3].Value;
            PostPath = groups[4].Value;
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

            String returnLink = matches[0].Value.Replace("&amp;", "&");
            if (!GetIncludesDomain) returnLink =
                $"{ConnectionType}://{PostDomain}{returnLink}";
            return returnLink;
        }
    }

    public static class Providers
    {
        public static PUploadGResultProvider GoogleLens = new PUploadGResultProvider(
            "Google Lens", "https://lens.google.com/upload",
            new Dictionary<String, String>{ { "ep", "ccm" }, { "s", "csp" } },
            PUploadGResultProvider.ImageEncodingType.Raw,
            "st", "encoded_image", 200,
            new Regex(@"https:\/\/lens\.google\.com\/search\?p=[^\""]+", RegexOptions.Compiled),
            true
        );

        public static PUploadGResultProvider MicrosoftBing = new PUploadGResultProvider(
            "Microsoft Bing", "https://www.bing.com/images/detail/search",
            new Dictionary<String, String>{ { "iss", "sbiupload" }, { "FORM", "ANCMS1" } },
            PUploadGResultProvider.ImageEncodingType.Base64,
            null, "imageBin", 302,
            new Regex(@"\/images\/search\?[^\""]+"), false
        );
    }
}
