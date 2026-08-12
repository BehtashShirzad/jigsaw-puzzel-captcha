using JigsawPuzzleCaptcha.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;

namespace JigsawPuzzleCaptcha.Shapes;

/// <summary>
/// A regular, flat-top hexagon inscribed in the piece's bounding box: the top and bottom
/// edges span the middle half of the width, and the left/right vertices touch the box
/// edges at half height.
/// </summary>
internal sealed class HexagonPieceShape : IPuzzlePieceShape
{
    public (int CanvasWidth, int CanvasHeight) GetCanvasSize(int pieceWidth, int pieceHeight, float tabRatio) =>
        (pieceWidth, pieceHeight);

    public IPath CreatePath(int pieceWidth, int pieceHeight, float tabRatio)
    {
        float midY = pieceHeight / 2f;
        float quarterWidth = pieceWidth / 4f;

        return PolygonPath.Create(
            new PointF(quarterWidth, 0f),
            new PointF(pieceWidth - quarterWidth, 0f),
            new PointF(pieceWidth, midY),
            new PointF(pieceWidth - quarterWidth, pieceHeight),
            new PointF(quarterWidth, pieceHeight),
            new PointF(0f, midY));
    }
}
