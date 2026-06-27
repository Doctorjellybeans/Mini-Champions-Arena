using Godot;
using System.Collections.Generic;

public partial class AxisGizmo : Control
{
    private Camera3D _camera;
    private const float AxisLength = 55f;
    private const float LineWidth  = 2.5f;
    private const int   FontSize   = 13;

    public void SetCamera(Camera3D camera) => _camera = camera;

    public override void _Draw()
    {
        if (_camera == null) return;

        Vector2 center = Size / 2f;
        Basis basis = _camera.GlobalTransform.Basis;
        Vector3 camForward = -basis.Z; // cámara mira hacia -Z

        // Ordena los ejes por profundidad (painter's algorithm: dibuja los más alejados primero)
        var axes = new List<(Vector3 world, Color baseColor, string label, float depth)>
        {
            (Vector3.Right, new Color(0.88f, 0.31f, 0.31f), "X", Vector3.Right.Dot(camForward)),
            (Vector3.Up,    new Color(0.31f, 0.88f, 0.31f), "Y", Vector3.Up.Dot(camForward)),
            (Vector3.Back,  new Color(0.31f, 0.50f, 1.00f), "Z", Vector3.Back.Dot(camForward)),
        };
        axes.Sort((a, b) => a.depth.CompareTo(b.depth));

        foreach (var (worldAxis, baseColor, label, depth) in axes)
        {
            float alpha = depth > 0f ? 1f : 0.38f;
            DrawAxis(center, basis, worldAxis, new Color(baseColor, alpha), label);
        }
    }

    private void DrawAxis(Vector2 center, Basis camBasis, Vector3 worldAxis, Color color, string label)
    {
        // Proyecta el eje 3D al espacio 2D de pantalla
        float sx =  worldAxis.Dot(camBasis.X);
        float sy = -worldAxis.Dot(camBasis.Y); // flip Y: pantalla crece hacia abajo
        var tip = center + new Vector2(sx, sy) * AxisLength;

        DrawLine(center, tip, color, LineWidth, true);

        // Pequeño círculo en la punta para mayor claridad
        DrawCircle(tip, 3f, color);

        // Etiqueta desplazada ligeramente desde la punta
        Vector2 labelOffset = new Vector2(sx, sy).Normalized() * 10f + new Vector2(2f, 4f);
        DrawString(ThemeDB.FallbackFont, tip + labelOffset, label,
                   HorizontalAlignment.Left, -1, FontSize, color);
    }
}
