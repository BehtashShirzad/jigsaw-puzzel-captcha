using JigsawPuzzleCaptcha;
using JigsawPuzzleCaptcha.Contracts;
using JigsawPuzzleCaptcha.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registration helpers for the jigsaw puzzle CAPTCHA generator.
/// </summary>
public static class JigsawCaptchaServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IJigsawCaptchaGenerator"/> as a singleton.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configure">Optional callback for adjusting the default options.</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddJigsawCaptcha(
        this IServiceCollection services,
        Action<JigsawCaptchaOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new JigsawCaptchaOptions();
        configure?.Invoke(options);
        options.Validate();

        services.TryAddSingleton(options);
        services.TryAddSingleton<IJigsawCaptchaGenerator>(
            sp => new JigsawCaptchaGenerator(sp.GetRequiredService<JigsawCaptchaOptions>()));

        return services;
    }
}
