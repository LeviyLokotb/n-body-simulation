using System;
using NBodySimulation.Models;

namespace NBodySimulation.Utils
{
    /// <summary>
    /// Этот класс не только предоставляет физические функции и постоянные,
    /// но и позволяет настраивать эти постоянные (посмотреть что выйдет)
    /// </summary>
    public static class PhysicsCalculator
    {
        public static event Action<Planet, Planet> OnPlanetsTouch;
        
        /// <summary>
        /// Гравитационная постоянная
        /// <para>
        /// Значение по умолчанию: 6.6743e-11f
        /// </para>
        /// </summary>
        // Мы имеем право выбирать G для красивой симуляции,
        // оправдывая это изменением масштаба
        public const float DistanceScale = 1.0f;
        public const float G = 6.6743e-11f;
        public const float PI = MathF.PI;

        /// <summary>
        /// Вычисление силы тяготения для двух тел
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector2 ComputeGravity(Planet a, Planet b)
        {
            if (a.UUID == b.UUID) return new Vector2();
            
            Vector2 direction = b.Position - a.Position;
            float R = (float)direction.Lenth * DistanceScale;

            float force = G * a.Mass * b.Mass / (R*R);
            return direction.Norma * force;
        }

        // Гравитационная энергия
        // https://ru.wikipedia.org/wiki/%D0%93%D1%80%D0%B0%D0%B2%D0%B8%D1%82%D0%B0%D1%86%D0%B8%D0%BE%D0%BD%D0%BD%D0%B0%D1%8F_%D1%8D%D0%BD%D0%B5%D1%80%D0%B3%D0%B8%D1%8F?
        public static float KineticEnergy(Planet p)
        {
            return (float)(p.Mass * (p.Velocity.Lenth*DistanceScale * p.Velocity.Lenth*DistanceScale) / 2);
        }

        public static float PotentialEnergy(Planet a, Planet b)
        {
            return (float)(-G * a.Mass * b.Mass / (b.Position - a.Position).Lenth*DistanceScale);
        }

            // Для получения новой позиции будем использовать метод Стёрмера-Верле
            // https://ru.wikipedia.org/wiki/%D0%9C%D0%B5%D1%82%D0%BE%D0%B4_%D0%A1%D1%82%D1%91%D1%80%D0%BC%D0%B5%D1%80%D0%B0_%E2%80%94_%D0%92%D0%B5%D1%80%D0%BB%D0%B5
            // Если быть точнее, это скоростной метод Верле (позволяет точнее считать скорость)
    }
}