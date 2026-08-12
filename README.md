# JigsawPuzzleCaptcha

[![NuGet](https://img.shields.io/nuget/v/JigsawPuzzleCaptcha.svg)](https://www.nuget.org/packages/JigsawPuzzleCaptcha/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Slide-to-fit jigsaw puzzle CAPTCHA for .NET. Give it any image and it returns two PNGs — a background with a puzzle-shaped hole, and the matching piece — plus the answer to store server-side.

Built on ImageSharp, so it runs on Linux, macOS and Windows with no `System.Drawing` dependency.

## Install

```bash
dotnet add package JigsawPuzzleCaptcha
```

Targets `net8.0` and `net10.0`.

## Quick start

```csharp
using JigsawPuzzleCaptcha;

var generator = new JigsawCaptchaGenerator();

byte[] source = File.ReadAllBytes("background.jpg");
CaptchaResult captcha = generator.Generate(source);

// Send these to the browser:
//   captcha.BackgroundDataUri
//   captcha.PieceDataUri
//   captcha.Y            (where to draw the piece)
//
// Keep this on the server, keyed by a challenge id:
//   captcha.X            (the answer)
```

Later, when the user drops the piece:

```csharp
bool ok = generator.Validate(submittedX, storedX);
```

## ASP.NET Core

```csharp
builder.Services.AddJigsawCaptcha(options =>
{
    options.PieceWidth = 60;
    options.PieceHeight = 60;
    options.Tolerance = 5;
});
```

```csharp
app.MapGet("/captcha", (IJigsawCaptchaGenerator captcha, IDistributedCache cache) =>
{
    CaptchaResult result = captcha.Generate(File.ReadAllBytes("wwwroot/bg.jpg"));

    var id = Guid.NewGuid().ToString("N");
    cache.SetString(id, result.X.ToString(), new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
    });

    // Note: X is deliberately not returned.
    return Results.Ok(new { id, background = result.BackgroundDataUri, piece = result.PieceDataUri, y = result.Y });
});

app.MapPost("/captcha/verify", (VerifyRequest req, IJigsawCaptchaGenerator captcha, IDistributedCache cache) =>
{
    var stored = cache.GetString(req.Id);
    if (stored is null) return Results.BadRequest("Challenge expired.");

    cache.Remove(req.Id); // one attempt per challenge
    return captcha.Validate(req.X, int.Parse(stored)) ? Results.Ok() : Results.BadRequest("Wrong position.");
});

record VerifyRequest(string Id, int X);
```

## Options

| Option | Default | Purpose |
| --- | --- | --- |
| `Shape` | `Classic` | Outline of the piece — see [Piece shapes](#piece-shapes). |
| `PieceWidth` | `60` | Width of the piece body, in pixels. |
| `PieceHeight` | `60` | Height of the piece body, in pixels. |
| `Padding` | `20` | Minimum gap kept between the slot and the image edges. |
| `Tolerance` | `5` | Horizontal error, in pixels, still accepted by `Validate`. |
| `TabRatio` | `0.15` | Tab and notch size, as a fraction of the smaller piece dimension. Only affects `Classic`. |

Options can be set once at registration, or passed per call: `generator.Generate(bytes, options)`.

## Piece shapes

`JigsawCaptchaOptions.Shape` (`JigsawPuzzleCaptcha.Shapes.PuzzleShapeKind`) picks the piece outline:

| Shape | Description |
| --- | --- |
| `Classic` (default) | Rectangle with an interlocking tab on top and a notch on the left — the original puzzle look. |
| `Square` | A plain rectangular cutout, no tab or notch. |
| `Triangle` | An isosceles triangle inscribed in the piece's bounding box. |
| `Hexagon` | A regular, flat-top hexagon inscribed in the piece's bounding box. |
| `Circle` | An ellipse inscribed in the piece's bounding box &#8212; a true circle when `PieceWidth == PieceHeight`. |

Shapes are resolved through an internal factory keyed by `PuzzleShapeKind`, so picking one is just setting `Shape` on the options passed to `Generate` — no extra registration needed, and it can change per call/request, not only at DI setup time:

```csharp
CaptchaResult result = generator.Generate(source, new JigsawCaptchaOptions
{
    Shape = PuzzleShapeKind.Hexagon,
});
```

Only `Classic` needs extra canvas height for the tab overhang; the other shapes stay inside `PieceWidth x PieceHeight`.

## Result

`CaptchaResult` carries the two PNGs as `byte[]` (with `BackgroundDataUri` / `PieceDataUri` convenience properties), the answer `X`, the render position `Y`, the dimensions of both images, and the `Shape` that was used.

`PieceHeight` on the result can be larger than the configured `PieceHeight` for shapes that stick out of their bounding box (currently just `Classic`, whose canvas includes the tab).

## Security notes

This is a friction device, not an authentication mechanism. To get real value from it:

- **Never send `X` to the client.** Store it server-side against a challenge id. If it reaches the browser, the CAPTCHA is decorative.
- **One attempt per challenge.** Delete the stored answer on the first verification, pass or fail.
- **Expire challenges** after a minute or two.
- **Rate-limit** both the issue and the verify endpoint per IP or session.
- **Rotate background images.** A fixed background lets an attacker precompute the slot positions.
- **Validate uploads** if the source image comes from users — check dimensions before decoding to avoid decompression bombs.

The answer is generated with `RandomNumberGenerator`, not `Random`, so the sequence is not predictable from earlier challenges.

## Licensing

This package is MIT.

It depends on [ImageSharp](https://github.com/SixLabors/ImageSharp), which uses the Six Labors Split License. Because you consume ImageSharp *transitively* through this package, it is licensed to you under Apache 2.0 regardless of your company's revenue — the split license's commercial threshold applies to direct package dependencies. If you also reference ImageSharp directly, check [sixlabors.com/pricing](https://sixlabors.com/pricing/) for your own situation.

## Contributing

Issues and pull requests welcome. Run `dotnet test` before opening a PR.
