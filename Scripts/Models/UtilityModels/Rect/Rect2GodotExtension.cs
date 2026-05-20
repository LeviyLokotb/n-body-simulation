namespace NBodySimulation.Models
{
    public static class Rect2GodotExtension
    {
        public static Godot.Rect2 ToGodot(this Rect2 r){
            return new Godot.Rect2(r.Position.ToGodot(), r.Size.ToGodot());
        }

        public static Rect2 FromGodot(this Godot.Rect2 r){
            return new Rect2(r.Position.FromGodot(), r.Size.FromGodot());
        }
    }
}