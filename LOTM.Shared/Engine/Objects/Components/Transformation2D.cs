using LOTM.Shared.Engine.Math;

namespace LOTM.Shared.Engine.Objects.Components
{
    public class Transformation2D : IComponent
    {
        private Vector2 _Position;

        public Vector2 Position
        {
            get
            {
                return _Position;
            }

            set
            {
                _Position = value;

                UpdateBoundingBox();
                _Position.PropertyChanged += (sender, args) => UpdateBoundingBox();
            }
        }

        public double Rotation { get; set; }

        private Vector2 _Scale;
        public Vector2 Scale
        {
            get
            {
                return _Scale;
            }

            set
            {
                _Scale = value;

                UpdateBoundingBox();
                _Position.PropertyChanged += (sender, args) => UpdateBoundingBox();
            }
        }

        private Rectangle _BoundingBox;

        public Rectangle GetBoundingBox()
        {
            return _BoundingBox;
        }

        private void UpdateBoundingBox()
        {
            if (_Position == null || _Scale == null) return;

            if (_BoundingBox == null)
            {
                _BoundingBox = new Rectangle(_Position.X, _Position.Y, _Scale.X, _Scale.Y);
            }
            else
            {
                _BoundingBox.X = _Position.X;
                _BoundingBox.Y = _Position.Y;
                _BoundingBox.Width = _Scale.X;
                _BoundingBox.Height = _Scale.Y;
            }
        }
    }
}
