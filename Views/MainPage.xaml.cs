using ShepherdEplan.ViewModels;

namespace ShepherdEplan.Views
{
    public partial class MainPage : ContentPage
    {
        private const double NormalRowHeight = 50;
        private const double HoverRowHeight = 80;
        private const double NormalImageSize = 40;
        private const double HoverImageSize = 60;

        public MainPage(MaterialsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        // ═══════════════════════════════════════════════════════════════
        // UPDATED: Header scrolling synced with CollectionView
        // Previously: Synced with DataScrollView (ScrollView)
        // Now: Synced with CollectionView scrolling
        // ═══════════════════════════════════════════════════════════════
        private void OnCollectionViewScrolled(object? sender, ItemsViewScrolledEventArgs e)
        {
            // Sync horizontal scrolling between header and data
            // Note: CollectionView doesn't provide direct horizontal scroll position,
            // but we can use HorizontalOffset from the event args
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Approximate horizontal scroll sync
                    // This works for the virtualized CollectionView
                    await HeaderScrollView.ScrollToAsync(e.HorizontalOffset, 0, false);
                }
                catch
                {
                    // Ignore scroll sync errors
                }
            });
        }

        // ═══════════════════════════════════════════════════════════════
        // IMAGE HOVER EFFECTS (unchanged, works with virtualization)
        // ═══════════════════════════════════════════════════════════════
        private void OnImagePointerEntered(object? sender, PointerEventArgs e)
        {
            if (sender is not Image image)
                return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    // Find the parent Grid (RowGrid)
                    var grid = image.Parent as Grid;
                    if (grid != null)
                    {
                        grid.HeightRequest = HoverRowHeight;
                    }

                    // Enlarge the image
                    image.WidthRequest = HoverImageSize;
                    image.HeightRequest = HoverImageSize;

                    // Optional: Add subtle animation
                    image.FadeTo(0.8, 100);
                }
                catch
                {
                    // Ignore animation errors
                }
            });
        }

        private void OnImagePointerExited(object? sender, PointerEventArgs e)
        {
            if (sender is not Image image)
                return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    // Find the parent Grid (RowGrid)
                    var grid = image.Parent as Grid;
                    if (grid != null)
                    {
                        grid.HeightRequest = NormalRowHeight;
                    }

                    // Reset image size
                    image.WidthRequest = NormalImageSize;
                    image.HeightRequest = NormalImageSize;

                    // Reset opacity
                    image.FadeTo(1.0, 100);
                }
                catch
                {
                    // Ignore animation errors
                }
            });
        }
    }
}