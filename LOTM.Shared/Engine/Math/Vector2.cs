using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LOTM.Shared.Engine.Math
{
    public class Vector2 : INotifyPropertyChanged
    {
        public static Vector2 ZERO => new Vector2(0, 0);

        public event PropertyChangedEventHandler PropertyChanged;

        protected double _X;

        public double X
        {
            get
            {
                return _X;
            }

            set
            {
                _X = value;
                OnPropertyChanged();
            }
        }


        protected double _Y;

        public double Y
        {
            get
            {
                return _Y;
            }

            set
            {
                _Y = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Normalize vector in place
        /// </summary>
        public void Normalize()
        {
            var magnitude = System.Math.Sqrt(X * X + Y * Y);

            if (magnitude == 0) return;

            X /= magnitude;
            Y /= magnitude;
        }
    }
}