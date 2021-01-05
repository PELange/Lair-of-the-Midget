using System.IO;
using System.Net;

namespace LOTM.Shared.Engine.Network
{
    public class NetworkPacket
    {
        protected static int NEXT_PACKET_ID = 0;

        public int Id { get; set; }
        public IPEndPoint Sender { get; }

        public NetworkPacket(IPEndPoint sender = default)
        {
            if (sender == default)
            {
                Id = NEXT_PACKET_ID++; //Outboind packet. Set unique id
            }
            else
            {
                Sender = sender; //Inbound packet, we will know the id afer we read the packet bytes. Just store the sender from IP layer
            }
        }

        public virtual void WriteBytes(BinaryWriter writer)
        {
            writer.Write(Id);
        }

        public virtual void ReadBytes(BinaryReader reader)
        {
            Id = reader.ReadInt32();
        }
    }
}