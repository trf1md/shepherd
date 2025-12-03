using Microsoft.Extensions.Logging;
using ShepherdEplan.Services.Eplan;
using ShepherdEplan.Services.Standard;
using ShepherdEplan.Services.SAP;
using ShepherdEplan.Services.Images;
using ShepherdEplan.Services.Merge;
using ShepherdEplan.ViewModels;
using ShepherdEplan.Views;

namespace ShepherdEplan
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Servicios base
            builder.Services.AddSingleton<HttpClient>();

            builder.Services.AddSingleton<GetEplanMatInfoService>();
            builder.Services.AddSingleton<GetStdMatInfoService>();
            builder.Services.AddSingleton<GetSapMaterialInfoService>();
            builder.Services.AddSingleton<GetImageService>();
            builder.Services.AddSingleton<DataMergeService>();

            // ViewModels
            builder.Services.AddSingleton<MaterialsViewModel>();

            // Views
            builder.Services.AddSingleton<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
