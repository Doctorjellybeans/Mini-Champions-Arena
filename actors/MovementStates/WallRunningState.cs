using Godot;

// Estado de wall-run: el jugador corre por una pared vertical con gravedad progresiva.
// La transición de entrada/salida la gestiona UpdateCurrentState en Player.cs.
public class WallRunningState : MovementState
{
    // Normal de la pared activa; asignada por Player antes de llamar Enter().
    public Vector3 WallNormal;

    // Tiempo transcurrido en la pared actual; leído por UpdateCurrentState para verificar expiración.
    public float Timer { get; private set; }

    private Node3D _head;
    private Tween _tween;

    public WallRunningState(Player player, PlayerMovementConfig config, float gravity)
        : base(player, config, gravity) { }

    public override void Enter()
    {
        Timer = 0f;
        _player._doubleJumpAvailable = true;

        _head ??= _player.GetNode<Node3D>("CollisionShape3D/Head");

        // Inclinar cámara hacia la pared: negativo si la pared está a la derecha del jugador
        float sign = WallNormal.Dot(_player.Transform.Basis.X) > 0f ? -1f : 1f;
        float tiltRad = Mathf.DegToRad(_config.WallRunCameraTilt) * sign;

        _tween?.Kill();
        _tween = _player.CreateTween();
        _tween.TweenProperty(_head, "rotation:z", tiltRad, 0.15f);
    }

    public override void Exit()
    {
        _tween?.Kill();
        _tween = _player.CreateTween();
        _tween.TweenProperty(_head, "rotation:z", 0f, 0.15f);
    }

    public override void PhysicsUpdate(double delta, bool inputLocked)
    {
        Timer += (float)delta;

        Vector3 velocity = _player.Velocity;

        // Gravedad progresiva: de WallRunGravity hasta gravedad normal a lo largo de MaxDuration
        float gravityLerp = Mathf.Min(Timer / _config.WallRunMaxDuration, 1f);
        velocity.Y -= Mathf.Lerp(_config.WallRunGravity, _gravity, gravityLerp) * (float)delta;

        if (inputLocked)
        {
            float step = _config.WallRunSpeed * 8f * (float)delta;
            velocity.X = Mathf.MoveToward(velocity.X, 0f, step);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0f, step);
        }
        else
        {
            // Dirección de carrera paralela a la pared (perpendicular a la normal, en plano horizontal)
            Vector3 wallAlong = WallNormal.Cross(Vector3.Up).Normalized();
            // Elegir el sentido que se alinea con la velocidad horizontal actual
            if (wallAlong.Dot(new Vector3(velocity.X, 0f, velocity.Z)) < 0f)
                wallAlong = -wallAlong;

            // Movimiento automático: siempre converge a WallRunSpeed sin depender del input.
            // Bajo el límite: acelera rápidamente. Sobre el límite: desacelera con WallRunFriction.
            float speedAlongWall = new Vector3(velocity.X, 0f, velocity.Z).Dot(wallAlong);
            float step = speedAlongWall > _config.WallRunSpeed
                ? _config.WallRunFriction * (float)delta
                : _config.WallRunSpeed * 8f * (float)delta;
            velocity.X = Mathf.MoveToward(velocity.X, wallAlong.X * _config.WallRunSpeed, step);
            velocity.Z = Mathf.MoveToward(velocity.Z, wallAlong.Z * _config.WallRunSpeed, step);
        }

        _player.Velocity = velocity;
        _player.MoveAndSlide();
    }

    // Aplica el impulso de wall-jump. Llamado por UpdateCurrentState justo antes de salir del estado.
    // Conserva el 50% de la velocidad horizontal previa para recompensar chains de wall-run.
    public void PerformWallJump()
    {
        Vector3 vel = _player.Velocity;
        vel.X = vel.X * 0.5f + WallNormal.X * _config.WallJumpHorizontalForce;
        vel.Y = _config.WallJumpVerticalForce;
        vel.Z = vel.Z * 0.5f + WallNormal.Z * _config.WallJumpHorizontalForce;
        _player.Velocity = vel;
    }
}
