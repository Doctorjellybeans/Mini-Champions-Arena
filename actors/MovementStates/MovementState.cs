using Godot;

// Clase base abstracta de la máquina de estados de movimiento.
// No deriva de Node: es una clase C# plana instanciada y manejada por Player.cs.
// Aloja las referencias compartidas (player, config, gravedad) y la lógica horizontal
// común para evitar duplicarla entre GroundedState y AirborneState.
public abstract class MovementState
{
    protected readonly Player _player;
    protected readonly PlayerMovementConfig _config;
    protected readonly float _gravity;

    protected MovementState(Player player, PlayerMovementConfig config, float gravity)
    {
        _player = player;
        _config = config;
        _gravity = gravity;
    }

    // Se llama al entrar en el estado (transición). Vacío por ahora.
    public virtual void Enter() { }

    // Se llama al salir del estado (transición). Vacío por ahora.
    public virtual void Exit() { }

    // Actualización de física del estado. inputLocked es true cuando la consola dev
    // está abierta: en ese caso no se lee input nuevo, pero igual se llama MoveAndSlide.
    public abstract void PhysicsUpdate(double delta, bool inputLocked);

    // Movimiento horizontal compartido. Con inputLocked fuerza X/Z a 0 sin leer input
    // (replica el congelado de la consola); sin bloqueo aplica WASD con deceleración suave.
    protected void ApplyHorizontalMovement(ref Vector3 velocity, bool inputLocked)
    {
        if (inputLocked)
        {
            velocity.X = 0f;
            velocity.Z = 0f;
            return;
        }

        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        Vector3 direction = (_player.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * _config.WalkSpeed;
            velocity.Z = direction.Z * _config.WalkSpeed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, _config.WalkSpeed);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, _config.WalkSpeed);
        }
    }
}
