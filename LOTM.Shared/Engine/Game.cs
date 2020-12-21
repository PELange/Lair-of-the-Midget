using LOTM.Shared.Engine.Network;
using LOTM.Shared.Engine.World;
using LOTM.Shared.Utils;
using System;
using System.Diagnostics;
using System.Threading;

namespace LOTM.Shared.Engine
{
    public abstract class Game
    {
        private const double MAX_REFRESH_RATE_IN_MS = 6; //165 fps in ms

        private bool ShouldShutdown { get; set; }

        protected double FixedUpdateDeltaTime { get; }

        protected NetworkManager NetworkManager { get; }

        protected GameWorld World { get; }


        public DateTime LastUpdate { get; set; }
        public double Accumulator { get; set; }
        public HighPrecisionTimer Timer { get; set; }
        public Stopwatch Stopwatch { get; set; }
        public long GameloopRunningFlag;

        public AutoResetEvent GameLoopReset { get; set; }

        public Game(NetworkManager networkManager)
        {
            NetworkManager = networkManager;

            FixedUpdateDeltaTime = 1.0 / 60; //60 fps

            World = new GameWorld();
        }

        public void Start()
        {
            OnInit();

            LastUpdate = DateTime.Now;

            Stopwatch = Stopwatch.StartNew();

            GameLoopReset = new AutoResetEvent(false);

            Timer = new HighPrecisionTimer();
            Timer.Timer += (sender, args) => GameLoopTick();
            Timer.Start(1, true);

            while (!ShouldShutdown)
            {
                ////GameLoopTick();

                ////Wait 100 ms to see if we should exit then
                //Task.Delay(10).GetAwaiter().GetResult();

                GameLoopReset.WaitOne();

                GameLoopInner();
            }

            NetworkManager.Shutdown();
            OnShutdown();
        }

        protected void GameLoopTick()
        {
            //Avoid two GameLoopTicks running in parallel because of the timer
            if (Interlocked.Read(ref GameloopRunningFlag) == 1)
            {
                return;
            }

            //Do not run if above rate limit
            if (Stopwatch.Elapsed.TotalMilliseconds < MAX_REFRESH_RATE_IN_MS)
            {
                return;
            }

            //Lock the gameloop and restart rate limit counter
            Stopwatch.Restart();
            Interlocked.Increment(ref GameloopRunningFlag);

            //GameLoopInner();
            GameLoopReset.Set();

            //Interlocked.Decrement(ref GameloopRunningFlag);
        }

        void GameLoopInner()
        {
            //Debug.WriteLine($"{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}.{DateTime.Now.Millisecond}");

            var currentTime = DateTime.Now;
            var deltaTime = (currentTime - LastUpdate).TotalMilliseconds / 1000;

            Accumulator += deltaTime;

            //todo only process objects that are close to players ... especially on the server

            //var worldObjects = World.Objects.GetObjectsInArea(World.Objects.Bounds);
            var worldObjects = World.Objects;

            OnBeforeUpdate();

            foreach (var worldObject in worldObjects)
            {
                worldObject.OnBeforeUpdate();
            }

            while (Accumulator >= FixedUpdateDeltaTime)
            {
                OnFixedUpdate(FixedUpdateDeltaTime);

                foreach (var worldObject in worldObjects)
                {
                    worldObject.OnFixedUpdate(FixedUpdateDeltaTime);
                }

                Accumulator -= FixedUpdateDeltaTime;
            }

            OnUpdate(deltaTime);

            foreach (var worldObject in worldObjects)
            {
                worldObject.OnUpdate(deltaTime);
            }

            OnAfterUpdate();

            foreach (var worldObject in worldObjects)
            {
                worldObject.OnAfterUpdate();
            }

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
