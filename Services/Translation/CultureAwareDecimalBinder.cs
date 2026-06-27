using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace Sammlerplattform.Services.Translation
{
    public class CultureAwareDecimalBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            var rawValue = valueProviderResult.FirstValue;
            if (string.IsNullOrEmpty(rawValue))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            // Versuche mit der Benutzerkultur zu parsen
            if (decimal.TryParse(rawValue, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
                return Task.CompletedTask;
            }

            // Fallback: Versuche Invariant (Punkt als Trennzeichen)
            if (decimal.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName,
                $"Ungültiges Format. Erwartet wird ein Dezimalzahl im Format Ihrer Region (z.B. {FormatExample(CultureInfo.CurrentCulture)})");
            return Task.CompletedTask;
        }

        private static string FormatExample(CultureInfo culture)
        {
            var example = 1234.56m;
            return example.ToString(culture);
        }
    }
}
