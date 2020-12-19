namespace LOTM.Shared.Engine.Network.Packets
{
    public class GameObjectSync : NetworkPacket
    {
        public string Type { get; set; }

        public int Id { get; set; }
        public double? PositionX { get; set; }
        public double? PositionY { get; set; }
        public double? Rotation { get; set; }
        public double? ScaleX { get; set; }
        public double? ScaleY { get; set; }
    }
}
