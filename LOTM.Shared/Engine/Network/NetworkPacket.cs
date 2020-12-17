using System.Net;
using System.Text.Json.Serialization;

namespace LOTM.Shared.Engine.Network
{
    public class NetworkPacket
    {
        [JsonIgnore]
        public IPEndPoint Sender { get; set; }
    }
}