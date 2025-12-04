using ShepherdEplan.ViewModels;

namespace ShepherdEplan.Views
{
    public partial class MainPage : ContentPage
    {
        private int counter = 0;

        public MainPage(MaterialsViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        private void OnLoadClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] → Evento OnLoadClicked ejecutado");
        }

    }
}