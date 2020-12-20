using LOTM.Shared.Engine.Network;
using LOTM.Shared.Engine.World;
using System;
using System.Diagnostics;
using System.Threading;

namespace LOTM.Shared.Engine
{
    public abstract class Game
    {
        private const double MAX_REFRESH_RATE_IN_MS = (1.0 / 165) / 1000; //165 fps in ms

        private bool ShouldShutdown { get; set; }

        protected double FixedUpdateDeltaTime { get; }

        protected NetworkManager NetworkManager { get; }

        protected GameWorld World { get; }

        public Game(NetworkManager networkManager)
        {
            NetworkManager = networkManager;

            FixedUpdateDeltaTime = 1.0 / 60; //60 fps

            World = new GameWorld();
        }

        public void Start()
        {
            OnInit();

            var lastUpdate = DateTime.Now;
            double accumulator = 0.0;

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            while (!ShouldShutdown)
            {
                var currentTime = DateTime.Now;
                var deltaTime = (currentTime - lastUpdate).TotalMilliseconds / 1000;

                accumulator += deltaTime;

                //todo only process objects that are close to players ... especially on the server

                //var worldObjects = World.Objects.GetObjectsInArea(World.Objects.Bounds);
                var worldObjects = World.Objects;

                OnBeforeUpdate();

                foreach (var worldObject in worldObjects)
                {
                    worldObject.OnBeforeUpdate();
                }

                while (accumulator >= FixedUpdateDeltaTime)
                {
                    OnFixedUpdate(FixedUpdateDeltaTime);

                    foreach (var worldObject in worldObjects)
                    {
                        worldObject.OnFixedUpdate(FixedUpdateDeltaTime);
                    }

                    accumulator -= FixedUpdateDeltaTime;
                }

                OnUpdate(deltaTime);

                foreach (var worldObject in worldObjects)
                {
                    worldObject.OnUpdate(deltaTime);
                }

                OnAfterUpdate();

                lastUpdate = currentTime;

                //165 fps refesh limit. If the frame was calculated faster than this, sleep for the rest of the frame
                if (stopWatch.ElapsedMilliseconds < MAX_REFRESH_RATE_IN_MS)
                {
                    Thread.Sleep((int)(MAX_REFRESH_RATE_IN_MS - stopWatch.ElapsedMilliseconds));
                }

                stopWatch.Reset();
            }

            NetworkManager.Shutdown();
            OnShutdown();
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
