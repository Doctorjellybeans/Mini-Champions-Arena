using Godot;

public partial class Player : CharacterBody3D
{
    [ExportGroup("Movement")]
    [Export] public float WalkSpeed = 5f;
    [Export] public float JumpVelocity = 4.5f;
    [Export] public float MouseSensitivity = 0.003f;

    [ExportGroup("Slide")]
    [Export] public float SlideMinEntrySpeed = 3f;
    [Export] public float SlideEntryBoost = 3f;
    [Export] public float SlideMinExitSpeed = 1f;
    [Export] public float SlideFrictionDeceleration = 6f;
    [Export] public float SlideCapsuleHeight = 1.0f;

    private PlayerMovementConfig Config;

    private const float NoclipSpeed = 15f;

    private float _gravity;
    private Node3D _head;
    private readonly float _pitchLimit = Mathf.DegToRad(89f);

    private bool _isNoclip;
    private uint _savedCollisionLayer;
    private uint _savedCollisionMask;

    private DevConsole _console;
    private DebugOverlay _debugOverlay;

    // Máquina de estados de movimiento
    private MovementState _grounded;
    private MovementState _airborne;
    private MovementState _sliding;
    private MovementState _currentState;

    private CollisionShape3D _collisionShape;
    private float _originalCapsuleHeight;
    private float _originalShapeLocalY;

    public bool IsNoclip => _isNoclip;

    public override void _Ready()
    {
        _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
        _head = GetNode<Node3D>("CollisionShape3D/Head");
        _savedCollisionLayer = CollisionLayer;
        _savedCollisionMask  = CollisionMask;

        // Cachea forma de colisión y valores originales antes de cualquier slide
        _collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
        _originalCapsuleHeight = ((CapsuleShape3D)_collisionShape.Shape).Height;
        _originalShapeLocalY = _collisionShape.Position.Y;

        // Construye el config a partir de los campos exportados del inspector
        Config = new PlayerMovementConfig
        {
            WalkSpeed             = WalkSpeed,
            JumpVelocity          = JumpVelocity,
            MouseSensitivity      = MouseSensitivity,
            SlideMinEntrySpeed    = SlideMinEntrySpeed,
            SlideEntryBoost       = SlideEntryBoost,
            SlideMinExitSpeed     = SlideMinExitSpeed,
            SlideFrictionDeceleration = SlideFrictionDeceleration,
            SlideCapsuleHeight    = SlideCapsuleHeight,
        };

        // Instancia los estados de movimiento
        _grounded = new GroundedState(this, Config, _gravity);
        _airborne = new AirborneState(this, Config, _gravity);
        _sliding  = new SlidingState(this, Config, _gravity);
        _currentState = IsOnFloor() ? _grounded : _airborne;
        _currentState.Enter();

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
            RotateY(-mouseMotion.Relative.X * Config.MouseSensitivity);
            _head.RotateX(-mouseMotion.Relative.Y * Config.MouseSensitivity);
            Vector3 rot = _head.Rotation;
            rot.X = Mathf.Clamp(rot.X, -_pitchLimit, _pitchLimit);
            _head.Rotation = rot;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Modo noclip: override total, la máquina de estados no corre.
        // (Maneja también el caso consola-abierta para conservar el orden previo.)
        if (_isNoclip)
        {
            UpdateNoclip(delta);
            return;
        }

        // Movimiento normal delegado a la máquina de estados.
        // Con la consola abierta el input se bloquea pero igual se llama MoveAndSlide
        // (preserva el fix de glitch de velocidad).
        bool inputLocked = _console?.IsOpen == true;
        UpdateCurrentState(inputLocked);
        _currentState.PhysicsUpdate(delta, inputLocked);
    }

    // Gestiona todas las transiciones de estado, incluyendo entrada y salida del slide
    private void UpdateCurrentState(bool inputLocked)
    {
        if (_currentState == _sliding)
        {
            bool jumpPressed = !inputLocked && Input.IsActionJustPressed("jump");
            bool slideHeld   = !inputLocked && Input.IsActionPressed("slide");
            float hSpeed     = new Vector2(Velocity.X, Velocity.Z).Length();

            // Salto desde slide: conserva velocidad horizontal, transiciona a airborne
            if (jumpPressed)
            {
                Vector3 vel = Velocity;
                vel.Y = Config.JumpVelocity;
                Velocity = vel;
                _currentState.Exit();
                _currentState = _airborne;
                _currentState.Enter();
                return;
            }

            // Salida normal: tecla suelta, velocidad baja, o cayó por un borde
            if (!IsOnFloor() || !slideHeld || hSpeed < Config.SlideMinExitSpeed)
            {
                // No salir si un techo impide restaurar la cápsula (excepto si está en el aire)
                if (!IsOnFloor() || CanStandUp())
                {
                    _currentState.Exit();
                    _currentState = IsOnFloor() ? _grounded : _airborne;
                    _currentState.Enter();
                }
            }
            return;
        }

        // Transición normal grounded ↔ airborne
        MovementState desired = IsOnFloor() ? _grounded : _airborne;
        if (desired != _currentState)
        {
            _currentState.Exit();
            _currentState = desired;
            _currentState.Enter();
        }

        // Intentar entrar al slide (solo desde grounded, sin noclip, sin console)
        if (!inputLocked && _currentState == _grounded && !_isNoclip)
        {
            if (Input.IsActionJustPressed("slide"))
            {
                float hSpeed = new Vector2(Velocity.X, Velocity.Z).Length();
                if (hSpeed >= Config.SlideMinEntrySpeed)
                {
                    _currentState.Exit();
                    _currentState = _sliding;
                    _currentState.Enter();
                }
            }
        }
    }

    // Raycast hacia arriba para verificar si hay espacio suficiente para salir del slide
    private bool CanStandUp()
    {
        float slideTop    = _collisionShape.Position.Y + Config.SlideCapsuleHeight / 2f;
        float originalTop = _originalShapeLocalY + _originalCapsuleHeight / 2f;
        float checkDist   = originalTop - slideTop;
        if (checkDist <= 0f) return true;

        var spaceState = GetWorld3D().DirectSpaceState;
        Vector3 from = GlobalPosition + new Vector3(0f, slideTop, 0f);
        var query = PhysicsRayQueryParameters3D.Create(from, from + Vector3.Up * checkDist, CollisionMask);
        query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
        return spaceState.IntersectRay(query).Count == 0;
    }

    // Movimiento libre en 6DOF, sin gravedad ni colisión.
    // Si la consola está abierta queda congelado (mismo comportamiento que antes).
    private void UpdateNoclip(double delta)
    {
        if (_console?.IsOpen == true)
        {
            Velocity = Vector3.Zero;
            MoveAndSlide();
            return;
        }

        Vector3 dir = Vector3.Zero;
        Vector2 inp = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        dir += _head.GlobalTransform.Basis.Z * inp.Y;
        dir += _head.GlobalTransform.Basis.X * inp.X;
        if (Input.IsKeyPressed(Key.E)) dir += Vector3.Up;
        if (Input.IsKeyPressed(Key.Q)) dir -= Vector3.Up;
        Velocity = dir == Vector3.Zero ? Vector3.Zero : dir.Normalized() * NoclipSpeed;
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
