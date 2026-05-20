using Godot;
using NBodySimulation.Core;

namespace NBodySimulation.UI
{
    public partial class InfoScreen : Control
    {
        private Label energyVal;
        public override void _Ready()
        {
            energyVal = new Label
            {
                Position = new Vector2(700, 10),
                Text = $"Energy: -- Дж",
                Modulate = new Color(0.6f, 0.7f, 1f)
            };
            AddChild(energyVal);
        }

        public override void _Process(double delta)
        {
            energyVal.Text = $"Energy: {SimulationManager.TotalEnergy} Дж";
        }
    }
}