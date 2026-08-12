using JigsawPuzzleCaptcha.Shapes;

namespace JigsawPuzzleCaptcha.Options;

/// <summary>
/// Configuration for <see cref="JigsawCaptchaGenerator"/>.
/// </summary>
public sealed class JigsawCaptchaOptions
{
    /// <summary>
    /// The outline used for the puzzle piece. Default is <see cref="PuzzleShapeKind.Classic"/>.
    /// Can be set once at registration or overridden per call, e.g.
    /// <c>generator.Generate(bytes, new JigsawCaptchaOptions { Shape = PuzzleShapeKind.Hexagon })</c>.
    /// </summary>
    public PuzzleShapeKind Shape { get; set; } = PuzzleShapeKind.Classic;

    /// <summary>Width of the puzzle piece body, in pixels. Default is 60.</summary>
    public int PieceWidth { get; set; } = 60;

    /// <summary>Height of the puzzle piece body, in pixels. Default is 60.</summary>
    public int PieceHeight { get; set; } = 60;

    /// <summary>
    /// Minimum distance in pixels kept between the puzzle slot and the image edges. Default is 20.
    /// </summary>
    public int Padding { get; set; } = 20;

    /// <summary>
    /// Maximum horizontal error, in pixels, still accepted by
    /// <see cref="JigsawCaptchaGenerator.Validate(int, int, int?)"/>. Default is 5.
    /// </summary>
    public int Tolerance { get; set; } = 5;

    /// <summary>
    /// Size of the puzzle tab and notch, as a fraction of the smaller piece dimension.
    /// Must be between 0.05 and 0.30. Default is 0.15.
    /// </summary>
    public float TabRatio { get; set; } = 0.15f;

    /// <summary>Throws if any option is outside its supported range.</summary>
    /// <exception cref="ArgumentOutOfRangeException">An option value is invalid.</exception>
    public void Validate()
    {
        if (PieceWidth < 16) throw new ArgumentOutOfRangeException(nameof(PieceWidth), PieceWidth, "PieceWidth must be at least 16.");
        if (PieceHeight < 16) throw new ArgumentOutOfRangeException(nameof(PieceHeight), PieceHeight, "PieceHeight must be at least 16.");
        if (Padding < 0) throw new ArgumentOutOfRangeException(nameof(Padding), Padding, "Padding cannot be negative.");
        if (Tolerance < 0) throw new ArgumentOutOfRangeException(nameof(Tolerance), Tolerance, "Tolerance cannot be negative.");
        if (TabRatio is < 0.05f or > 0.30f) throw new ArgumentOutOfRangeException(nameof(TabRatio), TabRatio, "TabRatio must be between 0.05 and 0.30.");
        if (!Enum.IsDefined(Shape)) throw new ArgumentOutOfRangeException(nameof(Shape), Shape, "Unsupported puzzle shape.");
    }
}
