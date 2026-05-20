using System;
using Godot;
using NBodySimulation.Core;
using NBodySimulation.Utils;
using NBodySimulation.Models;
using Color = Godot.Color;
//using Vector2 = NBodySimulation.Models.Vector2;
using Vector2 = Godot.Vector2;

namespace NBodySimulation.UI
{
    public partial class PlanetCreator : Control
    {
        // События
        public static event Action<Planet> OnPlanetCreated;
        public static event Action OnPlacementRequested;
        
        // Параметры планеты
        private float planetMass = 1e24f; // масса Земли ~5.97e24
        private float planetHue = 0.5f;
        private Vector2? pendingPlacement = null;
        
        // UI элементы
        private HSlider massSlider;
        private HSlider hueSlider;
        private Label massValue;
        private Label hueValue;
        private Control planetPreview;
        private Button createButton;
        private Button cancelButton;
        private Label statusLabel;
        
        public bool isPlacing = false;
        
        public override void _Ready()
        {
            // // Основной контейнер
            // var bg = new ColorRect
            // {
            //     Color = new Color(0.1f, 0.12f, 0.15f, 0.95f),
            //     Size = new Vector2(300, 400),
            //     Position = new Vector2(10, 100)
            // };
            // AddChild(bg);
            
            // Заголовок
            // var title = new Label
            // {
            //     Text = "🌍 Planet Creator",
            //     Position = new Vector2(20, 110),
            //     Modulate = new Color(0.8f, 0.9f, 1f)
            // };
            // AddChild(title);
            
            // Превью планеты
            planetPreview = new Control
            {
                Position = new Vector2(130, 150),
                Size = new Vector2(60, 60)
            };
            AddChild(planetPreview);
            planetPreview.Draw += DrawPlanetPreview;
            
            // Масса
            var massLabel = new Label
            {
                Text = "Mass (kg):",
                Position = new Vector2(20, 220),
                Modulate = new Color(0.7f, 0.8f, 1f)
            };
            AddChild(massLabel);
            
            massValue = new Label
            {
                Position = new Vector2(250, 220),
                Text = FormatMass(planetMass),
                Modulate = new Color(0.6f, 0.7f, 1f)
            };
            AddChild(massValue);
            
            massSlider = new HSlider
            {
                Position = new Vector2(20, 240),
                Size = new Vector2(260, 20),
                MinValue = 1e20f,
                MaxValue = 1e30f,
                Value = planetMass,
                Step = 1e19f,
                ExpEdit = true
            };
            massSlider.ValueChanged += (v) => 
            { 
                planetMass = (float)v;
                massValue.Text = FormatMass(planetMass);
                planetPreview.QueueRedraw();
            };
            AddChild(massSlider);
            
            // Цвет (Hue)
            var hueLabel = new Label
            {
                Text = "Color (Hue):",
                Position = new Vector2(20, 270),
                Modulate = new Color(0.7f, 0.8f, 1f)
            };
            AddChild(hueLabel);
            
            hueValue = new Label
            {
                Position = new Vector2(250, 270),
                Text = $"{planetHue:F2}",
                Modulate = new Color(0.6f, 0.7f, 1f)
            };
            AddChild(hueValue);
            
            hueSlider = new HSlider
            {
                Position = new Vector2(20, 290),
                Size = new Vector2(260, 20),
                MinValue = 0f,
                MaxValue = 1f,
                Value = planetHue,
                Step = 0.01f
            };
            hueSlider.ValueChanged += (v) => 
            { 
                planetHue = (float)v;
                hueValue.Text = $"{planetHue:F2}";
                planetPreview.QueueRedraw();
            };
            AddChild(hueSlider);
            
            // Кнопки
            createButton = new Button
            {
                Text = "Create & Place",
                Position = new Vector2(20, 330),
                Size = new Vector2(130, 40)
            };
            createButton.Pressed += () => StartPlacement();
            AddChild(createButton);
            
            cancelButton = new Button
            {
                Text = "❌ Cancel",
                Position = new Vector2(160, 330),
                Size = new Vector2(120, 40),
                Disabled = true,
                Modulate = new Color(0.8f, 0.8f, 0.8f)
            };
            cancelButton.Pressed += () => CancelPlacement();
            AddChild(cancelButton);
            
            // Статус
            statusLabel = new Label
            {
                Text = "Ready",
                Position = new Vector2(20, 380),
                Size = new Vector2(260, 20),
                Modulate = new Color(0.5f, 0.6f, 0.8f)
            };
            AddChild(statusLabel);
        }
        
        private void DrawPlanetPreview()
        {
            var center = planetPreview.Size / 2;
            float radius = CalculatePreviewRadius(planetMass);
            
            // Рисуем планету
            Color planetColor = Color.FromHsv(planetHue, 0.73f, 0.96f);
            planetPreview.DrawCircle(center, radius, planetColor);
            
            // Рисуем ободок
            planetPreview.DrawArc(center, radius + 2, 0, 360, 360, new Color(1, 1, 1, 0.3f), 1);
            
            // Подпись массы
            string massText = FormatMassShort(planetMass);
            var font = GetThemeDefaultFont();
            var textSize = font.GetStringSize(massText);
            planetPreview.DrawString(font, center - textSize/2, massText, modulate: new Color(1, 1, 1, 0.8f));
        }
        
        private float CalculatePreviewRadius(float mass)
        {
            // Радиус для превью (не физический, а для отображения)
            float logMass = MathF.Log10(mass);
            return 10f + (logMass - 20f) * 2f; // От 10 до 30 пикселей
        }
        
        private string FormatMass(float mass)
        {
            if (mass >= 1e27f) return $"{mass/1e27f:F2}×10²⁷";
            if (mass >= 1e24f) return $"{mass/1e24f:F2}×10²⁴";
            if (mass >= 1e21f) return $"{mass/1e21f:F2}×10²¹";
            return $"{mass:E2}";
        }
        
        private string FormatMassShort(float mass)
        {
            if (mass >= 1e27f) return $"{mass/1e27f:F1}Y";
            if (mass >= 1e24f) return $"{mass/1e24f:F1}Z";
            if (mass >= 1e21f) return $"{mass/1e21f:F1}E";
            return $"{mass:E1}";
        }
        
        private void StartPlacement()
        {
            isPlacing = true;
            createButton.Disabled = true;
            cancelButton.Disabled = false;
            statusLabel.Text = "Click on simulation area to place planet";
            
            // Подписываемся на глобальный ввод
            OnPlacementRequested?.Invoke();
        }
        
        private void CancelPlacement()
        {
            isPlacing = false;
            createButton.Disabled = false;
            cancelButton.Disabled = true;
            statusLabel.Text = "Ready";
        }
        
        public Planet CreatePlanetAt(Models.Vector2 position)
        {
            if (!isPlacing) return default;
            
            // Создаем планету
            var planet = new Planet(position, planetMass)
            {
                UUID = UUIDSystem.GetUUID(),
                Velocity = new Models.Vector2(),
                Acceleration = new Models.Vector2(),
                Color = Color.FromHsv(planetHue, 0.73f, 0.96f)
            };
            
            OnPlanetCreated?.Invoke(planet);
            CancelPlacement();
            
            return planet;
        }
    }
}