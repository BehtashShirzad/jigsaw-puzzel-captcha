namespace jigsaw_puzzel_net.Core;

public record CaptchaResult(
    byte[] BackgroundImageBytes,
    byte[] PieceImageBytes,
    int X,
    int Y
);