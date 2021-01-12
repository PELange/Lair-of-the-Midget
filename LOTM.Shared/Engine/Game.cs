using LOTM.Shared.Engine.Network;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Utils;
using System;
using System.Threading;

namespace LOTM.Shared.Engine
{
    public abstract class Game
    {
        private const double MAX_REFRESH_RATE_IN_MS = 8; //Lock to 112 to 125 fps
        private double FixedTickrate { get; }

        private bool ShouldShutdown { get; set; }
        private DateTime LastUpdate { get; set; }
        private double Accumulator { get; set; }
        private HighPrecisionTimer Timer { get; set; }
        private DateTime GameLoopLastRun { get; set; }
        private AutoResetEvent GameLoopReset { get; set; }
        private long GameloopRunningFlag;

        protected GameWorld World { get; }
        protected NetworkManager NetworkManager { get; }

        public Game(double tickRate, NetworkManager networkManager)
        {
            FixedTickrate = tickRate;

            NetworkManager = networkManager;

            World = new GameWorld();
        }

        public void Start()
        {
            OnInit();

            LastUpdate = DateTime.Now;
            GameLoopLastRun = DateTime.Now;

            GameLoopReset = new AutoResetEvent(false);

            Timer = new HighPrecisionTimer();
            Timer.Timer += (sender, args) => TimerTick();
            Timer.Start(1, true);

            while (!ShouldShutdown)
            {
                GameLoopReset.WaitOne();
                GameLoopInner();
            }

            NetworkManager.Shutdown();
            OnShutdown();
        }

        protected void TimerTick()
        {
            //Avoid two GameLoopTicks running in parallel because of the timer
            if (Interlocked.Read(ref GameloopRunningFlag) == 1) return;

            //Do not run if above rate limit
            if ((DateTime.Now - GameLoopLastRun).TotalMilliseconds < MAX_REFRESH_RATE_IN_MS) return;
            GameLoopLastRun = DateTime.Now;

            //Lock the gameloop and restart rate limit counter
            Interlocked.Increment(ref GameloopRunningFlag);
            GameLoopReset.Set();
        }

        void GameLoopInner()
        {
            //Debug.WriteLine($"{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}.{DateTime.Now.Millisecond}");

            var currentTime = DateTime.Now;
            var deltaTime = (currentTime - LastUpdate).TotalMilliseconds / 1000;

            Accumulator += deltaTime;

            OnBeforeUpdate();

            while (Accumulator >= FixedTickrate)
            {
                OnFixedUpdate(FixedTickrate);

                Accumulator -= FixedTickrate;
            }

            OnUpdate(deltaTime);

            OnAfterUpdate();

            LastUpdate = currentTime;

            Interlocked.Decrement(ref GameloopRunningFlag);
        }

        public void Shutdown()
        {
            ShouldShutdown = true;
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnBeforeUpdate()
        {
        }

        protected virtual void OnFixedUpdate(double deltaTime)
        {
        }

        protected virtual void OnUpdate(double deltaTime)
        {
        }

        protected virtual void OnAfterUpdate()
        {
        }

        protected virtual void OnShutdown()
        {
        }
    }
}
