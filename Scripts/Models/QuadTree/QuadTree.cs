using System;
using System.Collections.Generic;
using Godot;
using NBodySimulation.Utils;

namespace NBodySimulation.Models
{
    /// <summary>
    /// Квадродерево -- структура для оптимизации гравитационных взаимодействий
    /// <para>
    /// Состоит из QuadTreeNode. Строится из списка Planet
    /// </para>
    /// </summary>
    public class QuadTree
    {
        private QuadTreeNode root;
        /// <summary>
        /// Точность
        /// </summary>
        public float Theta { get; set; }

        public QuadTree(Rect2 bounds, float theta)
        {
            root = new QuadTreeNode(bounds);
            Theta = theta;
        }

        /// <summary>
        /// Строит дерево на основе списка тел. 
        /// При повторном вызове не удаляет предыдущие узлы 
        /// [ используйте Clear( ) ]
        /// </summary>
        /// <param name="planets"></param>
        public void Build(IEnumerable<Planet> planets)
        {
            foreach (Planet planet in planets)
            {
                root.Insert(planet);
            }
        }

        /// <summary>
        /// Удаляет все добавленные узлы
        /// </summary>
        public void Clear()
        {
            root = new QuadTreeNode(root.Bounds);
        }

        /// <summary>
        /// Возвращает вектор силы, приложенной к телу, 
        /// и потенциальную энергию
        /// </summary>
        /// <param name="planet"></param>
        /// <returns></returns>
        public Vector2 ComputeForce(Planet planet)
        {
            return computeForceRec(planet, root);
        }

        private Vector2 computeForceRec(Planet planet, QuadTreeNode node, int deep=0)
        {
            //# Узел пустой
            if ( node.IsLeaf && node.IsEmpty )
            {
                return new Vector2();
            }
            //# Лист с телом
            if (node.IsLeaf)
            {
                if (node.Planet.Value.Position == planet.Position)
                    return new Vector2();
                var f = PhysicsCalculator.ComputeGravity(planet, node.Planet.Value);
                return f;
            }

            //# Узел с узлами
            float distance = planet.Position.DistanceTo(node.CenterOfMass);
            float width = node.Bounds.Size.X;

            // Если расстояние достаточно большое, можно упростить:
            if (width / distance < Theta)
            {
                Planet pseudo = new Planet(node.CenterOfMass, node.TotalMass);
                var f = PhysicsCalculator.ComputeGravity(planet, pseudo);
                return f;
            }
            // Если недостаточно -- считаем для каждого подузла рекурсивно
            Vector2 totalForce = new();
            foreach (QuadTreeNode subNode in node.Branches)
            {
                if (subNode != null)
                {
                    var f = computeForceRec(planet, subNode, deep+1);
                    totalForce += f;
                }
            }
            return totalForce;
        }

        public void PrintAllNodes(){
            printAllNodesRec(root, 0);
        }

        private void printAllNodesRec(QuadTreeNode node, int lvl)
        {
            string type = "";
            if (node.IsEmpty && node.IsLeaf) type = "empty Leaf"; 
            else if (node.IsLeaf) type = "Leaf"; 
            else type = "Node"; 
            GD.Print($"{lvl} {type} {node.Bounds.Position}");
            
            if (type == "Node")
                foreach (var n in node.Branches)
                    printAllNodesRec(n, lvl+1);
        }

    }
}