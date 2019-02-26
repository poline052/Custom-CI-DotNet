using Microsoft.AspNet.SignalR.Hosting;

namespace Com.CI.Infrastructure.Impls
{
    public class CISubscriber
    {
        public string BranchId { get; set; }
        public string RepositoryId { get; set; }
        public string ConnectionId { get; set; }

        public static CISubscriber CreateFromRequestQueryString( string connectionId, INameValueCollection queryString)
        {
            var ciSubscriber = new CISubscriber
            {
                ConnectionId = connectionId,
                RepositoryId = queryString["RepositoryId"],
                BranchId = queryString["BranchId"]
            };

            return ciSubscriber;
        }
    }




}
