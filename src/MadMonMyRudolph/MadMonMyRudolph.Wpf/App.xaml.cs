using MadMonMyRudolph.Abstractions.Interfaces;
using MadMonMyRudolph.Core.Services;
using MadMonMyRudolph.Wpf.Abstractions;
using MadMonMyRudolph.Wpf.Core.Services;

namespace MadMonMyRudolph.Wpf;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = CreateHostBuilder().Build();

        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }

    private static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Information);
                logging.AddZLoggerConsole();
            })
            .ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<ICameraService, CameraService>();
                //services.AddSingleton<IFaceDetectionService, FaceDetectionService>();
                //services.AddSingleton<INavigationService, NavigationService>();
                //services.AddSingleton<IDialogService, DialogService>();
                //services.AddSingleton<IImageSaveService, ImageSaveService>();

                // Face Effects (명명된 서비스로 등록)
                //services.AddSingleton<IFaceEffectService, RudolphNoseEffectService>();

                // ViewModels
                services.AddSingleton<MainWindowViewModel>();
                //services.AddTransient<VideoModeViewModel>();
                //services.AddTransient<PhotoModeViewModel>();
                //services.AddSingleton<LogViewerViewModel>();

                // Views
                services.AddSingleton<MainWindow>();
                //services.AddTransient<VideoModeView>();
                //services.AddTransient<PhotoModeView>();
                //services.AddTransient<LogViewerView>();
            });
}
