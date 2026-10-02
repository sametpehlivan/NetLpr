using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NetLpr.Desktop.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {

        [ObservableProperty]
        private bool _isPaneOpen;
        [ObservableProperty]
        private ListItemTemplate _selectedItem;
        [ObservableProperty]
        private ViewModelBase _currentPage;
        [ObservableProperty]
        private int _languageIndex = -1;
        private List<string> _langs = new List<string>()
        {
             "tr-TR",
             "en-US",
             "az-AZ"
        };
        public ObservableCollection<ListItemTemplate> Items { get; set; }
        public MainWindowViewModel()
        {
            Initiate();
            LanguageIndex = 0;

        }
        private void OnLanguageChanged(int index)
        {
            if (index >= 0 && index < _langs.Count)
            {
                
                DesktopApplicationContext?.ChangeCultureInfo(_langs[index]);

            }
        }
        partial void OnSelectedItemChanged(ListItemTemplate value)
        {
            if (value is null) return;
            var Instance = ViewModelContainer.Container.FindInstanceSideBarViewModelInfo(value.ModelType);
            if (Instance is null) return;
            CurrentPage = ((ViewModelInfo)Instance).ViewModelBase;

            
        }
        [RelayCommand]
        private void TriggerPane()
        {
            IsPaneOpen = !IsPaneOpen;
        }
        [RelayCommand]
        private void ToggleTheme()
        {
            if (Application.Current?.RequestedThemeVariant == ThemeVariant.Dark)
            {
                Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                
            }
            else
            {
                if (Application.Current is not null)
                    Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
            }
        }
        public void Initiate()
        {

            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(LanguageIndex))
                {
                    OnLanguageChanged(LanguageIndex);
                }
            };
            Items = ViewModelContainer.Container.SideBarViewModelInfoToObservableCollection(new Dictionary<string, bool>());
            if (Items.Count > 0)
            {

                CurrentPage = ViewModelContainer.Container.FindInstanceSideBarViewModelInfo(Items.First().ModelType)!.ViewModelBase;
            }

        }
    }

}
