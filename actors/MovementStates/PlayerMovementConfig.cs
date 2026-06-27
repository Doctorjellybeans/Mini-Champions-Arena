using Godot;

// Perfil de parámetros de movimiento. Como Resource puede guardarse en archivos .tres
// para tener distintos "perfiles" (p. ej. caminar normal, sprint, etc.) en el futuro.
[GlobalClass]
public partial class PlayerMovementConfig : Resource
{
    // Velocidad de caminata en suelo (reemplaza a la antigua Speed)
    [Export] public float WalkSpeed = 5f;

    // Impulso vertical al saltar
    [Export] public float JumpVelocity = 4.5f;

    // Sensibilidad del mouse para el look (radianes por pixel)
    [Export] public float MouseSensitivity = 0.003f;

    // --- Parámetros de slide ---

    // Velocidad horizontal mínima para poder iniciar un slide
    [Export] public float SlideMinEntrySpeed = 3f;

    // Impulso de velocidad horizontal al entrar al slide (se suma a la velocidad actual)
    [Export] public float SlideEntryBoost = 3f;

    // Velocidad horizontal mínima para mantener el slide activo
    [Export] public float SlideMinExitSpeed = 1f;

    // Deceleración horizontal durante el slide (m/s²)
    [Export] public float SlideFrictionDeceleration = 6f;

    // Altura de la cápsula durante el slide (Godot default = 2.0)
    [Export] public float SlideCapsuleHeight = 1.0f;
}
