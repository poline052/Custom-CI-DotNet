using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Com.CI.Infrastructure
{
    public interface IHttpClient
    {
        Task<Response> PostAsync<Response>(string agentHostUri, KeyValuePair<string, string>[] multipartFormValues, Stream deploymentPackageStream);
    }
}
