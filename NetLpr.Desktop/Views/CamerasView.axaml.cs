using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Linq;

namespace NetLpr.Desktop.Views
{
    public partial class CamerasView : UserControl
    {
        public CamerasView()
        {
            InitializeComponent();

        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
