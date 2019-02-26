using System;
using System.IO;

namespace Com.CI.Infrastructure.Impls
{
    public class Message
    {
        public string EndPoint { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }

        public string Serialize()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memoryStream))
                {
                    writer.Write(EndPoint);

                    writer.Write(Type);

                    writer.Write(Payload);
                }
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public static Message InitializeFromBase64EncodedString(string base64EncodedString)
        {
            var bytes = Convert.FromBase64String(base64EncodedString);

            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(memoryStream))
                {
                    string endPoint = reader.ReadString();
                    string type = reader.ReadString();
                    string serilizedPayload = reader.ReadString();

                    return new Message
                    {
                        EndPoint = endPoint,
                        Type = type,
                        Payload = serilizedPayload
                    };
                }
            }
        }

    }
}
