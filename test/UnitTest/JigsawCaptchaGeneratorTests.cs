using JigsawPuzzleCaptcha.Core;
using JigsawPuzzleCaptcha.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace JigsawPuzzleCaptcha.Tests;

public class JigsawCaptchaGeneratorTests
{
    private readonly JigsawCaptchaGenerator _generator = new();

    private static byte[] CreateTestImageBytes(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);
        image.Mutate(ctx => ctx.Fill(Color.Blue));
        using var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        return ms.ToArray();
    }

    [Fact]
    public void Generate_ValidImage_ReturnsBothImages()
    {
        CaptchaResult result = _generator.Generate(CreateTestImageBytes(400, 250));

        Assert.NotEmpty(result.BackgroundImageBytes);
        Assert.NotEmpty(result.PieceImageBytes);
    }

    [Fact]
    public void Generate_FromStream_ReturnsSameShapeAsByteArray()
    {
        using var stream = new MemoryStream(CreateTestImageBytes(400, 250));

        CaptchaResult result = _generator.Generate(stream);

        Assert.Equal(400, result.BackgroundWidth);
        Assert.Equal(250, result.BackgroundHeight);
    }

    [Fact]
    public void Generate_PieceCanvas_IsTallerThanPieceBodyToFitTheTab()
    {
        var options = new JigsawCaptchaOptions { PieceWidth = 60, PieceHeight = 60 };

        CaptchaResult result = _generator.Generate(CreateTestImageBytes(400, 300), options);

        using var piece = Image.Load<Rgba32>(result.PieceImageBytes);
        Assert.Equal(options.PieceWidth, piece.Width);
        Assert.True(piece.Height > options.PieceHeight, "The canvas must leave room for the top tab.");
        Assert.Equal(piece.Width, result.PieceWidth);
        Assert.Equal(piece.Height, result.PieceHeight);
    }

    [Fact]
    public void Generate_Piece_HasTransparentCornersAndOpaqueCentre()
    {
        CaptchaResult result = _generator.Generate(CreateTestImageBytes(400, 300));

        using var piece = Image.Load<Rgba32>(result.PieceImageBytes);
        Assert.Equal(0, piece[0, 0].A);
        Assert.True(piece[piece.Width / 2, piece.Height / 2].A > 0);
    }

    [Fact]
    public void Generate_Answer_StaysInsideTheImage()
    {
        var options = new JigsawCaptchaOptions { PieceWidth = 60, PieceHeight = 60, Padding = 20 };

        CaptchaResult result = _generator.Generate(CreateTestImageBytes(400, 250), options);

        Assert.InRange(result.X, 200, 400 - result.PieceWidth - options.Padding);
        Assert.InRange(result.Y, options.Padding, 250 - result.PieceHeight - options.Padding);
    }

    [Theory]
    [InlineData(40, 40)]
    [InlineData(60, 60)]
    [InlineData(80, 50)]
    public void Generate_CustomPieceSize_MatchesRequestedWidth(int pieceWidth, int pieceHeight)
    {
        var options = new JigsawCaptchaOptions { PieceWidth = pieceWidth, PieceHeight = pieceHeight };

        CaptchaResult result = _generator.Generate(CreateTestImageBytes(400, 300), options);

        using var piece = Image.Load<Rgba32>(result.PieceImageBytes);
        Assert.Equal(pieceWidth, piece.Width);
    }

    [Fact]
    public void Generate_ImageTooSmall_ThrowsWithAHelpfulMessage()
    {
        byte[] tiny = CreateTestImageBytes(100, 100);

        ArgumentException ex = Assert.Throws<ArgumentException>(() => _generator.Generate(tiny));

        Assert.Contains("too small", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Generate_EmptyInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => _generator.Generate(Array.Empty<byte>()));
    }

    [Fact]
    public void Generate_RepeatedCalls_ProduceDifferentAnswers()
    {
        byte[] source = CreateTestImageBytes(600, 400);

        var answers = new HashSet<int>();
        for (int i = 0; i < 20; i++)
        {
            answers.Add(_generator.Generate(source).X);
        }

        Assert.True(answers.Count > 1, "The answer must not be constant across calls.");
    }

    [Theory]
    [InlineData(100, 100, true)]
    [InlineData(105, 100, true)]
    [InlineData(95, 100, true)]
    [InlineData(106, 100, false)]
    [InlineData(94, 100, false)]
    public void Validate_AppliesTheConfiguredTolerance(int submitted, int expected, bool valid)
    {
        var generator = new JigsawCaptchaGenerator(new JigsawCaptchaOptions { Tolerance = 5 });

        Assert.Equal(valid, generator.Validate(submitted, expected));
    }

    [Fact]
    public void Options_InvalidTabRatio_Throws()
    {
        var options = new JigsawCaptchaOptions { TabRatio = 0.9f };

        Assert.Throws<ArgumentOutOfRangeException>(() => options.Validate());
    }
}
