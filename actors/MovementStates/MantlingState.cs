using Godot;

// Estado de mantle: mueve al jugador sobre un ledge mediante interpolación directa de posición.
// La transición de salida la gestiona Player.UpdateCurrentState al detectar IsComplete == true.
public class MantlingState : MovementState
{
    public Vector3 TargetPosition;
    public bool IsComplete { get; private set; }
    public Vector3 ExitVelocity { get; private set; }

    private Vector3 _startPos;
    private Vector3 _entryDir;
    private float _elapsed;

    public MantlingState(Player player, PlayerMovementConfig config, float gravity)
        : base(player, config, gravity) { }

    public override void Enter()
    {
        _startPos = _player.GlobalPosition;
        _elapsed = 0f;
        IsComplete = false;

        // Dirección horizontal al entrar (para la velocidad de salida)
        Vector3 hVel = new Vector3(_player.Velocity.X, 0f, _player.Velocity.Z);
        _entryDir = hVel.Length() > 0.01f
            ? hVel.Normalized()
            : new Vector3(-_player.Transform.Basis.Z.X, 0f, -_player.Transform.Basis.Z.Z).Normalized();
    }

    public override void PhysicsUpdate(double delta, bool inputLocked)
    {
        // El mantle completa aunque la consola esté abierta (inputLocked ignorado intencionalmente)
        _elapsed += (float)delta;
        float t = Mathf.Clamp(_elapsed / _config.MantleDuration, 0f, 1f);
        float smooth = t * t * (3f - 2f * t);

        _player.GlobalPosition = _startPos.Lerp(TargetPosition, smooth);
        _player.Velocity = Vector3.Zero;
        _player.MoveAndSlide();

        if (_elapsed >= _config.MantleDuration)
        {
            IsComplete = true;
            ExitVelocity = _entryDir * _config.MantleExitBoost;
        }
    }
}
