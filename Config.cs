using Newtonsoft.Json;

namespace LoopTimer
{
    internal class Config
    {
        [JsonProperty("maxSeconds")]
        public int MaxSeconds { get; set; }
    }
}