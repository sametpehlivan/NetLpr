
namespace NetLpr.Desktop.ViewModels
{
    public class ViewModelInfo
    {
        public ViewModelBase ViewModelBase { get; }
        public ListItemTemplate ListItemTemplate { get;}
        public ViewModelInfo(ViewModelBase viewModel,string? label = null, string? iconKey = null) {
            this.ViewModelBase = viewModel;
            this.ListItemTemplate = new ListItemTemplate(viewModel.GetType(), label, iconKey);
        }
    }

}
