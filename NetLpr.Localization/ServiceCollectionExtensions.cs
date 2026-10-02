using NetLpr.Core.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace NetLpr.Localization

{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddLocalization(this IServiceCollection services, LocalizationConfig config)
        {
            services.AddSingleton(new JsonLocalizationStorage(config));
            services.AddSingleton<ILocalizationService, LocalizationService>();
            return services;
        }
    }
}
