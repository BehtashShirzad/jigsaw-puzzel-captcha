using JigsawPuzzleCaptcha.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;

namespace JigsawPuzzleCaptcha.Shapes;

/// <summary>A plain rectangular piece, with no interlocking tab or notch.</summary>
internal sealed class SquarePieceShape : IPuzzlePieceShape
{
    public (int CanvasWidth, int CanvasHeight) GetCanvasSize(int pieceWidth, int pieceHeight, float tabRatio) =>
        (pieceWidth, pieceHeight);

    public IPath CreatePath(int pieceWidth, int pieceHeight, float tabRatio) => PolygonPath.Create(
        new PointF(0f, 0f),
        new PointF(pieceWidth, 0f),
        new PointF(pieceWidth, pieceHeight),
        new PointF(0f, pieceHeight));
}
