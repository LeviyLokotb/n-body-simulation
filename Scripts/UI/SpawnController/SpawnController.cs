using System;
using Godot;

namespace NBodySimulation.UI
{
    public partial class SpawnController : Control
    {
        public static event Action<int?> OnPlanetsGenerateRequest;

        public override void _Ready()
        {
            var restartButton = new Button
            {
                Text = "🔁",
                Position = new Vector2(10, 50),
                Shortcut = new()
                {
                    Events = (Godot.Collections.Array)InputMap.ActionGetEvents("ui_restart")
                }
            };
            restartButton.Pressed += () =>
            {
                OnPlanetsGenerateRequest?.Invoke(null);
            };
            AddChild(restartButton);

        }
    }
}