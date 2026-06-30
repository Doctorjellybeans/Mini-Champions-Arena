using Godot;

// Estado de movimiento en el aire (IsOnFloor() == false).
// Aplica gravedad y air-strafing estilo Quake: el input añade velocidad en la dirección deseada
// limitado por AirSpeedCap, sin tocar la velocidad horizontal preexistente (slide, sprint, etc).
public class AirborneState : MovementState
{
    private bool _firstFrame;

    public AirborneState(Player player, PlayerMovementConfig config, float gravity)
        : base(player, config, gravity) { }

    public override void Enter()
    {
        _firstFrame = true;
    }

    public override void PhysicsUpdate(double delta, bool inputLocked)
    {
        Vector3 velocity = _player.Velocity;

        if (!inputLocked && _player._doubleJumpAvailable && !_firstFrame && Input.IsActionJustPressed("jump"))
        {
            velocity.Y = _config.DoubleJumpVelocity;
            _player._doubleJumpAvailable = false;
        }

        velocity.Y -= _gravity * (float)delta;

        if (!inputLocked)
        {
            Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
            Vector3 wishDir = (_player.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

            if (wishDir != Vector3.Zero)
            {
                // Quake air-strafing: solo se añade velocidad en wishDir si la proyección
                // actual en esa dirección está por debajo de AirSpeedCap.
                // La velocidad heredada (slide/sprint) no se recorta — el cap aplica solo al
                // control nuevo que el input puede agregar por frame.
                float currentDot = new Vector3(velocity.X, 0f, velocity.Z).Dot(wishDir);
                float addSpeed = _config.AirSpeedCap - currentDot;

                if (addSpeed > 0f)
                {
                    float accelSpeed = _config.AirAcceleration * _config.AirSpeedCap * (float)delta;
                    accelSpeed = Mathf.Min(accelSpeed, addSpeed);
                    velocity.X += accelSpeed * wishDir.X;
                    velocity.Z += accelSpeed * wishDir.Z;
                }
            }
            // Sin input no hay fricción: la velocidad horizontal se conserva sin cambios.
        }

        _player.Velocity = velocity;
        _player.MoveAndSlide();
        _firstFrame = false;
    }
}
