using ShepherdEplan.ViewModels;

namespace ShepherdEplan.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MaterialsViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
