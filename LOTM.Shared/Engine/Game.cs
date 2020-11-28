using LOTM.Shared.Engine.World;
using System;

namespace LOTM.Shared.Engine
{
    public abstract class Game
    {
        private bool ShouldShutdown { get; set; }

        protected float FixedUpdateDeltaTime { get; set; } = 50; //50 ms = 20 fps

        protected GameWorld World { get; }

        public void Start()
        {
            OnInit();

            var lastUpdate = DateTime.Now;
            double accumulator = 0.0;

            while (!ShouldShutdown)
            {
                var currentTime = DateTime.Now;
                var deltaTime = (currentTime - lastUpdate).TotalMilliseconds;

                accumulator += deltaTime;

                OnBeforeUpdate();

                while (accumulator >= FixedUpdateDeltaTime)
                {
                    OnFixedUpdate(FixedUpdateDeltaTime);
                    accumulator -= FixedUpdateDeltaTime;
                }

                OnUpdate(deltaTime);

                OnAfterUpdate();

                lastUpdate = currentTime;
            }

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
