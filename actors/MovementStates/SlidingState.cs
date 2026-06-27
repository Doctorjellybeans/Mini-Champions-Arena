using Godot;

// Estado de slide: mantiene la velocidad de entrada y la desacelera con fricción propia.
// WASD no redirige — la dirección queda fija según el momento de entrada.
// Reduce la cápsula temporalmente para pasar bajo obstáculos bajos.
public class SlidingState : MovementState
{
    private float _originalCapsuleHeight;
    private Vector3 _originalShapePosition;

    public SlidingState(Player player, PlayerMovementConfig config, float gravity)
        : base(player, config, gravity) { }

    public override void Enter()
    {
        var shape = _player.GetNode<CollisionShape3D>("CollisionShape3D");
        var capsule = (CapsuleShape3D)shape.Shape;

        _originalCapsuleHeight = capsule.Height;
        _originalShapePosition = shape.Position;

        // Baja el nodo de CollisionShape3D para que la base de la cápsula
        // reducida siga tocando el suelo en el mismo punto
        float heightDiff = (_originalCapsuleHeight - _config.SlideCapsuleHeight) / 2f;
        capsule.Height = _config.SlideCapsuleHeight;
        shape.Position = _originalShapePosition with { Y = _originalShapePosition.Y - heightDiff };
    }

    public override void Exit()
    {
        var shape = _player.GetNode<CollisionShape3D>("CollisionShape3D");
        var capsule = (CapsuleShape3D)shape.Shape;

        capsule.Height = _originalCapsuleHeight;
        shape.Position = _originalShapePosition;
    }

    public override void PhysicsUpdate(double delta, bool inputLocked)
    {
        Vector3 velocity = _player.Velocity;

        if (inputLocked)
        {
            velocity.X = 0f;
            velocity.Z = 0f;
            _player.Velocity = velocity;
            _player.MoveAndSlide();
            return;
        }

        // Desacelera la magnitud horizontal sin cambiar la dirección del slide
        float hSpeed = new Vector2(velocity.X, velocity.Z).Length();
        float newSpeed = Mathf.Max(0f, hSpeed - _config.SlideFrictionDeceleration * (float)delta);
        if (hSpeed > 0f)
        {
            velocity.X = velocity.X / hSpeed * newSpeed;
            velocity.Z = velocity.Z / hSpeed * newSpeed;
        }

        _player.Velocity = velocity;
        _player.MoveAndSlide();
    }
}
