using Godot;

public partial class DebugOverlay : CanvasLayer
{
    private Panel _panel;
    private Label _labelFps;
    private Label _labelSpeed;
    private Label _labelVelX, _labelVelY, _labelVelZ;
    private Label _labelPos;

    private AxisGizmo _axisGizmo;
    private Player _player;

    private const string BasePath = "OverlayPanel/MarginContainer/VBoxContainer/";

    public override void _Ready()
    {
        _panel      = GetNode<Panel>("OverlayPanel");
        _labelFps   = GetNode<Label>(BasePath + "LabelFPS");
        _labelSpeed = GetNode<Label>(BasePath + "LabelSpeed");
        _labelVelX  = GetNode<Label>(BasePath + "LabelVelX");
        _labelVelY  = GetNode<Label>(BasePath + "LabelVelY");
        _labelVelZ  = GetNode<Label>(BasePath + "LabelVelZ");
        _labelPos   = GetNode<Label>(BasePath + "LabelPos");

        _axisGizmo = GetNode<AxisGizmo>("AxisGizmo");

        _panel.Visible     = false;
        _axisGizmo.Visible = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("open_metrics"))
        {
            bool show = !_panel.Visible;
            _panel.Visible     = show;
            _axisGizmo.Visible = show;
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Process(double delta)
    {
        if (!_panel.Visible || _player == null) return;

        Vector3 vel = _player.Velocity;
        Vector3 pos = _player.GlobalPosition;

        _labelFps.Text   = $"FPS: {Engine.GetFramesPerSecond()}";
        _labelSpeed.Text = $"Speed: {vel.Length():F2} m/s";
        _labelVelX.Text  = $"Vel X: {vel.X:F2} m/s";
        _labelVelY.Text  = $"Vel Y: {vel.Y:F2} m/s";
        _labelVelZ.Text  = $"Vel Z: {vel.Z:F2} m/s";
        _labelPos.Text   = $"Pos: ({pos.X:F2}, {pos.Y:F2}, {pos.Z:F2})";

        var camera = GetViewport().GetCamera3D();
        if (camera != null)
        {
            _axisGizmo.SetCamera(camera);
            _axisGizmo.QueueRedraw();
        }
    }

    public void RegisterPlayer(Player player) => _player = player;
    public void UnregisterPlayer(Player player) { if (_player == player) _player = null; }
}
