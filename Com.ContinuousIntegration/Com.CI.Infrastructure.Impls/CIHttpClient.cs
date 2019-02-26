using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Com.CI.Infrastructure.Impls
{
    public class CIHttpClient : IHttpClient
    {
        public async Task<Response> PostAsync<Response>(string agentHostUri, KeyValuePair<string, string>[] multipartFormValues, Stream deploymentPackageStream)
        {
            using (var httpClient = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    foreach (var keyValuePair in multipartFormValues)
                    {
                        content.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
                    }

                    var fileContent = new StreamContent(deploymentPackageStream);

                    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "Package"
                    };

                    content.Add(fileContent);

                    var response = await httpClient.PostAsync(agentHostUri, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContentJsonString = await response.Content.ReadAsStringAsync();

                        return JsonConvert.DeserializeObject<Response>(responseContentJsonString);
                    }

                    return default(Response);
                }
            }
        }
    }
}
