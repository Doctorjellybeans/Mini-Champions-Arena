using Godot;

// Estado de movimiento en el aire (IsOnFloor() == false).
// Aplica gravedad y comparte el control horizontal con GroundedState. Sin salto.
public class AirborneState : MovementState
{
    public AirborneState(Player player, PlayerMovementConfig config, float gravity)
        : base(player, config, gravity) { }

    public override void PhysicsUpdate(double delta, bool inputLocked)
    {
        Vector3 velocity = _player.Velocity;

        // Gravedad: se acumula mientras se está en el aire
        velocity.Y -= _gravity * (float)delta;

        ApplyHorizontalMovement(ref velocity, inputLocked);

        _player.Velocity = velocity;
        _player.MoveAndSlide();
    }
}
