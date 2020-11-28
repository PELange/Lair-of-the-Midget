using GlmNet;
using LOTM.Shared.Engine.Math;

namespace LOTM.Client.Engine.Graphics
{
    public class OrthographicCamera
    {
        public class Viewport
        {
            public Vector2 TopLeft { get; set; }
            public Vector2 BottomRight { get; set; }

            public Viewport(Vector2 topLeft, Vector2 bottomRight)
            {
                TopLeft = topLeft;
                BottomRight = bottomRight;
            }
        }

        protected Viewport View { get; set; }

        protected mat4 ProjectionMatrix { get; set; }

        public OrthographicCamera(Viewport viewport)
        {
            SetViewport(viewport);
        }

        public void SetViewport(Viewport viewport)
        {
            View = viewport;
            UpdateProjectionMatrix();
        }

        public Viewport GetViewport()
        {
            return View;
        }

        public void UpdateProjectionMatrix()
        {
            ProjectionMatrix = glm.ortho((float)View.TopLeft.X, (float)View.BottomRight.X, (float)View.BottomRight.Y, (float)View.TopLeft.Y, -1.0f, 1.0f);
        }

        public mat4 GetProjectionMatrix()
        {
            return ProjectionMatrix;
        }
    }
}
