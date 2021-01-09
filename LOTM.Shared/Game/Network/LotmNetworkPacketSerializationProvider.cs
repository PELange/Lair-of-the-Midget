using LOTM.Shared.Engine.Network;
using LOTM.Shared.Game.Network.Packets;
using System;
using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network
{
    public class LotmNetworkPacketSerializationProvider : INetworkPacketSerializationProvider
    {
        public byte[] SerializePacket(NetworkPacket packet)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);

            switch (packet)
            {
                case PlayerJoin _:
                    writer.Write(1);
                    break;

                case PlayerJoinAck _:
                    writer.Write(2);
                    break;

                case PlayerInput _:
                    writer.Write(3);
                    break;

                case LivingObjectCreation _:
                    writer.Write(4);
                    break;

                case ObjectHealthUpdate _:
                    writer.Write(5);
                    break;

                case ObjectPositionUpdate _:
                    writer.Write(6);
                    break;

                default:
                    throw new Exception($"Tried to serialize packet with unknown type '{packet}'.");
            }

            packet.WriteBytes(writer);

            return memoryStream.ToArray();
        }

        public NetworkPacket DeserializePacket(byte[] data, IPEndPoint sender)
        {
            if (data == null || sender == null || data.Length < 1) return null;

            using MemoryStream memoryStream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(memoryStream);

            var type = reader.ReadInt32();
            NetworkPacket networkPacket = null;

            switch (type)
            {
                case 1:
                    networkPacket = new PlayerJoin(sender);
                    break;

                case 2:
                    networkPacket = new PlayerJoinAck(sender);
                    break;

                case 3:
                    networkPacket = new PlayerInput(sender);
                    break;

                case 4:
                    networkPacket = new LivingObjectCreation(sender);
                    break;

                case 5:
                    networkPacket = new ObjectHealthUpdate(sender);
                    break;

                case 6:
                    networkPacket = new ObjectPositionUpdate(sender);
                    break;

                default:
                    Console.WriteLine($"Recieved packet with unknown type '{type}'.");
                    break;
            }

            if (networkPacket != null)
            {
                networkPacket.ReadBytes(reader);
            }

            return networkPacket;
        }
    }
}
