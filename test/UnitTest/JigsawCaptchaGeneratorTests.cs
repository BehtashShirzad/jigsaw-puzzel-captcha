using JigsawPuzzleCaptcha.Core;
using JigsawPuzzleCaptcha.Options;
using JigsawPuzzleCaptcha.Shapes;
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

    [Fact]
    public void Options_InvalidShape_Throws()
    {
        var options = new JigsawCaptchaOptions { Shape = (PuzzleShapeKind)999 };

        Assert.Throws<ArgumentOutOfRangeException>(() => options.Validate());
    }

    [Theory]
    [InlineData(PuzzleShapeKind.Square)]
    [InlineData(PuzzleShapeKind.Triangle)]
    [InlineData(PuzzleShapeKind.Hexagon)]
    [InlineData(PuzzleShapeKind.Circle)]
    public void Generate_NonClassicShape_CanvasMatchesPieceBodyExactly(PuzzleShapeKind shapeKind)
    {
        var options = new JigsawCaptchaOptions { PieceWidth = 60, PieceHeight = 60, Shape = shapeKind };

        CaptchaResult result = _generator.Generate(CreateTestImageBytes(400, 300), options);

        using var piece = Image.Load<Rgba32>(result.PieceImageBytes);
        Assert.Equal(options.PieceWidth, piece.Width);
        Assert.Equal(options.PieceHeight, piece.Height);
        Assert.Equal(shapeKind, result.Shape);
    }

    [Fact]
    public void Generate_ClassicShape_ResultReportsTheShapeUsed()
    {
        CaptchaResult result = _generator.Generate(CreateTestImageBytes(400, 300));

        Assert.Equal(PuzzleShapeKind.Classic, result.Shape);
    }

    [Theory]
    [InlineData(PuzzleShapeKind.Triangle)]
    [InlineData(PuzzleShapeKind.Hexagon)]
    [InlineData(PuzzleShapeKind.Circle)]
    public void Generate_ShapeNotFillingTheBoundingBox_HasTransparentCorners(PuzzleShapeKind shapeKind)
    {
        var options = new JigsawCaptchaOptions { PieceWidth = 60, PieceHeight = 60, Shape = shapeKind };

        CaptchaResult result = _generator.Generate(CreateTestImageBytes(400, 300), options);

        using var piece = Image.Load<Rgba32>(result.PieceImageBytes);
        Assert.Equal(0, piece[0, 0].A);
        Assert.True(piece[piece.Width / 2, piece.Height / 2].A > 0);
    }

    [Fact]
    public void Generate_SquareShape_FillsTheEntireCanvasOpaque()
    {
        var options = new JigsawCaptchaOptions { PieceWidth = 60, PieceHeight = 60, Shape = PuzzleShapeKind.Square };

        CaptchaResult result = _generator.Generate(CreateTestImageBytes(400, 300), options);

        using var piece = Image.Load<Rgba32>(result.PieceImageBytes);
        Assert.True(piece[0, 0].A > 0, "The square shape fills its full bounding box, so the corner must be opaque.");
    }

    /// <summary>Runtime, per-call shape switching (not just at DI registration time).</summary>
    [Fact]
    public void Generate_DifferentShapePerCall_OverridesTheRegisteredDefault()
    {
        var generator = new JigsawCaptchaGenerator(new JigsawCaptchaOptions { Shape = PuzzleShapeKind.Classic });
        byte[] source = CreateTestImageBytes(400, 300);

        CaptchaResult classic = generator.Generate(source);
        CaptchaResult hexagon = generator.Generate(source, new JigsawCaptchaOptions { Shape = PuzzleShapeKind.Hexagon });

        Assert.Equal(PuzzleShapeKind.Classic, classic.Shape);
        Assert.Equal(PuzzleShapeKind.Hexagon, hexagon.Shape);
    }

    /// <summary>
    /// Every defined <see cref="PuzzleShapeKind"/>, pulled via reflection so a future shape is
    /// covered automatically instead of relying on someone remembering to add an InlineData row.
    /// </summary>
    public static IEnumerable<object[]> AllShapeKinds()
    {
        foreach (PuzzleShapeKind kind in Enum.GetValues<PuzzleShapeKind>())
        {
            yield return new object[] { kind };
        }
    }

    [Theory]
    [MemberData(nameof(AllShapeKinds))]
    public void Generate_EveryShapeKind_ProducesAValidOpaquePieceReportingItsShape(PuzzleShapeKind shapeKind)
    {
        var options = new JigsawCaptchaOptions { PieceWidth = 60, PieceHeight = 60, Shape = shapeKind };

        CaptchaResult result = _generator.Generate(CreateTestImageBytes(400, 300), options);
        var back = Convert.ToBase64String(result.BackgroundImageBytes);
        var pi = Convert.ToBase64String(result.PieceImageBytes);
        using var piece = Image.Load<Rgba32>(result.PieceImageBytes);
        Assert.Equal(options.PieceWidth, piece.Width);
        Assert.True(piece.Height >= options.PieceHeight, "The canvas must never be smaller than the piece body.");
        Assert.Equal(shapeKind, result.Shape);
        Assert.True(piece[piece.Width / 2, piece.Height / 2].A > 0, "Every shape must be opaque at its centre.");
    }

    [Theory]
    [MemberData(nameof(AllShapeKinds))]
    public void Options_EveryShapeKind_ValidatesSuccessfully(PuzzleShapeKind shapeKind)
    {
        var options = new JigsawCaptchaOptions { Shape = shapeKind };

        Exception? exception = Record.Exception(() => options.Validate());

        Assert.Null(exception);
    }
}
