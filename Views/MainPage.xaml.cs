using ShepherdEplan.ViewModels;

namespace ShepherdEplan.Views
{
    public partial class MainPage : ContentPage
    {
        private const double NormalRowHeight = 50;
        private const double HoverRowHeight = 150; // 100% bigger (50 * 2.0)
        private const double NormalImageSize = 40;
        private const double HoverImageSize = 100; // 100% bigger (40 * 2.0)

        public MainPage(MaterialsViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        private void OnLoadClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] → Evento OnLoadClicked ejecutado");
        }

        // Synchronize horizontal scrolling between header and data
        private void OnDataScrolled(object sender, ScrolledEventArgs e)
        {
            HeaderScrollView.ScrollToAsync(e.ScrollX, 0, false);
        }

        // Mouse enters IMAGE - expand row
        private async void OnImagePointerEntered(object sender, PointerEventArgs e)
        {
            if (sender is Image image && image.Parent is Grid grid)
            {
                // Animate row expansion (100% bigger)
                await Task.WhenAll(
                    grid.FadeTo(0.95, 100), // Slight fade for visual feedback
                    Task.Run(() =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            grid.HeightRequest = HoverRowHeight;
                            image.WidthRequest = HoverImageSize;
                            image.HeightRequest = HoverImageSize;
                        });
                    })
                );

                await grid.FadeTo(1.0, 100);
            }
        }

        // Mouse leaves IMAGE - restore normal size
        private async void OnImagePointerExited(object sender, PointerEventArgs e)
        {
            if (sender is Image image && image.Parent is Grid grid)
            {
                // Animate row contraction
                await Task.WhenAll(
                    Task.Run(() =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            grid.HeightRequest = NormalRowHeight;
                            image.WidthRequest = NormalImageSize;
                            image.HeightRequest = NormalImageSize;
                        });
                    })
                );
            }
        }
    }
}