namespace MadMonMyRudolph.Abstractions.Models;

/// <summary>
/// 얼굴 인식 결과를 나타내는 읽기 전용 구조체
/// </summary>
public readonly struct FaceDetectionResult
{
    public FaceDetectionResult(
        bool isDetected,
        FaceLandmarks? landmarks,
        float confidence,
        string detectionEngine)
    {
        IsDetected = isDetected;
        Landmarks = landmarks;
        Confidence = confidence;
        DetectionEngine = detectionEngine;
    }

    /// <summary>
    /// 얼굴이 감지되었는지 여부
    /// </summary>
    public bool IsDetected { get; }

    /// <summary>
    /// 얼굴 특징점 정보
    /// </summary>
    public FaceLandmarks? Landmarks { get; }

    /// <summary>
    /// 인식 신뢰도 (0.0 ~ 1.0)
    /// </summary>
    public float Confidence { get; }

    /// <summary>
    /// 사용된 인식 엔진 (dlib 또는 mediapipe)
    /// </summary>
    public string DetectionEngine { get; }
}
