using MadMonMyRudolph.Abstractions.Interfaces;
using System.Drawing;

namespace MadMonMyRudolph.Wpf.Controls;

public partial class VideoModeViewModel : ObservableObject, IDisposable
{
    public VideoModeViewModel(
        ICameraService cameraService,
        IFaceDetectionService faceDetectionService,
        IFaceEffectService faceEffectService,
        ILogger<VideoModeViewModel> logger)
    {
        _cameraService = cameraService;
        _faceDetectionService = faceDetectionService;
        _faceEffectService = faceEffectService;
        _logger = logger;

        StatusText = " 준비됨";

        InitializeAsync();
    }

    private readonly ICameraService _cameraService;
    private readonly IFaceDetectionService _faceDetectionService;
    private readonly IFaceEffectService _faceEffectService;
    private readonly ILogger<VideoModeViewModel> _logger;
    private readonly CompositeDisposable _disposables = new();

    [ObservableProperty]
    private Bitmap? _currentFrame;

    [ObservableProperty]
    private bool _isRecording;

    [ObservableProperty]
    private string _statusText;

    private async void InitializeAsync()
    {
        try
        {
            await _faceDetectionService.StartAsync();
            await _cameraService.StartAsync();

            // 프레임 스트림 구독
            _cameraService.FrameStream
                .SelectMany(async frame => await ProcessFrameAsync(frame))
                .ObserveOnDispatcher()
                .Subscribe(processedFrame =>
                {
                    CurrentFrame = processedFrame;
                })
                .DisposeWith(_disposables);

            StatusText = "카메라 실행 중";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize video mode");
            StatusText = "초기화 실패";
        }
    }

    private async Task<Bitmap> ProcessFrameAsync(Mat frame)
    {
        try
        {
            // BGR 이미지 데이터 추출
            var imageData = new byte[frame.Width * frame.Height * frame.Channels()];
            frame.GetArray(out imageData);

            // 얼굴 인식
            var detectionResult = await _faceDetectionService.DetectFacesAsync(
                imageData,
                frame.Width,
                frame.Height);

            if (detectionResult.IsDetected)
            {
                // 효과 적용
                var processedData = _faceEffectService.ApplyEffect(
                    imageData,
                    frame.Width,
                    frame.Height,
                    detectionResult);

                // 처리된 데이터로 Mat 생성
                using var processedMat = new Mat(frame.Height, frame.Width, MatType.CV_8UC3, processedData);
                return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(processedMat);
            }

            return frame.ToBitmap();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing frame");
            return frame.ToBitmap();
        }
    }

    [RelayCommand]
    private async Task StartRecording()
    {
        if (IsRecording) return;

        IsRecording = true;
        StatusText = "녹화 중...";
        _logger.LogInformation("Started recording");
    }

    [RelayCommand]
    private async Task StopRecording()
    {
        if (!IsRecording) return;

        IsRecording = false;
        StatusText = "녹화 중지됨";
        _logger.LogInformation("Stopped recording");
    }

    public void Dispose()
    {
        _disposables.Dispose();
        _cameraService.StopAsync().Wait();
        _faceDetectionService.StopAsync().Wait();
    }
}

