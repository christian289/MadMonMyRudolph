using MadMonMyRudolph.Abstractions;
using MadMonMyRudolph.Abstractions.Models;
using MadMonMyRudolph.Abstractions.Interfaces;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace MadMonMyRudolph.Core.Services;

/// <summary>
/// Python 서버와 Named Pipe를 통해 통신하는 얼굴 인식 서비스
/// </summary>
public sealed class FaceDetectionService(ILogger<FaceDetectionService> logger) : IFaceDetectionService, IDisposable
{
    private readonly Subject<FaceDetectionResult> _detectionSubject = new();
    private NamedPipeClientStream? _pipeClient;
    private StreamWriter? _writer;
    private StreamReader? _reader;
    private bool _isRunning;
    private FaceDetectionEngine _currentEngine = FaceDetectionEngine.MediaPipe;

    public FaceDetectionEngine CurrentEngine
    {
        get => _currentEngine;
        set => _currentEngine = value;
    }

    public async Task<FaceDetectionResult> DetectFacesAsync(
        byte[] imageData,
        int width,
        int height,
        CancellationToken cancellationToken = default)
    {
        if (!_isRunning || _writer == null || _reader == null)
        {
            logger.LogWarning("Face detection service is not running");
            return new FaceDetectionResult(false, null, 0f, _currentEngine.ToString());
        }

        try
        {
            // 요청 데이터 구성
            var request = new
            {
                command = "detect",
                engine = _currentEngine.ToString().ToLower(),
                image = Convert.ToBase64String(imageData),
                width = width,
                height = height
            };

            var json = JsonSerializer.Serialize(request);

            // Python 서버로 전송
            await _writer.WriteLineAsync(json).ConfigureAwait(false);
            await _writer.FlushAsync().ConfigureAwait(false);

            // 응답 수신
            var response = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrEmpty(response))
            {
                logger.LogWarning("Empty response from Python server");
                return new FaceDetectionResult(false, null, 0f, _currentEngine.ToString());
            }

            // 응답 파싱
            var result = JsonSerializer.Deserialize<FaceDetectionResponse>(response);
            if (result == null || !result.success)
            {
                logger.LogWarning("Face detection failed: {Error}", result?.error ?? "Unknown error");
                return new FaceDetectionResult(false, null, 0f, _currentEngine.ToString());
            }

            // 결과 변환
            if (result.faces != null && result.faces.Length > 0)
            {
                var face = result.faces[0]; // 첫 번째 얼굴만 사용
                var noseTip = new Point(face.nose_tip.x, face.nose_tip.y);
                var boundingBox = new Rectangle(
                    face.bounding_box.x,
                    face.bounding_box.y,
                    face.bounding_box.width,
                    face.bounding_box.height);

                var contour = face.face_contour
                    ?.Select(p => new Point(p.x, p.y))
                    .ToArray() ?? Array.Empty<Point>();

                var landmarks = new FaceLandmarks(noseTip, contour, boundingBox);
                return new FaceDetectionResult(true, landmarks, face.confidence, _currentEngine.ToString());
            }

            return new FaceDetectionResult(false, null, 0f, _currentEngine.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during face detection");
            return new FaceDetectionResult(false, null, 0f, _currentEngine.ToString());
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            logger.LogInformation("Face detection service is already running");
            return;
        }

        try
        {
            logger.LogInformation("Starting face detection service with engine: {Engine}", _currentEngine);

            // Python 서버 시작
            await StartPythonServerAsync(cancellationToken).ConfigureAwait(false);

            // Named Pipe 연결
            _pipeClient = new NamedPipeClientStream(".", "rudolph_nose_pipe", PipeDirection.InOut);
            await _pipeClient.ConnectAsync(5000, cancellationToken).ConfigureAwait(false);

            _writer = new StreamWriter(_pipeClient, Encoding.UTF8) { AutoFlush = false };
            _reader = new StreamReader(_pipeClient, Encoding.UTF8);

            _isRunning = true;
            logger.LogInformation("Face detection service started successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start face detection service");
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
            logger.LogInformation("Stopping face detection service");

            // Python 서버에 종료 명령 전송
            if (_writer != null)
            {
                var request = new { command = "shutdown" };
                await _writer.WriteLineAsync(JsonSerializer.Serialize(request)).ConfigureAwait(false);
                await _writer.FlushAsync().ConfigureAwait(false);
            }

            _writer?.Dispose();
            _reader?.Dispose();
            _pipeClient?.Dispose();

            _isRunning = false;
            logger.LogInformation("Face detection service stopped");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while stopping face detection service");
        }
    }

    private async Task StartPythonServerAsync(CancellationToken cancellationToken)
    {
        var pythonScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Python", "face_detection_server.py");

        if (!File.Exists(pythonScriptPath))
        {
            throw new FileNotFoundException($"Python script not found: {pythonScriptPath}");
        }

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "python",
            Arguments = pythonScriptPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        var process = System.Diagnostics.Process.Start(processStartInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start Python server");
        }

        // Python 서버가 시작될 때까지 대기
        await Task.Delay(2000, cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _detectionSubject.Dispose();
        _writer?.Dispose();
        _reader?.Dispose();
        _pipeClient?.Dispose();
    }

    // Response DTOs
    private class FaceDetectionResponse
    {
        public bool success { get; set; }
        public string? error { get; set; }
        public FaceData[]? faces { get; set; }
    }

    private class FaceData
    {
        public PointData nose_tip { get; set; } = new();
        public BoundingBoxData bounding_box { get; set; } = new();
        public PointData[]? face_contour { get; set; }
        public float confidence { get; set; }
    }

    private class PointData
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    private class BoundingBoxData
    {
        public int x { get; set; }
        public int y { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
}