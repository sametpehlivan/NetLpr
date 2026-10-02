using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NetLpr.Desktop.ViewModels
{
    public partial class InitiatingWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _version = string.Empty;
        public InitiatingWindowViewModel()
        {
            Version = "2.0";
        }
    }
}
