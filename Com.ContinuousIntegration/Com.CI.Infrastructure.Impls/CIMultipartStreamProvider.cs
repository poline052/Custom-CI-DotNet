using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Com.CI.Infrastructure.Impls
{
    public class CIMultipartStreamProvider : MultipartStreamProvider
    {
        private Collection<bool> isFormData;
        private NameValueCollection formData;
        private List<HttpContent> fileContents;

        public CIMultipartStreamProvider()
        {
            formData = new NameValueCollection();
            fileContents = new List<HttpContent>();
            isFormData = new Collection<bool>();
        }
        public NameValueCollection FormData
        {
            get { return formData; }
        }

        public List<HttpContent> Files
        {
            get { return fileContents; }
        }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;
            if (contentDisposition != null)
            {
                isFormData.Add(string.IsNullOrEmpty(contentDisposition.FileName));

                return new MemoryStream();
            }
            throw new InvalidOperationException(string.Format("Did not find required '{0}' header field in MIME multipart body part.", "Content-Disposition"));
        }

        public override async Task ExecutePostProcessingAsync()
        {
            for (int index = 0; index < Contents.Count; index++)
            {
                if (isFormData[index])
                {
                    HttpContent formContent = Contents[index];

                    ContentDispositionHeaderValue contentDisposition = formContent.Headers.ContentDisposition;
                    string formFieldName = UnquoteToken(contentDisposition.Name) ?? string.Empty;

                    string formFieldValue = await formContent.ReadAsStringAsync();
                    formData.Add(formFieldName, formFieldValue);
                }
                else
                {
                    fileContents.Add(Contents[index]);
                }
            }
        }

        private static string UnquoteToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return token;
            }

            if (token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal) && token.Length > 1)
            {
                return token.Substring(1, token.Length - 2);
            }

            return token;
        }
    }
}
