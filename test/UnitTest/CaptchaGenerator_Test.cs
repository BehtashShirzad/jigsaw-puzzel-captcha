using jigsaw_puzzel_net;
using jigsaw_puzzel_net.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Xunit;

namespace jigsaw_puzzel_net.Tests;

public class JigsawCaptchaGeneratorTests
{
    private readonly JigsawCaptchaGenerator _generator = new();

    private static byte[] CreateTestImageBytes(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);
        image.Mutate<Rgba32>(ctx => ctx.Fill(Color.Blue));
        using var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        return ms.ToArray();
    }

    [Fact]
    public void Generate_ValidImageInput_ReturnsNonNullCaptchaResult()
    {
        // Arrange
        byte[] sourceImageBytes = CreateTestImageBytes(400, 250);

        // Act
        CaptchaResult result = _generator.Generate(sourceImageBytes, pieceWidth: 60, pieceHeight: 60);
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.BackgroundImageBytes);
        Assert.NotNull(result.PieceImageBytes);
        Assert.NotEmpty(result.BackgroundImageBytes);
        Assert.NotEmpty(result.PieceImageBytes);
    }

    [Fact]
    public void Generate_TargetCoordinates_StayWithinExpectedImageBounds()
    {
        // Arrange
        int imageWidth = 400;
        int imageHeight = 200;
        int pieceWidth = 60;
        int pieceHeight = 60;
        int padding = 20;

        byte[] sourceImageBytes = CreateTestImageBytes(imageWidth, imageHeight);

        // Act
        CaptchaResult result = _generator.Generate(sourceImageBytes, pieceWidth, pieceHeight);

        // Assert
        int minX = imageWidth / 2;
        int maxX = imageWidth - pieceWidth - padding;
        int minY = padding;
        int maxY = imageHeight - pieceHeight - padding;

        Assert.InRange(result.X, minX, maxX);
        Assert.InRange(result.Y, minY, maxY);
    }

    [Fact]
    public void Generate_ReturnedBytes_AreValidLoadableImages()
    {
        // Arrange
        byte[] sourceImageBytes = CreateTestImageBytes(300, 200);

        // Act
        CaptchaResult result = _generator.Generate(sourceImageBytes, pieceWidth: 50, pieceHeight: 50);

        // Assert & Verify output images can be decoded
        using var bgImage = Image.Load<Rgba32>(result.BackgroundImageBytes);
        using var pieceImage = Image.Load<Rgba32>(result.PieceImageBytes);

        Assert.Equal(300, bgImage.Width);
        Assert.Equal(200, bgImage.Height);
        Assert.Equal(50, pieceImage.Width);
        Assert.Equal(50, pieceImage.Height);
    }

    [Theory]
    [InlineData(40, 40)]
    [InlineData(60, 60)]
    [InlineData(80, 50)]
    public void Generate_CustomPieceDimensions_GeneratesMatchingPieceSize(int pieceWidth, int pieceHeight)
    {
        // Arrange
        byte[] sourceImageBytes = CreateTestImageBytes(400, 300);

        // Act
        CaptchaResult result = _generator.Generate(sourceImageBytes, pieceWidth, pieceHeight);

        // Assert
        using var pieceImage = Image.Load<Rgba32>(result.PieceImageBytes);
        Assert.Equal(pieceWidth, pieceImage.Width);
        Assert.Equal(pieceHeight, pieceImage.Height);
    }
}