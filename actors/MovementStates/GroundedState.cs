using Godot;

// Estado de movimiento en el suelo (IsOnFloor() == true).
// Maneja caminata/sprint WASD con aceleración suave y salto. Sin gravedad acumulada en piso.
public class GroundedState : MovementState
{
    public GroundedState(Player player, PlayerMovementConfig config, float gravity)
        : base(player, config, gravity) { }

    public override void Enter()
    {
        _player.LastWallNormal = Vector3.Zero;
    }

    public override void PhysicsUpdate(double delta, bool inputLocked)
    {
        Vector3 velocity = _player.Velocity;

        if (!inputLocked && Input.IsActionJustPressed("jump"))
            velocity.Y = _config.JumpVelocity;

        // Sprint activo solo si Shift está presionado y hay componente de movimiento adelante
        // (inputDir.Y < 0 en Godot = tecla move_forward). Strafe lateral y back siempre WalkSpeed.
        Vector2 inputDir = inputLocked
            ? Vector2.Zero
            : Input.GetVector("move_left", "move_right", "move_forward", "move_back");

        bool sprinting = !inputLocked && Input.IsActionPressed("sprint") && inputDir.Y < 0f;
        float targetSpeed = sprinting ? _config.SprintSpeed : _config.WalkSpeed;

        ApplyHorizontalMovement(ref velocity, delta, inputLocked, targetSpeed, _config.GroundAcceleration);

        _player.Velocity = velocity;
        _player.MoveAndSlide();
    }
}
