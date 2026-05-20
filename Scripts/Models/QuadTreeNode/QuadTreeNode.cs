using System.Linq;
using Godot;

namespace NBodySimulation.Models
{
    public class QuadTreeNode
    {
        public Rect2 Bounds { get; private set; }
        public Vector2 CenterOfMass { get; private set; }
        public float TotalMass { get; set; }
        /// <summary>
        /// Тело (если это лист)
        /// </summary>
        public Planet? Planet { get; set; }
        /// <summary>
        /// 4 подузла
        /// </summary>
        public QuadTreeNode[] Branches { get; private set; }

        public bool IsLeaf => Branches == null;
        public bool IsEmpty => Planet == null;
        
        public QuadTreeNode(Rect2 bounds)
        {
            Bounds = bounds;
            TotalMass = 0;
            CenterOfMass = new Vector2();
            Planet = null;
            Branches = null;
        }

        public void Split()
        {
            Vector2 defaultPos = Bounds.Position;
            Vector2 halfSize = Bounds.Half;
            
            Branches = new QuadTreeNode[4];
            // ld
            Branches[(int)Quadrants.LeftDown] = new QuadTreeNode(new Rect2(defaultPos, halfSize));
            // ru
            Branches[(int)Quadrants.RightUp] = new QuadTreeNode(new Rect2(defaultPos+halfSize, halfSize));
            // lu
            Vector2 LUPos = defaultPos + new Vector2(0, halfSize.Y);
            Branches[(int)Quadrants.LeftUp] = new QuadTreeNode(new Rect2(LUPos, halfSize));
            // rd
            Vector2 RDPos = defaultPos + new Vector2(halfSize.X, 0);
            Branches[(int)Quadrants.RightDown] = new QuadTreeNode(new Rect2(RDPos, halfSize));
        }

        public void Insert(Planet planet)
        {   
            //# Промежуточный узел
            if (!IsLeaf)
            {
                var newMass = TotalMass + planet.Mass;
                
                CenterOfMass = CenterOfMass*TotalMass + planet.Position*planet.Mass;
                CenterOfMass /= newMass;

                TotalMass = newMass;

                int branchIdx = (int)GetCorrectSector(planet.Position);
                Branches[branchIdx].Insert(planet);
                return;
            }
            
            //# Пустой лист
            if (IsEmpty)
            {
                this.Planet = planet;
                this.CenterOfMass = planet.Position;
                this.TotalMass = planet.Mass;
                return;
            }

            //# Не пустой лист
            Planet existPlanet = (Planet)this.Planet; // ситуация null уже обработана
            this.Planet = null;

            Split();

            // Теперь это промежуточный узел, можем вызвать вставку рекурсивно
            this.Insert(existPlanet);
            this.Insert(planet);
        }

        /// <summary>
        /// Определяет, в какой из 4 секторов поместить тело
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private Quadrants GetCorrectSector(Vector2 point)
        {
            Vector2 center = Bounds.Center;
            if (point.Y < center.Y)
            {
                // down
                if (point.X < center.X) return Quadrants.LeftDown;
                return Quadrants.RightDown;
            }
            {
                // up
                if (point.X < center.X) return Quadrants.LeftUp;
                return Quadrants.RightUp;
            }
        }
    }
}