using LOTM.Shared.Engine.Network;
using LOTM.Shared.Game.Network.Packets;
using System;
using System.Net;
using System.Text;
using System.Text.Json;

namespace LOTM.Shared.Game.Network
{
    public class LotmNetworkPacketSerializationProvider : INetworkPacketSerializationProvider
    {
        public byte[] SerializePacket(NetworkPacket packet)
        {
            var textBytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(packet, packet.GetType()));

            var data = new byte[textBytes.Length + 1];

            byte type = 0;

            switch (packet)
            {
                case PlayerJoin _:
                    type = 1;
                    break;

                case PlayerJoinAck _:
                    type = 2;
                    break;

                default:
                    break;
            }

            data[0] = type;

            Buffer.BlockCopy(textBytes, 0, data, 1, textBytes.Length);

            return data;
        }

        public NetworkPacket DeserializePacket(byte[] data, IPEndPoint sender)
        {
            if (data == null || sender == null || data.Length < 1) return null;

            var type = data[0];

            //Replace data prefis with empty space to allow for direct deserialize
            data[0] = 0x20; //Space

            NetworkPacket resultPacket = default;

            switch (type)
            {
                case 1:
                    resultPacket = JsonSerializer.Deserialize<PlayerJoin>(data);
                    break;

                case 2:
                    resultPacket = JsonSerializer.Deserialize<PlayerJoinAck>(data);
                    break;
            }

            resultPacket.Sender = sender;

            return resultPacket;
        }
    }
}
