using System;
using System.Collections.Generic;
using Godot;
using System.Linq;
using NBodySimulation.Core;

namespace NBodySimulation.UI
{
    public partial class EnergyGraph : Control
    {
        // Настройки графика
        [Export] public int MaxPoints = 200;
        [Export] public float UpdateInterval = 0.1f; // секунды
        [Export] public Color TotalColor = Colors.Red;
        [Export] public Color GridColor = Colors.Gray;
        
        private List<float> totalEnergy = new();
        private List<float> timePoints = new();
        
        private float timeSinceLastUpdate = 0;
        private float minEnergy = float.MaxValue;
        private float maxEnergy = float.MinValue;
        
        // Для отрисовки
        private Vector2 graphArea;
        private Vector2 graphOffset = new(50, 30);
        
        public override void _Ready()
        {
            Size = new (400, 300);
            Position = new(1200, 400);
            
            SimulationManager.OnEnergyChanged += (e) => UpdateData(e);
        }
        
        // public override void _Process(double delta)
        // {
        //     if (SimulationManager.IsPaused && !SimulationManager.StepRequest) return;

        //     timeSinceLastUpdate += (float)delta;
            
        //     if (timeSinceLastUpdate >= UpdateInterval)
        //     {
        //         timeSinceLastUpdate = 0;
        //         UpdateData();
        //     }
        // }
        
        private void UpdateData(float total)
        {   
            totalEnergy.Add(total);
            timePoints.Add(timePoints.Count > 0 ? timePoints.Last() + UpdateInterval : 0);
            
            // Обновляем диапазон значений
            minEnergy = Math.Min(minEnergy, total*1.01f);
            maxEnergy = Math.Max(maxEnergy, total*0.99f);
            
            // Ограничиваем количество точек
            if (totalEnergy.Count > MaxPoints)
            {
                totalEnergy.RemoveAt(0);
                timePoints.RemoveAt(0);
            }
            
            QueueRedraw();
        }
        
        public override void _Draw()
        {
            if (totalEnergy.Count == 0) return;
            
            graphArea = new Vector2(
                Size.X - graphOffset.X * 2,
                Size.Y - graphOffset.Y * 2
            );
            
            DrawGrid();
            DrawLegend();
            
            // Рисуем графики
            DrawEnergyLine(totalEnergy, TotalColor);
        }
        
        private void DrawGrid()
        {
            // Вертикальные линии (время)
            int numLines = 5;
            for (int i = 0; i <= numLines; i++)
            {
                float x = graphOffset.X + (i * graphArea.X / numLines);
                DrawLine(
                    new Vector2(x, graphOffset.Y),
                    new Vector2(x, graphOffset.Y + graphArea.Y),
                    GridColor,
                    1,
                    true
                );
            }
            
            // Горизонтальные линии (энергия)
            for (int i = 0; i <= numLines; i++)
            {
                float y = graphOffset.Y + (i * graphArea.Y / numLines);
                DrawLine(
                    new Vector2(graphOffset.X, y),
                    new Vector2(graphOffset.X + graphArea.X, y),
                    GridColor,
                    1,
                    true
                );
            }
            
            // Подписи
            float timeRange = timePoints.Last() - timePoints.First();
            for (int i = 0; i <= numLines; i++)
            {
                float time = timePoints.First() + (i * timeRange / numLines);
                DrawString(
                    GetThemeDefaultFont(),
                    new Vector2(graphOffset.X + (i * graphArea.X / numLines) - 20, graphOffset.Y + graphArea.Y + 20),
                    $"{time:F1}s",
                    HorizontalAlignment.Center
                );
            }
            
            float energyRange = maxEnergy - minEnergy;
            for (int i = 0; i <= numLines; i++)
            {
                float energy = maxEnergy - (i * energyRange / numLines);
                DrawString(
                    GetThemeDefaultFont(),
                    new Vector2(graphOffset.X - 45, graphOffset.Y + (i * graphArea.Y / numLines) - 5),
                    $"{energy:e0}",
                    HorizontalAlignment.Right
                );
            }
        }
        
        private void DrawEnergyLine(List<float> energyData, Color color)
        {
            if (energyData.Count < 2) return;
            
            for (int i = 0; i < energyData.Count - 1; i++)
            {
                Vector2 p1 = GetGraphPoint(i, energyData[i]);
                Vector2 p2 = GetGraphPoint(i + 1, energyData[i + 1]);
                
                DrawLine(p1, p2, color, 2, true);
            }
        }
        
        private Vector2 GetGraphPoint(int index, float value)
        {
            float t = index / (float)(MaxPoints - 1);
            float x = graphOffset.X + t * graphArea.X;
            
            // Нормализуем значение энергии к высоте графика
            float y = graphOffset.Y + graphArea.Y;
            if (maxEnergy > minEnergy)
            {
                float norm = (value - minEnergy) / (maxEnergy - minEnergy);
                y = graphOffset.Y + (1 - norm) * graphArea.Y;
            }
            
            return new Vector2(x, y);
        }
        
        private void DrawLegend()
        {
            Vector2 startPos = new(graphOffset.X + graphArea.X - 100, graphOffset.Y + 10);
            
            DrawLegendItem(startPos + new Vector2(0, 40), "Total", TotalColor);
        }
        
        private void DrawLegendItem(Vector2 pos, string text, Color color)
        {
            DrawRect(new Rect2(pos, new Vector2(15, 15)), color);
            DrawString(GetThemeDefaultFont(), pos + new Vector2(20, 12), text);
        }
    }
}