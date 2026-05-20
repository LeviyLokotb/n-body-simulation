using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NBodySimulation.Utils;
using Vector2 = NBodySimulation.Models.Vector2;

namespace NBodySimulation.Models
{
    /// <summary>
    /// Тело, на которое действует сила 
    /// </summary>
    public struct Planet
    {
        public int UUID;
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }
        public float KineticEnergy => PhysicsCalculator.KineticEnergy(this);
        public float PotentialEnergy { get; set; }
        public float Mass { get; set; }
        public float Radius { get; set; }
        public Color Color {get; set; }

        // "След" для визуализации
        public Trail<Vector2> Trail;

        public Planet(Vector2 position, float mass)
        {
            Position = position;
            Velocity = new Vector2();
            Acceleration = new Vector2();
            Mass = mass;
            Radius = calculateRadius(mass);
            Color = RandomColorGenerator.GetRandomColor();
            ClearTrail();
        }

        private static float calculateRadius(float mass)
        {
            // Объём сферы ~ R^3
            // Плотность const
            // Значит радиус ~ mass^(1/3)
            return MathF.Pow(MathF.Abs(mass), 0.33f) * 2f;
        }

        public void UpdateTrail()
        {
            Trail.Add(Position);
        }

        public void ClearTrail()
        {
            Trail = new Trail<Vector2>(100);
        }

        public void UpdatePotentialEnergy(IEnumerable<Planet> planets)
        {
            PotentialEnergy = 0;
            for (int j = 0; j < planets.Count(); j++)
            {
                if (UUID == planets.ElementAt(j).UUID) continue;
                float e = PhysicsCalculator.PotentialEnergy(this, planets.ElementAt(j));
                // Потенциальная энергия планеты
                PotentialEnergy += e;
            }
        }
    }
}