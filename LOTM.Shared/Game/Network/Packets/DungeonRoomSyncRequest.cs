using LOTM.Shared.Engine.Network;
using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class DungeonRoomSyncRequest : NetworkPacket
    {
        public DungeonRoomSyncRequest(IPEndPoint sender = default) : base(sender)
        {
        }

        public int RoomNumber { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            RoomNumber = reader.ReadInt32();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write(RoomNumber);
        }
    }
}
