using Godot;

// Estado de movimiento en el suelo (IsOnFloor() == true).
// Maneja caminata WASD, deceleración suave y salto. Sin gravedad acumulada en piso.
public class GroundedState : MovementState
{
    public GroundedState(Player player, PlayerMovementConfig config, float gravity)
        : base(player, config, gravity) { }

    public override void PhysicsUpdate(double delta, bool inputLocked)
    {
        Vector3 velocity = _player.Velocity;

        // El salto solo se permite si hay input (consola cerrada)
        if (!inputLocked && Input.IsActionJustPressed("jump"))
            velocity.Y = _config.JumpVelocity;

        ApplyHorizontalMovement(ref velocity, inputLocked);

        _player.Velocity = velocity;
        _player.MoveAndSlide();
    }
}
