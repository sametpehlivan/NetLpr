using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using NetLpr.Core.Extensions;
using NetLpr.Desktop.Extensions;
using NetLpr.Desktop.Models;

namespace NetLpr.Desktop.ViewModels
{
    public class ListItemTemplate : CultureListenerReactiveObject
    {
        private string _labelKey;
        public StreamGeometry? Icon { get; }
        public string Label => LocalizationService.GetLocalizeMessage(_labelKey);
        public Type ModelType { get; }
 

        internal ListItemTemplate(Type type, string? label = null, string? iconKey = null)
        {
            NameOfMultiLangProperties = new[] {
                nameof(Label)
            };
            ModelType = type;
            _labelKey = label ?? string.Empty;
            if (iconKey != null && Application.Current != null)
            {
                Application.Current.TryFindResource(iconKey, out var res);
                Icon = (StreamGeometry)res!;
            }


        }
    
    }
}
