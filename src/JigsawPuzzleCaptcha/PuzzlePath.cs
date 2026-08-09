using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;

namespace JigsawPuzzleCaptcha;

/// <summary>
/// Builds the puzzle-piece outline: a rectangle with an outward tab on the top edge
/// and an inward notch on the left edge.
/// </summary>
/// <remarks>
/// The path is built in the coordinate space of the piece <em>canvas</em>, which is
/// <c>PieceWidth x (PieceHeight + tabOffset)</c>. The rectangular body starts at
/// <c>y = tabOffset</c> so the tab has room to bulge upward without being clipped.
/// </remarks>
internal static class PuzzlePath
{
    private const int ArcSegments = 24;

    internal static IPath Create(int pieceWidth, int pieceHeight, float tabRadius, int tabOffset)
    {
        float top = tabOffset;
        float bottom = tabOffset + pieceHeight;
        float right = pieceWidth;
        float midX = pieceWidth / 2f;
        float midY = top + (pieceHeight / 2f);

        var points = new List<PointF>
        {
            new(0f, top),
            new(midX - tabRadius, top),
        };

        // Top edge: semicircular tab bulging upward (out of the piece).
        AppendArc(points, new PointF(midX, top), tabRadius, 180f, 360f);

        points.Add(new PointF(right, top));
        points.Add(new PointF(right, bottom));
        points.Add(new PointF(0f, bottom));
        points.Add(new PointF(0f, midY + tabRadius));

        // Left edge, travelling upward: semicircular notch cutting inward.
        AppendArc(points, new PointF(0f, midY), tabRadius, 90f, -90f);

        points.Add(new PointF(0f, top));

        var builder = new PathBuilder();
        builder.StartFigure();
        for (int i = 0; i < points.Count - 1; i++)
        {
            builder.AddLine(points[i], points[i + 1]);
        }

        builder.CloseFigure();
        return builder.Build();
    }

    /// <summary>
    /// Appends a polyline approximation of a circular arc. The point at
    /// <paramref name="startDegrees"/> is skipped because the caller has already added it.
    /// </summary>
    private static void AppendArc(List<PointF> points, PointF center, float radius, float startDegrees, float endDegrees)
    {
        for (int i = 1; i <= ArcSegments; i++)
        {
            float degrees = startDegrees + ((endDegrees - startDegrees) * i / ArcSegments);
            float radians = degrees * MathF.PI / 180f;
            points.Add(new PointF(
                center.X + (radius * MathF.Cos(radians)),
                center.Y + (radius * MathF.Sin(radians))));
        }
    }
}
