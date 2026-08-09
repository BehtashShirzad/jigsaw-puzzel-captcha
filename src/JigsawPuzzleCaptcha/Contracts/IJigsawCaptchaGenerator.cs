using JigsawPuzzleCaptcha.Core;
using JigsawPuzzleCaptcha.Options;

namespace JigsawPuzzleCaptcha.Contracts;

/// <summary>
/// Generates jigsaw puzzle CAPTCHA challenges and validates the answers.
/// Implementations are stateless and safe to register as a singleton.
/// </summary>
public interface IJigsawCaptchaGenerator
{
    /// <summary>Generates a challenge from an encoded image (PNG, JPEG, WebP, ...).</summary>
    /// <param name="imageBytes">The encoded source image.</param>
    /// <param name="options">Overrides the configured defaults when supplied.</param>
    /// <returns>The background image, the piece image, and the answer.</returns>
    CaptchaResult Generate(byte[] imageBytes, JigsawCaptchaOptions? options = null);

    /// <summary>Generates a challenge from a readable stream containing an encoded image.</summary>
    /// <param name="imageStream">The stream to read the source image from.</param>
    /// <param name="options">Overrides the configured defaults when supplied.</param>
    /// <returns>The background image, the piece image, and the answer.</returns>
    CaptchaResult Generate(Stream imageStream, JigsawCaptchaOptions? options = null);

    /// <summary>Checks a user-submitted position against the stored answer.</summary>
    /// <param name="submittedX">The horizontal position reported by the client.</param>
    /// <param name="expectedX">The <see cref="CaptchaResult.X"/> stored when the challenge was issued.</param>
    /// <param name="tolerance">Overrides the configured tolerance when supplied.</param>
    /// <returns><c>true</c> when the submitted position is within tolerance.</returns>
    bool Validate(int submittedX, int expectedX, int? tolerance = null);
}
