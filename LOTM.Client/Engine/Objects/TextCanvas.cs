using LOTM.Client.Engine.Objects.Components;
using LOTM.Shared.Engine.Math;
using LOTM.Shared.Engine.Objects;
using LOTM.Shared.Engine.Objects.Components;
using System.Collections.Generic;
using static LOTM.Client.Engine.Objects.Components.TextRenderer;

namespace LOTM.Client.Engine.Objects
{
    public class TextCanvas : GameObject
    {
        public string Text { get; set; }
        public bool Show { get; set; }

        public TextCanvas(int id, Vector2 position, string text = "")
            : base(id, position)
        {
            Text = text;
            Show = true;

            AddComponent(new TextRenderer(new List<Segment>
            {
                new Segment(text, "showcard_gothic", 5)
            }));
        }

        public override void OnUpdate(double deltaTime)
        {
            base.OnUpdate(deltaTime);

            var textRenderer = GetComponent<TextRenderer>();

            textRenderer.Segments[0].Text = Text;
            textRenderer.Segments[0].Active = Show;
        }

        public void SetPosition(Vector2 postion)
        {
            var pos = GetComponent<Transformation2D>().Position;
            pos.X = postion.X;
            pos.Y = postion.Y;
        }
    }
}
