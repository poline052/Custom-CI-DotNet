using Newtonsoft.Json;

namespace Com.CI.Infrastructure
{
    public static class PushEventMessageFactory
    {
        public static GitBranchPushedEvent Create(string pushEventMessageJson)
        {
            dynamic payloadObject = JsonConvert.DeserializeObject(pushEventMessageJson);

            var pushEventMessage = new GitBranchPushedEvent
            {
                BranchId = payloadObject["push"].changes[0]["new"].name,
                Author = payloadObject["push"].changes[0]["new"].target.author.raw,
                CommitHash = payloadObject["push"].changes[0]["new"].target.hash,
                Message = payloadObject["push"].changes[0]["new"].target.message,
                PushDate = payloadObject["push"].changes[0]["new"].target.date,
                PushType = payloadObject["push"].changes[0]["new"].target.type,
             
                RepositoryId = payloadObject.repository.name,
                Valid = true
            };

            return pushEventMessage;
        }
    }
}
