using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ned
{
    internal class ExportedNode
    {
        [JsonProperty("id")]
        public Guid Id;
        [JsonProperty("type")]
        public int Type;
        [JsonProperty("outputs")]
        public List<ExportedConnection> Outputs;
    }
}