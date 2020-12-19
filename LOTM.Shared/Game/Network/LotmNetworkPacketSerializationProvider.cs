using LOTM.Shared.Engine.Network;
using LOTM.Shared.Engine.Network.Packets;
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

                case DynamicHealthObjectSync _:
                    type = 3;
                    break;

                case GameObjectSync _:
                    type = 4;
                    break;

                case PlayerInput _:
                    type = 5;
                    break;

                default:
                    Console.WriteLine($"Tried to serialize packet with unknown type '{packet}'.");
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

                case 3:
                    resultPacket = JsonSerializer.Deserialize<DynamicHealthObjectSync>(data);
                    break;

                case 4:
                    resultPacket = JsonSerializer.Deserialize<GameObjectSync>(data);
                    break;

                case 5:
                    resultPacket = JsonSerializer.Deserialize<PlayerInput>(data);
                    break;

                default:
                    Console.WriteLine($"Recieved packet with unknown type '{type}'.");
                    break;
            }

            if (resultPacket != null) resultPacket.Sender = sender;

            return resultPacket;
        }
    }
}
