using System;
using System.Runtime.InteropServices;

namespace LOTM.Shared.Utils
{
    /*
     * Based on https://www.pinvoke.net/default.aspx/winmm.timesetevent
     */

    public class HighPrecisionTimer : IDisposable
    {
        //Lib API declarations
        [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
        static extern uint timeSetEvent(uint uDelay, uint uResolution, TimerCallback lpTimeProc, UIntPtr dwUser, uint fuEvent);
        delegate void TimerCallback(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2);

        [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
        static extern uint timeKillEvent(uint uTimerID);

        [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
        static extern uint timeGetTime();

        [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
        static extern uint timeBeginPeriod(uint uPeriod);

        [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
        static extern uint timeEndPeriod(uint uPeriod);

        private uint id = 0;
        private bool disposed = false;
        private readonly TimerCallback thisCB;
        public event EventHandler Timer;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                Stop();
            }

            disposed = true;
        }

        ~HighPrecisionTimer()
        {
            Dispose(false);
        }

        protected virtual void OnTimer(EventArgs e)
        {
            Timer?.Invoke(this, e);
        }

        public HighPrecisionTimer()
        {
            thisCB = CBFunc;
        }

        /// <summary>
        /// Stop the current timer instance (if any)
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                if (id != 0)
                {
                    timeKillEvent(id);

                    id = 0;
                }
            }
        }

        /// <summary>
        /// Start a timer instance
        /// </summary>
        /// <param name="ms">Timer interval in milliseconds</param>
        /// <param name="repeat">If true sets a repetitive event, otherwise sets a one-shot</param>
        public void Start(uint ms, bool repeat)
        {
            Stop();

            lock (this)
            {
                if (timeSetEvent(ms, 0, thisCB, UIntPtr.Zero, (uint)(0x0000 | (repeat ? 1 : 0))) == 0) throw new Exception("timeSetEvent error");
            }
        }

        void CBFunc(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2)
        {
            OnTimer(new EventArgs());
        }
    }
}
