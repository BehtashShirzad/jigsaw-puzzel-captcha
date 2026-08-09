using jigsaw_puzzel_net.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace jigsaw_puzzel_net;

public class JigsawCaptchaGenerator
{
    public CaptchaResult Generate(byte[] imageBytes, int pieceWidth = 60, int pieceHeight = 60)
    {
        using var sourceImage = Image.Load<Rgba32>(imageBytes);

        int padding = 20;
        int minX = sourceImage.Width / 2;
        int maxX = Math.Max(minX + 1, sourceImage.Width - pieceWidth - padding);
        int minY = padding;
        int maxY = Math.Max(minY + 1, sourceImage.Height - pieceHeight - padding);

        int targetX = Random.Shared.Next(minX, maxX);
        int targetY = Random.Shared.Next(minY, maxY);

        // 1. Build puzzle piece paths
        IPath localPath = CreatePuzzlePiecePath(pieceWidth, pieceHeight);
        IPath targetPath = localPath.Translate(targetX, targetY);

        // 2. Crop rectangular piece region from source
        using var pieceImage = sourceImage.Clone(ctx => 
            ctx.Crop(new Rectangle(targetX, targetY, pieceWidth, pieceHeight)));

        // 3. Create a mask image and draw the puzzle path onto it
        using var mask = new Image<Rgba32>(pieceWidth, pieceHeight);
        mask.Mutate(ctx => ctx.Fill(Color.White, localPath));

        // 4. Make pixels outside the puzzle path transparent using the mask
        for (int y = 0; y < pieceHeight; y++)
        {
            for (int x = 0; x < pieceWidth; x++)
            {
                if (mask[x, y].A == 0)
                {
                    pieceImage[x, y] = new Rgba32(0, 0, 0, 0); // Transparent
                }
            }
        }

        // 5. Draw white border around piece
        pieceImage.Mutate(ctx => 
            ctx.Draw(Color.White.WithAlpha(0.8f), 1.5f, localPath));

        // 6. Create background image with darkened target slot
        using var bgImage = sourceImage.Clone();
        bgImage.Mutate(ctx =>
        {
            ctx.Fill(Color.Black.WithAlpha(0.7f), targetPath);
            ctx.Draw(Color.White.WithAlpha(0.5f), 1.5f, targetPath);
        });

        // 7. Output to PNG streams
        using var bgMs = new MemoryStream();
        using var pieceMs = new MemoryStream();

        bgImage.Save(bgMs, new PngEncoder());
        pieceImage.Save(pieceMs, new PngEncoder());

        return new CaptchaResult(
            bgMs.ToArray(),
            pieceMs.ToArray(),
            targetX,
            targetY
        );
    }

    private static IPath CreatePuzzlePiecePath(int width, int height)
    {
        var pb = new PathBuilder();
        pb.StartFigure();

        float tabRadius = Math.Min(width, height) * 0.15f;
        float midX = width / 2f;

        // Top edge with outward circular tab (4th argument is rotation: 0f)
        pb.AddLine(0f, 0f, midX - tabRadius, 0f);
        pb.AddArc(new RectangleF(midX - tabRadius, -tabRadius, tabRadius * 2f, tabRadius * 2f), 180f, -180f, 0f);
        pb.AddLine(midX + tabRadius, 0f, width, 0f);

        // Right edge
        pb.AddLine(width, 0f, width, height);

        // Bottom edge
        pb.AddLine(width, height, 0f, height);

        // Left edge
        pb.AddLine(0f, height, 0f, 0f);

        pb.CloseFigure();
        return pb.Build();
    }
}