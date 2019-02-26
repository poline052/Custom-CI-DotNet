using Com.CI.Infrastructure;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Com.CI.GitPushEventListener
{
    public class GitBranchPushedEventController : ApiController
    {
        private readonly IEventHandler<BranchPushNotifiedEvent, GitBranchPushedEventHandlerResponse> branchPushNotifiedEventHandler;
        public GitBranchPushedEventController(IEventHandler<BranchPushNotifiedEvent, GitBranchPushedEventHandlerResponse> branchPushNotifiedEventHandler)
        {
            this.branchPushNotifiedEventHandler = branchPushNotifiedEventHandler;
        }


        public async Task<HttpResponseMessage> Post()
        {
            var pushEventMessageJson = await Request.Content.ReadAsStringAsync();

            var branchPushNotifiedEvent = new BranchPushNotifiedEvent { PushEventMessageJson = pushEventMessageJson };

            var gitBranchPushedEventHandlerResponse = await branchPushNotifiedEventHandler.HandleAsync(branchPushNotifiedEvent);

            var responseHttpStatusCode = gitBranchPushedEventHandlerResponse.StatusCode.Equals(200) ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest;

            return new HttpResponseMessage(responseHttpStatusCode);
        }
    }
}