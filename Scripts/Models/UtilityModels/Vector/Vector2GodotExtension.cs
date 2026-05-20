namespace NBodySimulation.Models
{
    public static class Vector2GodotExtension
    {
        public static Godot.Vector2 ToGodot(this Vector2 v){
            return new Godot.Vector2((float)v.X, (float)v.Y);
        }

        public static Vector2 FromGodot(this Godot.Vector2 v){
            return new Vector2(v.X, v.Y);
        }
    }
}