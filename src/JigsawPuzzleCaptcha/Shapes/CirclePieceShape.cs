using JigsawPuzzleCaptcha.Contracts;
using SixLabors.ImageSharp.Drawing;

namespace JigsawPuzzleCaptcha.Shapes;

/// <summary>
/// An ellipse inscribed in the piece's bounding box &#8212; a true circle when
/// <c>pieceWidth</c> equals <c>pieceHeight</c>.
/// </summary>
internal sealed class CirclePieceShape : IPuzzlePieceShape
{
    public (int CanvasWidth, int CanvasHeight) GetCanvasSize(int pieceWidth, int pieceHeight, float tabRatio) =>
        (pieceWidth, pieceHeight);

    public IPath CreatePath(int pieceWidth, int pieceHeight, float tabRatio) =>
        // EllipsePolygon's (x, y, width, height) ctor takes the *centre* and the full
        // width/height of the bounding box, not a top-left location.
        new EllipsePolygon(pieceWidth / 2f, pieceHeight / 2f, pieceWidth, pieceHeight);
}
