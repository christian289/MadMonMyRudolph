using MadMonMyRudolph.Abstractions.Models;

namespace MadMonMyRudolph.Abstractions.Interfaces;

/// <summary>
/// 얼굴 인식 서비스 인터페이스
/// </summary>
public interface IFaceDetectionService
{
    /// <summary>
    /// 현재 사용 중인 인식 엔진
    /// </summary>
    FaceDetectionEngine CurrentEngine { get; set; }

    /// <summary>
    /// 이미지에서 얼굴을 검출합니다
    /// </summary>
    /// <param name="imageData">이미지 데이터 (BGR 형식)</param>
    /// <param name="width">이미지 너비</param>
    /// <param name="height">이미지 높이</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>얼굴 검출 결과</returns>
    Task<FaceDetectionResult> DetectFacesAsync(
        byte[] imageData,
        int width,
        int height,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 서비스 시작
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 서비스 중지
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}
