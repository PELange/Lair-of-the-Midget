using LOTM.Shared.Engine.Network;
using LOTM.Shared.Engine.World;
using System;

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

            while (!ShouldShutdown)
            {
                var currentTime = DateTime.Now;
                var deltaTime = (currentTime - lastUpdate).TotalMilliseconds / 1000;

                accumulator += deltaTime;

                OnBeforeUpdate();

                //var worldObjects = World.Objects.GetObjectsInArea(World.Objects.Bounds);
                var worldObjects = World.Objects;

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
