using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;



namespace NetLpr.Desktop.ViewModels
{
    public class ViewModelContainer
    {
        private static ViewModelContainer _instance = new ViewModelContainer();
        public static ViewModelContainer Container => _instance;
        private Dictionary<Type, ViewModelInfo> _sideBarViews = new();
         
        private ViewModelContainer() {
            InitateSidebarViews();
        }
        private void AddSidebar(ViewModelInfo info)
        {
            _sideBarViews.Add(
                info.ViewModelBase.GetType(),
                info
            );
        }
        public void InitateSidebarViews()
        {
            AddSidebar(
               new ViewModelInfo(new CamerasViewModel(), "desktop.sidebar.cameras", "cameraIcon")
            );
            
        }
        public bool GetViewModel<T>(out T? viewModel) where T : ViewModelBase
        {
            var res = _sideBarViews.TryGetValue(typeof(T),out var  viewModelInfo);
            viewModel = viewModelInfo == null ? null : (T)viewModelInfo.ViewModelBase;
            if (res == false) { 
                return false;
            }
            return true;
        }
        public ObservableCollection<ListItemTemplate> SideBarViewModelInfoToObservableCollection(Dictionary<string,bool> permissions)
        {

            return new ObservableCollection<ListItemTemplate>(_sideBarViews.Select(view => view.Value.ListItemTemplate));
        }

        public  ViewModelInfo? FindInstanceSideBarViewModelInfo(Type type)
        {
            if (_sideBarViews.TryGetValue(type, out var res))
            {
                return res;
            }
            return null;
        }
    }
}
