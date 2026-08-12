using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;

namespace JigsawPuzzleCaptcha.Shapes;

/// <summary>Builds a closed, straight-edged outline from a list of vertices.</summary>
internal static class PolygonPath
{
    internal static IPath Create(params PointF[] vertices)
    {
        var builder = new PathBuilder();
        builder.StartFigure();
        builder.AddLines(vertices);
        builder.CloseFigure();
        return builder.Build();
    }
}
