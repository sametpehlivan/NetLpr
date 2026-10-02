using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using NetLpr.Desktop.ViewModels;

namespace NetLpr.Desktop.Views;

public partial class InitiatingWindow : Window
{

    public InitiatingWindow()
    {
        InitializeComponent();
        DataContext = new InitiatingWindowViewModel();
    }
}