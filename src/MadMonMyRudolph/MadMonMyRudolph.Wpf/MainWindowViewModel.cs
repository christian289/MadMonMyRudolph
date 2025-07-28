using MadMonMyRudolph.Abstractions;
using MadMonMyRudolph.Abstractions.Interfaces;
using MadMonMyRudolph.Wpf.Abstractions;
using MadMonMyRudolph.Wpf.Controls;

namespace MadMonMyRudolph.Wpf;

internal partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel(
        INavigationService navigationService,
        IFaceDetectionService faceDetectionService)
    {
        _navigationService = navigationService;
        _faceDetectionService = faceDetectionService;
    }

    private readonly INavigationService _navigationService;
    private readonly IFaceDetectionService _faceDetectionService;

    [ObservableProperty]
    private bool _isDlibSelected = false;

    [ObservableProperty]
    private bool _isMediaPipeSelected = true;

    [ObservableProperty]
    private ObservableObject? _currentViewModel;

    [RelayCommand]
    private void NavigateToVideoMode()
    {
        _navigationService.NavigateTo<VideoModeViewModel>();
    }

    [RelayCommand]
    private void NavigateToPhotoMode()
    {
        //_navigationService.NavigateTo<PhotoModeViewModel>();
    }

    partial void OnIsDlibSelectedChanged(bool value)
    {
        if (value)
        {
            _faceDetectionService.CurrentEngine = FaceDetectionEngine.Dlib;
            IsMediaPipeSelected = false;
        }
    }

    partial void OnIsMediaPipeSelectedChanged(bool value)
    {
        if (value)
        {
            _faceDetectionService.CurrentEngine = FaceDetectionEngine.MediaPipe;
            IsDlibSelected = false;
        }
    }
}