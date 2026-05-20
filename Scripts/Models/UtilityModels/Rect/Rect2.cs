namespace NBodySimulation.Models
{
    public class Rect2
    {
        public Vector2 Position { get; private set; }
        public Vector2 Size { get; private set; }

        public Vector2 Half => Size/2;
        public Vector2 Center => Position + Half;
        
        public float Xmin => Position.X;
        public float Xmax => Position.X + Size.X;
        public float Ymin => Position.Y;
        public float Ymax => Position.Y + Size.Y;

        public Rect2(Vector2 pos, Vector2 size)
        {
            Position = pos;
            Size = size;
        }

        public Rect2(float xmin, float ymin, float xmax, float ymax)
        {
            Position = new(xmin, ymin);
            Size = new(xmax-xmin, ymax-ymin);
        }
    }
}