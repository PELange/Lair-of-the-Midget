using LOTM.Shared.Engine.Network;
using LOTM.Shared.Engine.World;
using System;
using System.Diagnostics;
using System.Threading;

namespace LOTM.Shared.Engine
{
    public abstract class Game
    {
        private bool ShouldShutdown { get; set; }

        protected float FixedUpdateDeltaTime { get; }

        protected NetworkManager NetworkManager { get; }

        protected GameWorld World { get; }

        public Game(NetworkManager networkManager)
        {
            NetworkManager = networkManager;

            FixedUpdateDeltaTime = 1 / 60.0f; //60 fps

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

                foreach (var worldObject in worldObjects)
                {
                    worldObject.OnBeforeUpdate();
                }

                OnBeforeUpdate();

                while (accumulator >= FixedUpdateDeltaTime)
                {
                    foreach (var worldObject in worldObjects)
                    {
                        worldObject.OnFixedUpdate(FixedUpdateDeltaTime);
                    }

                    OnFixedUpdate(FixedUpdateDeltaTime);

                    accumulator -= FixedUpdateDeltaTime;
                }

                foreach (var worldObject in worldObjects)
                {
                    worldObject.OnUpdate(deltaTime);
                }

                OnUpdate(deltaTime);

                OnAfterUpdate();

                lastUpdate = currentTime;

                Console.WriteLine($"Frame took {stopWatch.ElapsedMilliseconds} to process");

                if (stopWatch.ElapsedMilliseconds < 5) //200 hz refesh limit. If the frame was calculated faster than this, sleep for the rest of the frame
                {
                    Thread.Sleep((int)(5 - stopWatch.ElapsedMilliseconds));
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

        protected abstract void OnInit();

        protected abstract void OnBeforeUpdate();

        protected abstract void OnFixedUpdate(double deltaTime);

        protected abstract void OnUpdate(double deltaTime);

        protected abstract void OnAfterUpdate();

        protected abstract void OnShutdown();
    }
}
