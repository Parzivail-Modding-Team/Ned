using System;
using Newtonsoft.Json;

namespace Ned
{
    internal class ExportedConnection
    {
        [JsonProperty("connectId")]
        public Guid? ConnectedNode;
        [JsonProperty("text")]
        public string Text;
    }
}