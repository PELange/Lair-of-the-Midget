﻿using LOTM.Shared.Engine.Network;
using System.IO;
using System.Net;

namespace LOTM.Shared.Game.Network.Packets
{
    public class PlayerJoin : NetworkPacket
    {
        public PlayerJoin(IPEndPoint sender = default) : base(sender)
        {
        }

        public string PlayerName { get; set; }

        public override void ReadBytes(BinaryReader reader)
        {
            base.ReadBytes(reader);

            PlayerName = reader.ReadString();
        }

        public override void WriteBytes(BinaryWriter writer)
        {
            base.WriteBytes(writer);

            writer.Write(PlayerName);
        }
    }
}
