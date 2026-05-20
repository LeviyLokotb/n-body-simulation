using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Godot;
using NBodySimulation.Models;
using NBodySimulation.UI;
using NBodySimulation.Utils;

using Rect2 = NBodySimulation.Models.Rect2;
using Vector2 = NBodySimulation.Models.Vector2;

namespace NBodySimulation.Core
{
    public partial class SimulationManager : Node2D
    {
        // Параметры
        public const int MetersPerPixel = 16_000_000;
        public const int RadiusScale = 1;
        public int PlanetsCount { get; } = 50; 
        public static float TimeScale { get; private set; } = 5e5f;
        public float Theta { get; } = 0f;
        public static Rect2 worldBounds { get; private set; }

        // Переменные
        private Rect2 physicsWorldBounds;
        private int planetsCount;
        private List<Planet> planets;
        private QuadTree quadTree;
        private float heatEnergy = 0;
        public static float TotalEnergy { get; private set; }
        
        // UI
        private TimeController timeController;
        private SpawnController spawnController;
        public static bool IsPaused { get; private set; } = true;
        public static bool StepRequest { get; private set; } = false;

        public static event Action<float> OnEnergyChanged;

        private void CreatePlanets(int n)
        {
            GD.Print(":: Creating planets...");
            planetsCount = PlanetsCount;
            heatEnergy = 0;
            TotalEnergy = 0;
            planets = [];
            PlanetSystemBuilder psb = new(n, physicsWorldBounds);
            psb.AddCentralStar();
            psb.AddInitialVelocity();
            planets = psb.Generate();
            GD.Print($":: {n} planets created");
        }

        public override void _Ready()
        {
            GD.Print("Starting simulation...");
            worldBounds = new Rect2(100, 100, 1100, 1100);
            physicsWorldBounds = new Rect2(worldBounds.Position*MetersPerPixel, worldBounds.Size*MetersPerPixel);
            CreatePlanets(PlanetsCount);

            // Управление временем
            timeController = new TimeController();
            AddChild(timeController);
            TimeController.OnPauseSwitch += () => 
            {
                IsPaused = !IsPaused;
                GD.Print(IsPaused?":: Paused":":: Unpaused");
            };
            TimeController.OnStepRequest += () => 
            { 
                StepRequest = true;
                GD.Print(":: One step");
            };
            TimeController.OnTimeSpeedChanged += (delta) =>
            {
                TimeScale = delta;
                if (TimeScale < 0) TimeScale = 0;
            };

            // 
            spawnController = new SpawnController();
            AddChild(spawnController);
            SpawnController.OnPlanetsGenerateRequest += (n) =>
            {
                bool pauseState = IsPaused;
                IsPaused = true;
                
                CreatePlanets(n ?? PlanetsCount);

                IsPaused = pauseState;
            };

            //
            AddChild(new InfoScreen());
            AddChild(new EnergyGraph());

            quadTree = new QuadTree(physicsWorldBounds, Theta);
            GD.Print("Simulation started!");
            GD.Print(
            $"""
            ===========================
                QuadTree Theta: {Theta}
                Planets: {PlanetsCount}
            ===========================
            """);
        }

        public override void _Process(double delta)
        {
            // Решил отрисовывать по кадрам а не при обновлении физики
            QueueRedraw();
        }

        public override void _PhysicsProcess(double deltaD)
        {
            // Время
            if (IsPaused && !StepRequest)
                return;
            StepRequest = false;
            
            float delta = (float)deltaD;
            delta *= TimeScale;

            List<(Planet, Planet)> planetsToMerge = [];
            //object planetLocker = new();

            for (int i = 0; i < planets.Count; i++)
            {
                Planet planet = planets[i];

                Vector2 newPos;
                newPos = planet.Position + planet.Velocity * delta + 0.5f * planet.Acceleration * delta * delta;

                // Зацикливание по краям (тороидальная Вселенная)
                if (newPos.X > physicsWorldBounds.Xmax+planet.Radius) 
                {
                    newPos = new Vector2(physicsWorldBounds.Xmin-planet.Radius, newPos.Y);
                    planet.ClearTrail();
                }
                if (newPos.Y > physicsWorldBounds.Ymax+planet.Radius) 
                {
                    newPos = new Vector2(newPos.X, physicsWorldBounds.Ymin-planet.Radius);
                    planet.ClearTrail();
                }
                if (newPos.X < physicsWorldBounds.Xmin-planet.Radius) 
                {
                    newPos = new Vector2(physicsWorldBounds.Xmax+planet.Radius, newPos.Y);
                    planet.ClearTrail();
                }
                if (newPos.Y < physicsWorldBounds.Ymin-planet.Radius) 
                {
                    newPos = new Vector2(newPos.X, physicsWorldBounds.Ymax+planet.Radius);
                    planet.ClearTrail();
                }
                planet.Position = newPos;
                planet.UpdateTrail();

                //lock(planetLocker)
                //{
                planets[i] = planet; // Т.к это структура
                //}
            }
            
            // Строим квадродерево по новым позициям
            quadTree.Clear();
            quadTree.Build(planets);

            for (int i = 0; i < planets.Count; i++)
            {
                // Расстояние между планетами
                // (В этом цикле позиции уже установились)
                for (int j = i+1; j < planets.Count; j++)
                {
                    if (i==j) continue;

                    Planet a = planets[i];
                    Planet b = planets[j];
                    Vector2 dist = a.Position - b.Position;
                    if (dist.Lenth < a.Radius+b.Radius)
                        planetsToMerge.Add((a, b));
                }

                // Физика
                Planet planet = planets[i];

                Vector2 force = quadTree.ComputeForce(planet);

                Vector2 oldAccelerartion = planet.Acceleration;
                Vector2 newAcceleration = force / planet.Mass;

                Vector2 oldVelocity = planet.Velocity;
                Vector2 newVelocity = oldVelocity + (oldAccelerartion + newAcceleration)/2 * delta;

                planet.Velocity = newVelocity;
                planet.Acceleration = newAcceleration;

                //lock(planetLocker)
                //{
                planets[i] = planet; // Т.к это структура
                //}
            }
            updatePlanetsPotentialEnergy();

            float prevPotEn = planets.Sum( p => p.PotentialEnergy ) / 2;
            float prevKinEn = planets.Sum( p => p.KineticEnergy );
            // Производим слияния
            foreach (var (a, b) in planetsToMerge)
                mergePlanets(a, b);
            
            // При слиянии потенциальную энергию нужно пересчитать
            if (planetsToMerge.Count > 0) updatePlanetsPotentialEnergy();

            // Общая потенциальная (без повторов)
            float PotentialEnergy = planets.Sum( p => p.PotentialEnergy ) / 2;
            // Общая кинетическая
            float KineticEnergy = planets.Sum( p => p.KineticEnergy );

            // Тепловая энергия
            if (planetsToMerge.Count > 0) heatEnergy += prevPotEn - PotentialEnergy + prevKinEn - KineticEnergy;
            planetsToMerge.Clear();

            // Полная
            TotalEnergy = KineticEnergy + PotentialEnergy + heatEnergy;
            OnEnergyChanged?.Invoke(TotalEnergy);
        }

        private void updatePlanetsPotentialEnergy()
        {
            for (int i = 0; i < planets.Count; i++)
            {
                var planet = planets[i];

                planet.UpdatePotentialEnergy(planets);

                planets[i] = planet;
            }
        }

        /// <summary>
        /// Убирает старые планеты, создаёт новую, добавляет её в список
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Добавленная планета</returns>
        private Planet? mergePlanets(Planet a, Planet b)
        {
            if (!planets.Any(p => p.UUID == a.UUID) || 
                !planets.Any(p => p.UUID == b.UUID))
            {
                return null;
            }
            planetsCount--;
            
            // Параметры новой планеты
            float newMass = a.Mass + b.Mass;
            Vector2 newPos = (a.Position*a.Mass+b.Position*b.Mass)/newMass;
            // Получаем скорость через импульс
            Vector2 newVel = (a.Velocity*a.Mass + b.Velocity*b.Mass)/newMass;
            // И ускорение через силу
            Vector2 newAcc = (a.Acceleration*a.Mass + b.Acceleration*b.Mass)/newMass;

            Color newColor = (a.Color*a.Mass + b.Color*b.Mass)/newMass;

            Planet newPlanet = new Planet(newPos, newMass)
            {
                UUID = UUIDSystem.GetUUID(),
                Position = newPos,
                Velocity = newVel,
                Acceleration = newAcc,
                Mass = newMass,
                Color = newColor,
                PotentialEnergy = 0
            };
            newPlanet.UpdateTrail();

            // Энергия новой планеты
            // float potEnAfter = 0;

            planets.RemoveAll(p => p.UUID==a.UUID);
            planets.RemoveAll(p => p.UUID==b.UUID);

            newPlanet.UpdatePotentialEnergy(planets);

            planets.Add(newPlanet);

            GD.Print($":: Planets #{a.UUID} & #{b.UUID} merged. Created planet #{newPlanet.UUID}");

            return newPlanet;
        }

        //# Draw
        public override void _Draw()
        {
            // Сцена
            DrawRect(worldBounds.ToGodot(), Color.Color8(13, 17, 56));

            if (planets == null) return;
            foreach (Planet planet in planets)
            {
                // След
                var tr = planet.Trail;
                //GD.Print(string.Join(' ', tr.ToList()));
                if (tr.Count >= 2)
                {
                    for (int i = 0; i<tr.Count-1; i++)
                    {
                        DrawLine((tr[i]/MetersPerPixel).ToGodot(), (tr[i+1]/MetersPerPixel).ToGodot(), Color.Color8(177, 225, 252));
                        // 177, 225, 252
                        //DrawCircle(tr[i].ToGodot(), planet.Radius, Color.Color8(145, 145, 113));
                    }
                }

                // Планета
                DrawCircle((planet.Position/MetersPerPixel).ToGodot(), planet.Radius*RadiusScale/MetersPerPixel, planet.Color);

                // Ускорение
                Vector2 drawAcc = planet.Acceleration.Norma * planet.Radius*RadiusScale/MetersPerPixel * 1.5f;
                drawAcc += planet.Position/MetersPerPixel;
                DrawLine((planet.Position/MetersPerPixel).ToGodot(), drawAcc.ToGodot(), Color.Color8(219, 24, 96));
            }
        }
    }
}