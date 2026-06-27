using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public partial class DevConsole : Node
{
    public event Action<string> Output;
    public event Action ClearOutput;
    public bool IsOpen { get; set; }

    private readonly Dictionary<string, (Func<string[], string> Handler, string Description)> _commands = new();
    private Player _player;

    public override void _Ready()
    {
        RegisterCommand("clear", _ =>
        {
            ClearOutput?.Invoke();
            return string.Empty;
        }, "clear the console output");

        RegisterCommand("help", _ =>
        {
            var sb = new StringBuilder();
            foreach (var (name, (_, desc)) in _commands)
                sb.AppendLine($"  [color=#88aaff]{name}[/color] — {desc}");
            return sb.ToString().TrimEnd();
        }, "list all available commands");

        RegisterCommand("noclip", _ =>
        {
            if (_player == null) return "Error: no player registered";
            _player.ToggleNoclip();
            return _player.IsNoclip ? "noclip ON" : "noclip OFF";
        }, "toggle free-fly mode (no collision, no gravity). WASD + E/Q to move vertically");

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
        }, "set engine time scale (e.g. 0.25 for slow-mo, 1 for normal)");

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
        }, "teleport player to world coordinates");

        RegisterCommand("give",          _ => "give: not implemented yet",         "give item to player");
        RegisterCommand("god",           _ => "god: not implemented yet",           "toggle god mode (invincibility)");
        RegisterCommand("infinite_ammo", _ => "infinite_ammo: not implemented yet", "toggle infinite ammo");
        RegisterCommand("spawn_enemy",   _ => "spawn_enemy: not implemented yet",   "spawn enemy of given type");
    }

    /// <summary>
    /// Registers a command. Handler receives args (excluding the command name) and returns
    /// the string to display in the console output.
    /// </summary>
    public void RegisterCommand(string name, Func<string[], string> handler, string description = "")
    {
        _commands[name.ToLower()] = (handler, description);
    }

    public void ExecuteCommand(string rawInput)
    {
        string trimmed = rawInput.Trim();
        if (string.IsNullOrEmpty(trimmed)) return;

        Output?.Invoke($"[color=#aaaaaa]> {trimmed}[/color]");

        string[] parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string commandName = parts[0].ToLower();
        string[] args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

        if (_commands.TryGetValue(commandName, out var entry))
        {
            string result = entry.Handler(args);
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
