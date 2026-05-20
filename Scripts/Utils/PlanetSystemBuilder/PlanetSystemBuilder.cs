using System;
using System.Collections.Generic;
using Godot;
using NBodySimulation.Models;
using Color = Godot.Color;

using Vector2=NBodySimulation.Models.Vector2;
using Rect2=NBodySimulation.Models.Rect2;
using Environment=System.Environment;
using NBodySimulation.Core;

namespace NBodySimulation.Utils
{
    public class PlanetSystemBuilder
    {
        public int Seed { get; private set; }
        private int planetsToCreate;
        private Rect2 worldBounds;
        
        private bool createCentralStar = false;
        private bool initialVelocity = false;

        private const float DISTANCE_SCALE = SimulationManager.MetersPerPixel;
        private const float MASS_SCALE = DISTANCE_SCALE*DISTANCE_SCALE*DISTANCE_SCALE;

        public PlanetSystemBuilder(int count, Rect2 bounds, int? seed = null)
        {
            planetsToCreate = count;
            worldBounds = bounds;
            Seed = seed ?? Environment.TickCount;
        }

        public void AddCentralStar(bool yes = true) => createCentralStar = yes;
        public void AddInitialVelocity(bool yes = true) => initialVelocity = yes;

        public List<Planet> Generate()
        {
            Random random = new(Seed);
            var planets = new List<Planet>();
            
            if (createCentralStar)
            {
                planetsToCreate -= 1;
                Planet star = new Planet(worldBounds.Center, 1700*MASS_SCALE)
                {
                    UUID = UUIDSystem.GetUUID(),
                    Color = Color.Color8(245, 179, 66)
                };
                planets.Add(star);
            }

            for (int i = 0; i < planetsToCreate; i++)
            {
                
                float angle = (float)(random.NextDouble() * PhysicsCalculator.PI * 2);
                float radius = (float)((random.NextDouble()*0.8 + 0.2) * Math.Min(worldBounds.Half.X, worldBounds.Half.Y));

                Vector2 pos = worldBounds.Center + new Vector2(MathF.Cos(angle)*radius, MathF.Sin(angle)*radius);

                float mass = (random.NextSingle() * 190 + 10) * (1 - radius / 500)*1e-9f*MASS_SCALE;
                mass = MathF.Abs(mass);

                Vector2 vel = new Vector2();
                if (initialVelocity)
                {
                    float speed = MathF.Sqrt(2.6e-7f / radius) * MathF.Pow(DISTANCE_SCALE, 1.5f) / 2;
                    // float speed = MathF.Sqrt(PhysicsCalculator.G * mass / radius);
                    vel = new Vector2(-MathF.Sin(angle)*speed, MathF.Cos(angle)*speed);
                }

                Planet p = new Planet(pos, mass)
                {
                    Velocity = vel,
                    UUID = UUIDSystem.GetUUID()
                };
                planets.Add(p);
            }

            return planets;
        }
    }
}