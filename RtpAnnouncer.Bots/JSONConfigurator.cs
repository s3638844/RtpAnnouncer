using Newtonsoft.Json;

namespace RtpAnnouncer.Bots
{
    public struct JSONConfigurator
    {
        [JsonProperty("token")] public string Token { get; private set; }
        [JsonProperty("prefix")] public string Prefix { get; private set; }
    }
}