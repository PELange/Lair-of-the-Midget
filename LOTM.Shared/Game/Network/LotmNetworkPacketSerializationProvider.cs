using LOTM.Shared.Engine.Network;
using LOTM.Shared.Game.Network.Packets;
using System;
using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network
{
    public class LotmNetworkPacketSerializationProvider : NetworkPacketSerializationProvider
    {
        public override bool SerializePacket(NetworkPacket packet, out byte[] data)
        {
            if (base.SerializePacket(packet, out data))
            {
                return true;
            }

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

                case PlayerCreation _:
                    writer.Write(4);
                    break;

                case LivingObjectCreation _:
                    writer.Write(5);
                    break;

                case ObjectCreation _:
                    writer.Write(6);
                    break;

                case ObjectHealthUpdate _:
                    writer.Write(7);
                    break;

                case ObjectPositionUpdate _:
                    writer.Write(8);
                    break;

                case PlayerInputAck _:
                    writer.Write(9);
                    break;

                case GameStateRequest _:
                    writer.Write(10);
                    break;

                case GameStateUpdate _:
                    writer.Write(11);
                    break;

                case PickupStateUpdate _:
                    writer.Write(12);
                    break;

                default:
                    throw new Exception($"Tried to serialize packet with unknown type '{packet}'.");
            }

            packet.WriteBytes(writer);

            data = memoryStream.ToArray();

            return true;
        }

        public override NetworkPacket DeserializePacket(byte[] data, IPEndPoint sender)
        {
            var networkPacket = base.DeserializePacket(data, sender);

            if (networkPacket != null) return networkPacket;

            if (data == null || sender == null || data.Length < 1) return null;

            using MemoryStream memoryStream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(memoryStream);

            var type = reader.ReadInt32();

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
                    networkPacket = new PlayerCreation(sender);
                    break;

                case 5:
                    networkPacket = new LivingObjectCreation(sender);
                    break;

                case 6:
                    networkPacket = new ObjectCreation(sender);
                    break;

                case 7:
                    networkPacket = new ObjectHealthUpdate(sender);
                    break;

                case 8:
                    networkPacket = new ObjectPositionUpdate(sender);
                    break;

                case 9:
                    networkPacket = new PlayerInputAck(sender);
                    break;

                case 10:
                    networkPacket = new GameStateRequest(sender);
                    break;

                case 11:
                    networkPacket = new GameStateUpdate(sender);
                    break;

                case 12:
                    networkPacket = new PickupStateUpdate(sender);
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
