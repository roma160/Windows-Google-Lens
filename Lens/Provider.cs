﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        { Raw, Base64 }

        public readonly String PostUrl;
        public readonly String ConnectionType;
        public readonly String PostDomain;
        public readonly String PostPath;

        public readonly Dictionary<String, String> QueryParams;
        public readonly ImageEncodingType EncodingType;
        public readonly String TimestampQueryName;
        public readonly String ImageEntryName;

        public readonly int PostBoundaryLength;
        private readonly String PostHeaderTemplate;
        private readonly int PostBodyTemplateSize;
        private readonly String PostBodyBeginningTemplate;
        private readonly String PostBodyEndingTemplate;
        public readonly int PostSuccessStatusCode;

        public delegate String ParseURLFunction(PUploadGResultProvider self, String responseText);
        public readonly ParseURLFunction ParseUrlFunction;

        private static Random Random = new Random();
        private const String BoundaryCharacters = 
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public PUploadGResultProvider(
            String providerName, String postUrl,
            Dictionary<String, String> queryParams,
            ImageEncodingType encodingType,
            String timestampQueryName, String imageEntryName,
            int postSuccessStatusCode,
            ParseURLFunction parseUrlFunction,
            int postFormBoundaryLength = 40) : base(providerName)
        {
            PostUrl = postUrl;
            QueryParams = queryParams;
            EncodingType = encodingType;
            TimestampQueryName = timestampQueryName;
            ImageEntryName = imageEntryName;
            PostBoundaryLength = postFormBoundaryLength;
            PostSuccessStatusCode = postSuccessStatusCode;
            ParseUrlFunction = parseUrlFunction;

            GroupCollection groups = new Regex(
                @"(http(|s)):\/\/([^\/]+)(.*)").Match(PostUrl).Groups;
            ConnectionType = groups[1].Value;
            PostDomain = groups[3].Value;
            PostPath = groups[4].Value;

            // Working with templates
            PostHeaderTemplate = $"POST {PostPath}?{{0}} HTTP/1.1{SocketWorker.LineSep}" +
                                 $"Host: {PostDomain}{SocketWorker.LineSep}" +
                                 $"Content-Length: {{2}}{SocketWorker.LineSep}" +
                                 $"Content-Type: multipart/form-data; boundary={{1}}{SocketWorker.LineSep}{SocketWorker.LineSep}";

            PostBodyBeginningTemplate = $"--{{0}}{SocketWorker.LineSep}Content-Disposition: form-data";
            if (ImageEntryName != null)
                PostBodyBeginningTemplate += $"; name=\"{ImageEntryName}\"";
            PostBodyBeginningTemplate += $"{SocketWorker.LineSep}Content-Type: image/jpeg{SocketWorker.LineSep}{SocketWorker.LineSep}";

            PostBodyEndingTemplate = $"{SocketWorker.LineSep}--{{0}}--";

            // Terrible way of solving this problem, can't come up with another scalable algo 
            PostBodyTemplateSize = String.Format(
                    PostBodyBeginningTemplate + PostBodyEndingTemplate, GenerateRandomBoundary()).Length;
        }

        private async Task<String> GetQueryString()
        {
            Dictionary<String, String> queryDictionary = new Dictionary<String, String>(QueryParams);
            if (TimestampQueryName != null) queryDictionary.Add(
                TimestampQueryName, DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString());
            return await new FormUrlEncodedContent(queryDictionary).ReadAsStringAsync();
        }

        public String GenerateRandomBoundary()
        {
            // https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings
            return new String(Enumerable.Repeat(BoundaryCharacters, PostBoundaryLength)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        private int GetBodyLength(int imageBytesLength) => PostBodyTemplateSize + imageBytesLength;

        public async Task<Tuple<String, String>> GetPostContents(int imageBytesLength)
        {
            Task<String> queryString = GetQueryString();
            String boundary = GenerateRandomBoundary();
            String bodyBeginning = String.Format(PostBodyBeginningTemplate, boundary);
            String bodyEnding = String.Format(PostBodyEndingTemplate, boundary);
            String header = String.Format(PostHeaderTemplate, 
                await queryString, boundary, GetBodyLength(imageBytesLength));
            return new Tuple<String, String>(
                header + bodyBeginning, bodyEnding);
        }

        public String GetGetLink(String responseText)
        {
            return ParseUrlFunction(this, responseText);
        }
    }
}
