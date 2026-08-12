namespace JigsawPuzzleCaptcha.Shapes;

/// <summary>Identifies the outline used for a puzzle piece.</summary>
public enum PuzzleShapeKind
{
    /// <summary>
    /// Rectangle with an interlocking tab on the top edge and a notch on the left edge.
    /// The default, and the only shape available before piece shapes were configurable.
    /// </summary>
    Classic = 0,

    /// <summary>A plain rectangular cutout with no interlocking tab or notch.</summary>
    Square,

    /// <summary>An isosceles triangle inscribed in the piece's bounding box, apex up.</summary>
    Triangle,

    /// <summary>A regular, flat-top hexagon inscribed in the piece's bounding box.</summary>
    Hexagon,

    /// <summary>
    /// An ellipse inscribed in the piece's bounding box &#8212; a true circle when
    /// <c>PieceWidth</c> equals <c>PieceHeight</c>.
    /// </summary>
    Circle,
}
