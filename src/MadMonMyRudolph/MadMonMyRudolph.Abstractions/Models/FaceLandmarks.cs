using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace MadMonMyRudolph.Abstractions.Models;

/// <summary>
/// 얼굴 특징점 정보를 나타내는 읽기 전용 구조체
/// </summary>
public readonly struct FaceLandmarks(Point noseTip, Point[] faceContour, Rectangle boundingBox)
{

    /// <summary>
    /// 코 끝 좌표
    /// </summary>
    public Point NoseTip { get; } = noseTip;

    /// <summary>
    /// 얼굴 윤곽 점들
    /// </summary>
    public Point[] FaceContour { get; } = faceContour;

    /// <summary>
    /// 얼굴 경계 상자
    /// </summary>
    public Rectangle BoundingBox { get; } = boundingBox;
}