using Godot;

// Contenedor de parámetros de movimiento. Debe seguir siendo un Resource de Godot
// para que el motor no pierda la referencia de clase al recompilar.
// Los valores reales vienen de los [Export] del nodo Player (ver Player.cs).
[GlobalClass]
public partial class PlayerMovementConfig : Resource
{
    public float WalkSpeed;
    public float JumpVelocity;
    public float MouseSensitivity;

    public float SlideMinEntrySpeed;
    public float SlideEntryBoost;
    public float SlideMinExitSpeed;
    public float SlideFrictionDeceleration;
    public float SlideCapsuleHeight;

    public float SprintSpeed;
    public float GroundAcceleration;
    public float AirAcceleration;
    public float AirSpeedCap;

    public float WallRunSpeed;
    public float WallRunGravity;
    public float WallRunMaxDuration;
    public float WallRunDetectionDistance;
    public float WallRunMinEntrySpeed;
    public float WallRunMaxNormalY;
    public float WallRunSameWallThreshold;
    public float WallRunCameraTilt;
    public float WallJumpHorizontalForce;
    public float WallJumpVerticalForce;
}
