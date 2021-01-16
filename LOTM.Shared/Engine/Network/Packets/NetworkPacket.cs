using System.IO;
using System.Net;

namespace LOTM.Shared.Engine.Network
{
    public class NetworkPacket
    {
        public int Id { get; set; }
        public IPEndPoint Sender { get; }
        public bool RequiresAck { get; set; }

        public NetworkPacket(IPEndPoint sender = default, bool requiresAck = false)
        {
            Sender = sender; //Only interesting on receiver side. Will be default on a packet that is being prepared to be sent
            RequiresAck = requiresAck;
        }

        public virtual void WriteBytes(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(RequiresAck);
        }

        public virtual void ReadBytes(BinaryReader reader)
        {
            Id = reader.ReadInt32();
            RequiresAck = reader.ReadBoolean();
        }
    }
}