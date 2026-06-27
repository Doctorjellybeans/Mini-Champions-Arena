using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;

public partial class DevConsole : Node
{
    public event Action<string> Output;
    public bool IsOpen { get; set; }

    private readonly Dictionary<string, Func<string[], string>> _commands = new();
    private Player _player;

    public override void _Ready()
    {
        RegisterCommand("noclip", _ =>
        {
            if (_player == null) return "Error: no player registered";
            _player.ToggleNoclip();
            return _player.IsNoclip ? "noclip ON" : "noclip OFF";
        });

        RegisterCommand("timescale", args =>
        {
            if (args.Length == 0)
                return $"timescale: current value is {Engine.TimeScale}";
            if (!float.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float ts))
                return $"timescale: invalid value '{args[0]}'";
            if (ts < 0)
                return "timescale: value must be >= 0";
            Engine.TimeScale = ts;
            return $"timescale set to {ts}";
        });

        RegisterCommand("tp", args =>
        {
            if (_player == null) return "Error: no player registered";
            if (args.Length != 3) return "tp: usage: tp <x> <y> <z>";
            if (!float.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) ||
                !float.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) ||
                !float.TryParse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                return "tp: all arguments must be valid numbers";
            _player.Teleport(new Vector3(x, y, z));
            return $"Teleported to ({x}, {y}, {z})";
        });

        RegisterCommand("give",          _ => "give: not implemented yet");
        RegisterCommand("god",           _ => "god: not implemented yet");
        RegisterCommand("infinite_ammo", _ => "infinite_ammo: not implemented yet");
        RegisterCommand("spawn_enemy",   _ => "spawn_enemy: not implemented yet");
    }

    /// <summary>
    /// Registers a command. Handler receives args (excluding the command name) and returns
    /// the string to display in the console output.
    /// </summary>
    public void RegisterCommand(string name, Func<string[], string> handler)
    {
        _commands[name.ToLower()] = handler;
    }

    public void ExecuteCommand(string rawInput)
    {
        string trimmed = rawInput.Trim();
        if (string.IsNullOrEmpty(trimmed)) return;

        Output?.Invoke($"[color=#aaaaaa]> {trimmed}[/color]");

        string[] parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string commandName = parts[0].ToLower();
        string[] args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

        if (_commands.TryGetValue(commandName, out var handler))
        {
            string result = handler(args);
            bool isError = result.StartsWith("Error:") || result.StartsWith("tp: usage")
                        || result.StartsWith("timescale: invalid") || result.StartsWith("timescale: value")
                        || result.StartsWith("tp: all") || result.StartsWith("tp: no");
            Output?.Invoke(isError ? $"[color=#ff6060]{result}[/color]" : result);
        }
        else
        {
            Output?.Invoke($"[color=#ff6060]Unknown command: {commandName}[/color]");
        }
    }

    public void RegisterPlayer(Player player) => _player = player;
    public void UnregisterPlayer(Player player) { if (_player == player) _player = null; }
}
