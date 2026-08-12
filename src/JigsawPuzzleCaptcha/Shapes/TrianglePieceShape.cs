using JigsawPuzzleCaptcha.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;

namespace JigsawPuzzleCaptcha.Shapes;

/// <summary>An isosceles triangle inscribed in the piece's bounding box, apex up.</summary>
internal sealed class TrianglePieceShape : IPuzzlePieceShape
{
    public (int CanvasWidth, int CanvasHeight) GetCanvasSize(int pieceWidth, int pieceHeight, float tabRatio) =>
        (pieceWidth, pieceHeight);

    public IPath CreatePath(int pieceWidth, int pieceHeight, float tabRatio) => PolygonPath.Create(
        new PointF(pieceWidth / 2f, 0f),
        new PointF(pieceWidth, pieceHeight),
        new PointF(0f, pieceHeight));
}
