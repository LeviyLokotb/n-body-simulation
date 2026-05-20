using System;
using System.Linq;

namespace NBodySimulation.Models
{
    public struct Vector2
    {
        /// <summary>Массив представляющий собой двумерный вектор</summary>
        private float[] _vector = [0, 0];
        /// <summary>Прослойка чтобы избежать null при создании объекта без конструктора</summary>
        private float[] vector { get => _vector ?? new float[2]; set => _vector = value; }
        private float abs;
        private void UpdateAbs()
            => abs = MathF.Sqrt( vector.Select(n => n*n).Sum() );
        /// <summary>
        /// Индексатор
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public float this[int i]
        {
            get => i < 0 || i > vector.Length ? throw new IndexOutOfRangeException() : vector[i] ;
            set
            {
                vector[i] = value;
                UpdateAbs();
            }
        }
        public float X { get => this[0]; set => this[0] = value; }
        public float Y { get => this[1]; set => this[1] = value; }
        /// <summary>
        /// Модуль вектора
        /// </summary>
        public float Abs => abs;
        /// <summary>
        /// Длина вектора (аналогичен Abs)
        /// </summary>
        public float Lenth => abs;
        /// <summary>
        /// Нормализованный вектор
        /// </summary>
        public Vector2 Norma 
            => Lenth == 0? new Vector2() : new Vector2(X / Lenth, Y / Lenth);

        public float DistanceTo(Vector2 point) 
            => (point - this).Abs;

        /// <summary>Новый 2-мерный вектор</summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Vector2(float x = 0, float y = 0) => (X, Y) = (x, y);
        /// <summary>Новый 2-мерный вектор из массива</summary>
        /// <param name="vec"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public Vector2(float[] vec)
        {
            if(vec.Length != vector.Length) throw new IndexOutOfRangeException();
            for(int i=0; i<vec.Length; i++) this[i] = vec[i];
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
            => new Vector2([.. a.ToArray().Select((_, i) => a[i] + b[i])]);
        public static Vector2 operator -(Vector2 a)
            => new Vector2([.. a.ToArray().Select(n => -n)]);
        public static Vector2 operator -(Vector2 a, Vector2 b)
            => a + -b;

        /// <summary>Скалярное произведение</summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float operator *(Vector2 a, Vector2 b)
        {
            float[] sum = [.. a.ToArray().Select((_, i) => a[i] * b[i])];
            return Enumerable.Sum(sum);
        }

        public static Vector2 operator *(Vector2 a, float b)
            => new Vector2([.. a.ToArray().Select(n => n*b)]);
        public static Vector2 operator *(float a, Vector2 b)
            => b*a;
        public static Vector2 operator /(Vector2 a, float b)
            => new Vector2([.. a.ToArray().Select(n => n/b)]);
        
        public static bool operator ==(Vector2 a, Vector2 b)
            => a.ToArray().Select( (_, i) => a[i] == b[i]).Aggregate((aa, bb) => aa && bb);
        public static bool operator !=(Vector2 a, Vector2 b) 
            => !(a==b);

        public static bool operator >(Vector2 a, Vector2 b)
            => a.ToArray().Select( (_, i) => a[i] > b[i]).Aggregate((aa, bb) => aa || bb);

        public static bool operator <(Vector2 a, Vector2 b)
            => a.ToArray().Select( (_, i) => a[i] < b[i]).Aggregate((aa, bb) => aa || bb);

        public static explicit operator string(Vector2 a)
            => $"<{string.Join(", ", a.vector.Select(n => n.ToString().Replace(",", ".")))}>";
        public override string ToString()
            => (string)this;
        
        public float[] ToArray()
            => vector;

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Vector2 n) return n == this;

            try 
            { 
                Vector2 v = (Vector2)obj; 
                return v == this;
            }
            catch { return false; }
        }

        public override int GetHashCode()
        {
            var Sha = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = System.Text.ASCIIEncoding.ASCII.GetBytes((string)this);
            byte[] hash_bytes = Sha.ComputeHash(bytes);
            return BitConverter.ToInt32(hash_bytes);
        }
    }
}