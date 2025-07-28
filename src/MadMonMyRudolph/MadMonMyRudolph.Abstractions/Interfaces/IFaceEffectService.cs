using MadMonMyRudolph.Abstractions.Models;

namespace MadMonMyRudolph.Abstractions.Interfaces;

/// <summary>
/// 얼굴 효과 처리 서비스 인터페이스
/// </summary>
public interface IFaceEffectService
{
    /// <summary>
    /// 프레임에 효과를 적용합니다
    /// </summary>
    /// <param name="frameData">원본 프레임 데이터</param>
    /// <param name="width">프레임 너비</param>
    /// <param name="height">프레임 높이</param>
    /// <param name="faceDetectionResult">얼굴 인식 결과</param>
    /// <returns>효과가 적용된 프레임 데이터</returns>
    byte[] ApplyEffect(
        byte[] frameData,
        int width,
        int height,
        FaceDetectionResult faceDetectionResult);
}
