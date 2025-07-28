웹캠 영상이나 이미지에서 얼굴을 인식하여 루돌프의 빨간 코를 합성해주는 WPF 애플리케이션입니다.
주요 기능

🎥 실시간 웹캠 효과: 웹캠 영상에서 실시간으로 얼굴을 추적하여 루돌프 코 효과 적용
📷 이미지 효과: 정적 이미지 파일에 루돌프 코 효과 적용 및 저장
🤖 듀얼 AI 엔진: MediaPipe와 Dlib 중 선택 가능한 얼굴 인식 엔진
📊 실시간 로그 뷰어: 애플리케이션 동작 상태를 모니터링할 수 있는 로그 뷰어
🎨 모던 UI: 다크 테마 기반의 현대적인 사용자 인터페이스

기술 스택
.NET/WPF

.NET 9.0
WPF (Windows Presentation Foundation)
MVVM 패턴 (CommunityToolkit.Mvvm)
Reactive Extensions (System.Reactive)
OpenCvSharp4
Microsoft.Extensions.Hosting
ZLogger

Python

MediaPipe (Google의 얼굴 인식 솔루션)
Dlib (얼굴 특징점 검출)
OpenCV
Named Pipe (프로세스 간 통신)

시스템 요구사항

Visual Studio 2022 17.0.31903.59 버전 이상
최소 Windows 10 (64-bit)
.NET 9.0 Runtime
Python 3.8 이상
웹캠 (실시간 모드 사용 시)