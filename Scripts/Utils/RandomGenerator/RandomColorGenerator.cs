using System;
using Godot;

namespace NBodySimulation.Utils
{
    public static class RandomColorGenerator
    {
        public static float Saturation { get; private set; } = 0.73f;
        public static float Value { get; private set; } = 0.96f;
        private static Random random = new();
        public static Color GetRandomColor(float? hue = null, float? saturation = null, float? value = null)
        {
            float h = hue ?? random.NextSingle();
            float s = saturation ?? Saturation;
            float v = value ?? Value;
            return Color.FromHsv(h, s, v);
        }
    }
}