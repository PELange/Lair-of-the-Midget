using System.IO;
using System.Net;

namespace LOTM.Shared.Engine.Network
{
    public class NetworkPacket
    {
        protected static int NEXT_PACKET_ID = 1;

        public int Id { get; set; }
        public IPEndPoint Sender { get; }
        public bool RequiresAck { get; set; }

        public NetworkPacket(IPEndPoint sender = default, bool requiresAck = false)
        {
            if (sender == default)
            {
                Id = NEXT_PACKET_ID++; //Outboind packet. Set unique id
            }
            else
            {
                Sender = sender; //Inbound packet, we will know the id afer we read the packet bytes. Just store the sender from IP layer
            }

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