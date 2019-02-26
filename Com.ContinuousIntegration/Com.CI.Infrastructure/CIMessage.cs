using System;
using System.IO;

namespace Com.CI.Infrastructure
{
    public class CIMessage : IEvent
    {
        public DateTime MessageDate { get; set; }
        public bool IsError { get; set; }
        public string RepositoryId { get; set; }
        public string BranchId { get; set; }
        public string MessageBody { get; set; }

        public static CIMessage Create(string repositoryId, string branchId, string messageBody, bool isError = false)
        {
            return new CIMessage
            {
                BranchId = branchId,
                IsError = isError,
                MessageBody = messageBody,
                MessageDate = DateTime.UtcNow,
                RepositoryId = repositoryId
            };
        }


        public string Serialize()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memoryStream))
                {
                    writer.Write(MessageDate.ToString());
                    writer.Write(IsError);
                    writer.Write(RepositoryId);
                    writer.Write(BranchId);
                    writer.Write(MessageBody);
                }
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public static CIMessage InitializeFromBase64EncodedString(string base64EncodedString)
        {
            var bytes = Convert.FromBase64String(base64EncodedString);

            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(memoryStream))
                {
                    var messageDate = DateTime.Parse(reader.ReadString());
                    var isError = reader.ReadBoolean();
                    var repositoryId = reader.ReadString();
                    var branchId = reader.ReadString();
                    var messageBody = reader.ReadString();

                    return new CIMessage
                    {
                        BranchId = branchId,
                        IsError = isError,
                        MessageBody = messageBody,
                        MessageDate = messageDate,
                        RepositoryId = repositoryId
                    };
                }
            }
        }


    }
}
