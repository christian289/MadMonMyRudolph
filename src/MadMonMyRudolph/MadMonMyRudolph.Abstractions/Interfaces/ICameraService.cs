namespace MadMonMyRudolph.Abstractions.Interfaces;

/// <summary>
/// 카메라 서비스 인터페이스
/// </summary>
public interface ICameraService
{
    /// <summary>
    /// 프레임 스트림
    /// </summary>
    IObservable<Mat> FrameStream { get; }

    /// <summary>
    /// 카메라 시작
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 카메라 중지
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 카메라 실행 중 여부
    /// </summary>
    bool IsRunning { get; }
}