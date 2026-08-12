using JigsawPuzzleCaptcha.Shapes;

namespace JigsawPuzzleCaptcha.Core;

/// <summary>
/// The output of a CAPTCHA generation: two PNG images and the answer.
/// </summary>
/// <param name="BackgroundImageBytes">PNG of the source image with the puzzle slot cut out.</param>
/// <param name="PieceImageBytes">PNG of the puzzle piece, transparent outside the puzzle shape.</param>
/// <param name="X">
/// The correct horizontal position of the piece image's left edge, in background pixels.
/// This is the secret answer &#8212; keep it server-side and never send it to the browser.
/// </param>
/// <param name="Y">
/// The vertical position at which the piece image must be rendered, in background pixels.
/// This is not secret; the client needs it to place the piece.
/// </param>
/// <param name="PieceWidth">Width of the piece image in pixels.</param>
/// <param name="PieceHeight">Height of the piece image in pixels (includes the tab overhang, for shapes that have one).</param>
/// <param name="BackgroundWidth">Width of the background image in pixels.</param>
/// <param name="BackgroundHeight">Height of the background image in pixels.</param>
/// <param name="Shape">The outline used for the piece. Not secret; useful if the client renders shape-specific UI.</param>
public sealed record CaptchaResult(
    byte[] BackgroundImageBytes,
    byte[] PieceImageBytes,
    int X,
    int Y,
    int PieceWidth,
    int PieceHeight,
    int BackgroundWidth,
    int BackgroundHeight,
    PuzzleShapeKind Shape)
{
    /// <summary>The background image as a <c>data:</c> URI, ready to drop into an <c>img</c> tag.</summary>
    public string BackgroundDataUri => "data:image/png;base64," + Convert.ToBase64String(BackgroundImageBytes);

    /// <summary>The piece image as a <c>data:</c> URI, ready to drop into an <c>img</c> tag.</summary>
    public string PieceDataUri => "data:image/png;base64," + Convert.ToBase64String(PieceImageBytes);
}
