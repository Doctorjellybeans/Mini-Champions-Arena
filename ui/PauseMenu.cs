using Godot;

public partial class PauseMenu : Control
{
    public override void _Ready()
    {
        Visible = false;
        GetNode<Button>("Panel/MarginContainer/VBoxContainer/Resumir").Pressed += TogglePause;
        GetNode<Button>("Panel/MarginContainer/VBoxContainer/Salir").Pressed += () => GetTree().Quit();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Keycode: Key.Escape })
            TogglePause();
    }

    private void TogglePause()
    {
        Visible = !Visible;
        GetTree().Paused = Visible;
        Input.MouseMode = Visible ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
    }
}
