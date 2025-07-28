using MadMonMyRudolph.Abstractions.Interfaces;

namespace MadMonMyRudolph.Core.Services;

/// <summary>
/// OpenCV를 사용한 카메라 서비스
/// </summary>
public sealed class CameraService(ILogger<CameraService> logger) : ICameraService, IDisposable
{
    private readonly Subject<Mat> _frameSubject = new();
    private VideoCapture? _capture;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _captureTask;
    private bool _isRunning;

    public IObservable<Mat> FrameStream => _frameSubject.AsObservable();

    public bool IsRunning => _isRunning;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            logger.LogInformation("Camera is already running");
            return;
        }

        try
        {
            logger.LogInformation("Starting camera service");

            _capture = new VideoCapture(0);
            if (!_capture.IsOpened())
            {
                throw new InvalidOperationException("Failed to open camera");
            }

            // 카메라 설정
            _capture.Set(VideoCaptureProperties.FrameWidth, 1280);
            _capture.Set(VideoCaptureProperties.FrameHeight, 720);
            _capture.Set(VideoCaptureProperties.Fps, 30);

            _cancellationTokenSource = new CancellationTokenSource();
            _isRunning = true;

            // 캡처 태스크 시작
            _captureTask = Task.Run(() => CaptureFramesAsync(_cancellationTokenSource.Token), cancellationToken);

            logger.LogInformation("Camera service started successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start camera service");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
        {
            return;
        }

        try
        {
            logger.LogInformation("Stopping camera service");

            _cancellationTokenSource?.Cancel();

            if (_captureTask != null)
            {
                await _captureTask.ConfigureAwait(false);
            }

            _capture?.Release();
            _capture?.Dispose();
            _capture = null;

            _isRunning = false;
            logger.LogInformation("Camera service stopped");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while stopping camera service");
        }
    }

    private async Task CaptureFramesAsync(CancellationToken cancellationToken)
    {
        var frame = new Mat();

        while (!cancellationToken.IsCancellationRequested && _capture != null && _isRunning)
        {
            try
            {
                if (_capture.Read(frame) && !frame.Empty())
                {
                    // 프레임을 Observable로 전달
                    _frameSubject.OnNext(frame.Clone());
                }

                // CPU 사용량을 줄이기 위한 지연
                await Task.Delay(33, cancellationToken).ConfigureAwait(false); // ~30 FPS
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error capturing frame");
            }
        }

        frame.Dispose();
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _capture?.Dispose();
        _frameSubject.Dispose();
    }
}
