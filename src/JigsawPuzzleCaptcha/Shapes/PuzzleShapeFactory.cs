using JigsawPuzzleCaptcha.Contracts;

namespace JigsawPuzzleCaptcha.Shapes;

/// <summary>
/// Factory Method for obtaining the <see cref="IPuzzlePieceShape"/> strategy that matches a
/// <see cref="PuzzleShapeKind"/>. Each shape is stateless, so instances are created once and
/// reused, keeping shape selection allocation-free on every <c>Generate</c> call.
/// </summary>
internal static class PuzzleShapeFactory
{
    private static readonly IPuzzlePieceShape Classic = new ClassicJigsawShape();
    private static readonly IPuzzlePieceShape Square = new SquarePieceShape();
    private static readonly IPuzzlePieceShape Triangle = new TrianglePieceShape();
    private static readonly IPuzzlePieceShape Hexagon = new HexagonPieceShape();
    private static readonly IPuzzlePieceShape Circle = new CirclePieceShape();

    /// <summary>Returns the shape strategy for <paramref name="kind"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="kind"/> is not a known shape.</exception>
    internal static IPuzzlePieceShape Create(PuzzleShapeKind kind) => kind switch
    {
        PuzzleShapeKind.Classic => Classic,
        PuzzleShapeKind.Square => Square,
        PuzzleShapeKind.Triangle => Triangle,
        PuzzleShapeKind.Hexagon => Hexagon,
        PuzzleShapeKind.Circle => Circle,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported puzzle shape."),
    };
}
