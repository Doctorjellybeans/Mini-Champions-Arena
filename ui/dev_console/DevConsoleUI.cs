using Godot;

public partial class DevConsoleUI : CanvasLayer
{
    private Panel _panel;
    private RichTextLabel _output;
    private LineEdit _input;
    private DevConsole _console;

    public override void _Ready()
    {
        _panel  = GetNode<Panel>("ConsolePanel");
        _output = GetNode<RichTextLabel>("ConsolePanel/VBoxContainer/Output");
        _input  = GetNode<LineEdit>("ConsolePanel/VBoxContainer/InputRow/Input");

        _console = GetNode<DevConsole>("/root/DevConsole");
        _console.Output += AppendOutput;
        _input.TextSubmitted += OnSubmit;

        _panel.Visible = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true }) return;

        if (@event.IsActionPressed("open_console"))
        {
            ToggleConsole();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (_panel.Visible && @event is InputEventKey { Keycode: Key.Escape })
        {
            ToggleConsole();
            GetViewport().SetInputAsHandled();
        }
    }

    private void ToggleConsole()
    {
        _panel.Visible = !_panel.Visible;
        _console.IsOpen = _panel.Visible;

        if (_panel.Visible)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            _input.GrabFocus();
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            _input.Clear();
        }
    }

    private void OnSubmit(string text)
    {
        _console.ExecuteCommand(text);
        _input.Clear();
        _input.GrabFocus();
    }

    private void AppendOutput(string msg)
    {
        _output.AppendText(msg + "\n");
    }
}
