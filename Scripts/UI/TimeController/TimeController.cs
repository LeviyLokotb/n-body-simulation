using System;
using Godot;
using NBodySimulation.Core;

namespace NBodySimulation.UI
{
    public partial class TimeController : Control
    {
        public static event Action OnPauseSwitch;
        public static event Action OnStepRequest;
        public static event Action<float> OnTimeSpeedChanged;
        private float timeScale = SimulationManager.TimeScale;
        private float timeSpeedDelta = 0.05f*SimulationManager.TimeScale;

        private bool isPaused = true;

        public override void _Ready()
        {
            var pauseButton = new Button
            {
                Text = "⏸️",
                Position = new Vector2(10, 10),
                Shortcut = new()
                {
                    Events = (Godot.Collections.Array)InputMap.ActionGetEvents("ui_pause")
                }
            };
            pauseButton.Pressed += () =>
            {
                OnPauseSwitch?.Invoke();
            };
            AddChild(pauseButton);

            var stepButton = new Button
            {
                Text = "⏭️",
                Position = new Vector2(60, 10),
                Shortcut = new()
                {
                    Events = (Godot.Collections.Array)InputMap.ActionGetEvents("ui_step")
                }
            };
            stepButton.Pressed += () => { OnStepRequest?.Invoke(); };
            AddChild(stepButton);

            // Time Speed Slider
            var timeScaleVal = new Label
            {
                Position = new Vector2(260, 40),
                Text = $"{timeScale}",
                Modulate = new Color(0.6f, 0.7f, 1f)
            };
            AddChild(timeScaleVal);

            var timeSpeedSlider = new HSlider
            {
                Position = new Vector2(160, 15),
                Size = new Vector2(200, 20),
                MinValue = 0.01f*SimulationManager.TimeScale,
                MaxValue = 10.0f*SimulationManager.TimeScale,
                Value = timeScale,
                Step = timeSpeedDelta
            };
            timeSpeedSlider.ValueChanged += (v) => 
            { 
                timeScale = (float)v;
                OnTimeSpeedChanged?.Invoke(timeScale); 
                timeScaleVal.Text = $"{timeScale}"; 
            };
            AddChild(timeSpeedSlider);

            // Time Speed Down
            var timeSpeedDownButton = new Button
            {
                Text = "⏳➖",
                Position = new Vector2(110, 10),
                Shortcut = new()
                {
                    Events = (Godot.Collections.Array)InputMap.ActionGetEvents("ui_timespeed_down")
                }
            };
            timeSpeedDownButton.Pressed += () => 
            {
                timeScale -= timeSpeedDelta;
                timeScale = Math.Max(timeScale, 0);
                timeSpeedSlider.Value = timeScale;
                timeScaleVal.Text = $"{timeScale}";
                OnTimeSpeedChanged?.Invoke(timeScale);
            };
            AddChild(timeSpeedDownButton);

            // Time Speed Up
            var timeSpeedUpButton = new Button
            {
                Text = "⏳➕",
                Position = new Vector2(370, 10),
                Shortcut = new()
                {
                    Events = (Godot.Collections.Array)InputMap.ActionGetEvents("ui_timespeed_up")
                }
            };
            timeSpeedUpButton.Pressed += () => 
            {
                timeScale += timeSpeedDelta;
                timeSpeedSlider.Value = timeScale;
                timeScaleVal.Text = $"{timeScale}";
                OnTimeSpeedChanged?.Invoke(timeScale);
            };
            AddChild(timeSpeedUpButton);

        }
    }
}