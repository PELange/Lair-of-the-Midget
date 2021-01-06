﻿namespace LOTM.Shared.Engine.Math
{
    public class BoundingBox
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public BoundingBox(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool IntersectsWith(BoundingBox other)
        {
            //Check all conditions where the bounding box will not overlap and invert the result.
            return !(other.X > (X + Width) || (other.X + other.Width) < X || other.Y > (Y + Height) || (other.Y + other.Height) < Y);
        }

        public bool Contains(BoundingBox other)
        {
            //Other coords must be within bounds of this bbox
            return (other.X >= X) && (other.Y >= Y) && ((other.X + other.Width) <= (X + Width)) && ((other.Y + other.Height) <= (Y + Height));
        }
    }
}