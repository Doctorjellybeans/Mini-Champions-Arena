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
        _console.ClearOutput += () => _output.Clear();
        _panel.Visible = false;
    }

    // _Input fires before LineEdit processes key events, so we can intercept keys that
    // LineEdit would otherwise consume (open_console toggle, Ctrl+C, Enter).
    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true } key) return;

        // Toggle console — intercept before LineEdit types the character into the field
        if (@event.IsActionPressed("open_console"))
        {
            ToggleConsole();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (!_panel.Visible) return;

        // Close on Ctrl+C — LineEdit would consume this as "copy"
        if (key.CtrlPressed && key.Keycode == Key.C)
        {
            ToggleConsole();
            GetViewport().SetInputAsHandled();
            return;
        }

        // Submit on Enter — intercept so LineEdit never releases focus
        if (key.Keycode is Key.Enter or Key.KpEnter)
        {
            string text = _input.Text.Trim();
            _input.Clear();
            if (!string.IsNullOrEmpty(text))
                _console.ExecuteCommand(text);
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_panel.Visible && @event is InputEventKey { Pressed: true, Keycode: Key.Escape })
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

    private void AppendOutput(string msg)
    {
        if (!string.IsNullOrEmpty(msg))
            _output.AppendText(msg + "\n");
    }
}
