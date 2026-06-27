using Godot;

public partial class Player : CharacterBody3D
{
    [Export] public float Speed = 5f;
    [Export] public float JumpVelocity = 4.5f;
    [Export] public float MouseSensitivity = 0.003f;

    private const float NoclipSpeed = 15f;

    private float _gravity;
    private Node3D _head;
    private readonly float _pitchLimit = Mathf.DegToRad(89f);

    private bool _isNoclip;
    private uint _savedCollisionLayer;
    private uint _savedCollisionMask;

    private DevConsole _console;
    private DebugOverlay _debugOverlay;

    public bool IsNoclip => _isNoclip;

    public override void _Ready()
    {
        _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
        _head = GetNode<Node3D>("CollisionShape3D/Head");
        _savedCollisionLayer = CollisionLayer;
        _savedCollisionMask  = CollisionMask;

        Input.MouseMode = Input.MouseModeEnum.Captured;

        _console = GetNodeOrNull<DevConsole>("/root/DevConsole");
        _console?.RegisterPlayer(this);

        _debugOverlay = GetNodeOrNull<DebugOverlay>("/root/DebugOverlay");
        _debugOverlay?.RegisterPlayer(this);
    }

    public override void _ExitTree()
    {
        _console?.UnregisterPlayer(this);
        _debugOverlay?.UnregisterPlayer(this);
    }

    public override void _Input(InputEvent @event)
    {
        if (_console?.IsOpen == true) return;

        if (@event is InputEventMouseMotion mouseMotion)
        {
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            _head.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
            Vector3 rot = _head.Rotation;
            rot.X = Mathf.Clamp(rot.X, -_pitchLimit, _pitchLimit);
            _head.Rotation = rot;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // Bloquea movimiento mientras la consola está abierta (el mundo sigue corriendo)
        if (_console?.IsOpen == true)
        {
            velocity.X = 0f;
            velocity.Z = 0f;
            // En noclip CollisionMask=0 → IsOnFloor() siempre es false → no acumular gravedad
            velocity.Y = (_isNoclip || IsOnFloor()) ? 0f : velocity.Y - _gravity * (float)delta;
            Velocity = velocity;
            MoveAndSlide();
            return;
        }

        // Modo noclip: movimiento libre en 6DOF, sin gravedad ni colisión
        if (_isNoclip)
        {
            Vector3 dir = Vector3.Zero;
            Vector2 inp = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
            dir += _head.GlobalTransform.Basis.Z * inp.Y;
            dir += _head.GlobalTransform.Basis.X * inp.X;
            if (Input.IsKeyPressed(Key.E)) dir += Vector3.Up;
            if (Input.IsKeyPressed(Key.Q)) dir -= Vector3.Up;
            Velocity = dir == Vector3.Zero ? Vector3.Zero : dir.Normalized() * NoclipSpeed;
            MoveAndSlide();
            return;
        }

        // Movimiento normal
        if (!IsOnFloor())
            velocity.Y -= _gravity * (float)delta;

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
            velocity.Y = JumpVelocity;

        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, Speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    public void ToggleNoclip()
    {
        _isNoclip = !_isNoclip;
        CollisionLayer = _isNoclip ? 0u : _savedCollisionLayer;
        CollisionMask  = _isNoclip ? 0u : _savedCollisionMask;
    }

    public void Teleport(Vector3 pos) => GlobalPosition = pos;
}
