namespace LOTM.Shared.Engine.Math
{
    public class Rectangle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public Rectangle(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool IntersectsWith(Rectangle other)
        {
            //Left edge must be left of other right edge. Right edge must be right of other left edge. Do this for both axis. Same edge position is considered touch but not intersection
            return (X < (other.X + other.Width)) && ((X + Width) > other.X) && (Y < (other.Y + other.Height)) && ((Y + Height) > other.Y);
        }

        public bool IntersectsWith(Ray other, out Vector2 contactPoint, out Vector2 contactNormal, out double contactRayTime)
        {
            contactPoint = default;
            contactNormal = default;
            contactRayTime = default;

            var timeNearX = (X - other.Origin.X) / other.Direction.X;
            var timeNearY = (Y - other.Origin.Y) / other.Direction.Y;

            var timeFarX = (X + Width - other.Origin.X) / other.Direction.X;
            var timeFarY = (Y + Height - other.Origin.Y) / other.Direction.Y;

            //It is possible that as soon as 0 gets involved in the division, that we end up with NaN. What ever happens, a collision is not assumed for this situation. Touching aka collision distance of 0 is still handled correctly afterwards
            if (double.IsNaN(timeNearX) || double.IsNaN(timeNearY) || double.IsNaN(timeFarX) || double.IsNaN(timeFarY)) return false;

            //Make sure that "far" and "near" are actually what they say relative to the origin coords
            if (timeFarX < timeNearX) (timeNearX, timeFarX) = (timeFarX, timeNearX);
            if (timeFarY < timeNearY) (timeNearY, timeFarY) = (timeFarY, timeNearY);

            //If either of this conditions is met we know we did not have an intersection anywhere in our rect
            if (timeNearX > timeFarY || timeNearY > timeFarX) return false;

            //Get the overall time for near and far point.
            var timeHitNear = System.Math.Max(timeNearX, timeNearY);
            var timeHitFar = System.Math.Min(timeFarX, timeFarY);

            if (timeHitFar < 0) return false; //Ray direction to far pos is pointing in reverse to what we are evaluating
            if (timeHitNear < 0 || timeHitNear > 1) return false; //Hit is before ray start or after ray end

            contactPoint = new Vector2(other.Origin.X + timeHitNear * other.Direction.X, other.Origin.Y + timeHitNear * other.Direction.Y);

            if (timeNearX > timeNearY)
            {
                contactNormal = new Vector2((other.Direction.X < 0) ? 1 : -1, 0);
            }
            else if (timeNearX < timeNearY)
            {
                contactNormal = new Vector2(0, (other.Direction.Y < 0) ? 1 : -1);
            }
            else
            {
                contactNormal = Vector2.ZERO;
            }

            contactRayTime = timeHitNear;

            return true;
        }

        public Rectangle GetIntersectionArea(Rectangle other)
        {
            var maxLeft = System.Math.Max(X, other.X);
            var minRight = System.Math.Min(X + Width, other.X + other.Width);
            var maxTop = System.Math.Max(Y, other.Y);
            var minBottom = System.Math.Min(Y + Height, other.Y + other.Height);

            if (maxLeft < minRight && maxTop < minBottom)
            {
                return new Rectangle(maxLeft, maxTop, minRight - maxLeft, minBottom - maxTop);
            }

            return null;
        }

        public bool Contains(Rectangle other)
        {
            //Other coords must be within bounds of this bbox
            return (other.X >= X) && (other.Y >= Y) && ((other.X + other.Width) <= (X + Width)) && ((other.Y + other.Height) <= (Y + Height));
        }
    }
}
