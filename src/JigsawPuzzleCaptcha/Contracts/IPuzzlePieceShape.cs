using SixLabors.ImageSharp.Drawing;

namespace JigsawPuzzleCaptcha.Contracts;

/// <summary>
/// Strategy for building a puzzle piece's outline. Implementations are stateless and
/// immutable, so they are cached and shared as singletons by
/// <see cref="JigsawPuzzleCaptcha.Shapes.PuzzleShapeFactory"/> rather than constructed per call.
/// </summary>
internal interface IPuzzlePieceShape
{
    /// <summary>
    /// Computes the piece canvas size for the given piece body dimensions. The canvas can be
    /// larger than the body when the outline protrudes outside its bounding box (e.g. the
    /// classic jigsaw tab); shapes that stay inside the box just return the body size.
    /// </summary>
    (int CanvasWidth, int CanvasHeight) GetCanvasSize(int pieceWidth, int pieceHeight, float tabRatio);

    /// <summary>Builds the outline path in the piece canvas's local coordinate space.</summary>
    IPath CreatePath(int pieceWidth, int pieceHeight, float tabRatio);
}
