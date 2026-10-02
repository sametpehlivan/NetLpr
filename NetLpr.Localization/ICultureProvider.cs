using System.Globalization;

namespace NetLpr.Localization
{
    public interface ICultureProvider
    {
        CultureInfo CurrentCulture { get; }
        void SetCulture(string cultureName);
    }
}
