using Newtonsoft.Json;

namespace Rocket.Libraries.Sessions.Models
{
    public class ResponseObject<TPayload>
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("payload")]
        public TPayload Payload { get; set; }
    }
}