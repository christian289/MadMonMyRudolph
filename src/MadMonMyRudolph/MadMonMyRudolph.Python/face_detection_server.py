import sys
import json
import base64
import numpy as np
import cv2
import mediapipe as mp
import dlib
from typing import Dict, List, Optional, Tuple
import win32pipe
import win32file
import pywintypes
import logging
import traceback

# 로깅 설정
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)


class FaceDetectionServer:
    def __init__(self):
        self.pipe_name = r'\\.\pipe\rudolph_nose_pipe'
        self.pipe = None
        self.current_engine = "mediapipe"
        
        # MediaPipe 초기화
        self.mp_face_mesh = mp.solutions.face_mesh
        self.face_mesh = None
        
        # Dlib 초기화
        self.dlib_detector = dlib.get_frontal_face_detector()
        self.dlib_predictor = None
        
        # Dlib shape predictor 파일 경로 (68개 랜드마크)
        self.predictor_path = "shape_predictor_68_face_landmarks.dat"
        
    def init_mediapipe(self):
        """MediaPipe 초기화"""
        if self.face_mesh is None:
            self.face_mesh = self.mp_face_mesh.FaceMesh(
                static_image_mode=True,
                max_num_faces=1,
                refine_landmarks=True,
                min_detection_confidence=0.5,
                min_tracking_confidence=0.5
            )
            logger.info("MediaPipe initialized")
    
    def init_dlib(self):
        """Dlib 초기화"""
        if self.dlib_predictor is None:
            try:
                self.dlib_predictor = dlib.shape_predictor(self.predictor_path)
                logger.info("Dlib initialized")
            except Exception as e:
                logger.error(f"Failed to load Dlib predictor: {e}")
                logger.info("Dlib 사용을 위해서는 shape_predictor_68_face_landmarks.dat 파일이 필요합니다.")
                logger.info("다운로드: http://dlib.net/files/shape_predictor_68_face_landmarks.dat.bz2")
    
    def detect_faces_mediapipe(self, image: np.ndarray) -> Optional[Dict]:
        """MediaPipe로 얼굴 검출"""
        self.init_mediapipe()
        
        # BGR to RGB
        rgb_image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
        results = self.face_mesh.process(rgb_image)
        
        if results.multi_face_landmarks:
            face_landmarks = results.multi_face_landmarks[0]
            h, w = image.shape[:2]
            
            # 코 끝 좌표 (랜드마크 1번)
            nose_tip = face_landmarks.landmark[1]
            nose_x = int(nose_tip.x * w)
            nose_y = int(nose_tip.y * h)
            
            # 얼굴 윤곽 추출 (FACEMESH_FACE_OVAL)
            face_contour = []
            oval_indices = [10, 338, 297, 332, 284, 251, 389, 356, 454, 323, 361, 340, 346, 347, 348, 349, 350, 451, 452, 453, 464, 435, 410, 415, 308, 324, 318, 402, 317, 14, 87, 178, 88, 95, 78, 61, 84, 17, 314, 405, 303, 415, 308, 324, 318]
            
            for idx in oval_indices[:20]:  # 20개 포인트만 사용
                if idx < len(face_landmarks.landmark):
                    lm = face_landmarks.landmark[idx]
                    face_contour.append({
                        "x": lm.x * w,
                        "y": lm.y * h
                    })
            
            # 바운딩 박스 계산
            x_coords = [lm.x * w for lm in face_landmarks.landmark]
            y_coords = [lm.y * h for lm in face_landmarks.landmark]
            
            bbox_x = int(min(x_coords))
            bbox_y = int(min(y_coords))
            bbox_w = int(max(x_coords) - bbox_x)
            bbox_h = int(max(y_coords) - bbox_y)
            
            return {
                "nose_tip": {"x": nose_x, "y": nose_y},
                "bounding_box": {
                    "x": bbox_x,
                    "y": bbox_y,
                    "width": bbox_w,
                    "height": bbox_h
                },
                "face_contour": face_contour,
                "confidence": 0.95
            }
        
        return None
    
    def detect_faces_dlib(self, image: np.ndarray) -> Optional[Dict]:
        """Dlib으로 얼굴 검출"""
        self.init_dlib()
        
        if self.dlib_predictor is None:
            return None
        
        # 그레이스케일 변환
        gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
        faces = self.dlib_detector(gray)
        
        if len(faces) > 0:
            face = faces[0]
            shape = self.dlib_predictor(gray, face)
            
            # 코 끝 좌표 (랜드마크 30번)
            nose_tip = shape.part(30)
            
            # 얼굴 윤곽 추출 (턱선)
            face_contour = []
            for i in range(17):  # 0-16: 턱선
                pt = shape.part(i)
                face_contour.append({
                    "x": pt.x,
                    "y": pt.y
                })
            
            return {
                "nose_tip": {"x": nose_tip.x, "y": nose_tip.y},
                "bounding_box": {
                    "x": face.left(),
                    "y": face.top(),
                    "width": face.width(),
                    "height": face.height()
                },
                "face_contour": face_contour,
                "confidence": 0.9
            }
        
        return None
    
    def process_image(self, image_data: str, width: int, height: int, engine: str) -> Dict:
        """이미지 처리"""
        try:
            # Base64 디코딩
            image_bytes = base64.b64decode(image_data)
            nparr = np.frombuffer(image_bytes, np.uint8)
            image = nparr.reshape((height, width, 3))
            
            # 엔진에 따라 처리
            if engine == "mediapipe":
                result = self.detect_faces_mediapipe(image)
            else:
                result = self.detect_faces_dlib(image)
            
            if result:
                return {
                    "success": True,
                    "faces": [result]
                }
            else:
                return {
                    "success": True,
                    "faces": []
                }
                
        except Exception as e:
            logger.error(f"Image processing error: {e}")
            logger.error(traceback.format_exc())
            return {
                "success": False,
                "error": str(e)
            }
    
    def handle_command(self, command: Dict) -> Dict:
        """명령 처리"""
        cmd_type = command.get("command")
        
        if cmd_type == "detect":
            engine = command.get("engine", "mediapipe")
            image_data = command.get("image")
            width = command.get("width")
            height = command.get("height")
            
            return self.process_image(image_data, width, height, engine)
        
        elif cmd_type == "shutdown":
            logger.info("Shutdown command received")
            return {"success": True, "message": "Shutting down"}
        
        else:
            return {"success": False, "error": f"Unknown command: {cmd_type}"}
    
    def run(self):
        """서버 실행"""
        logger.info("Face detection server starting...")
        
        try:
            # Named Pipe 생성
            self.pipe = win32pipe.CreateNamedPipe(
                self.pipe_name,
                win32pipe.PIPE_ACCESS_DUPLEX,
                win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_READMODE_MESSAGE | win32pipe.PIPE_WAIT,
                1, 65536, 65536,
                0,
                None
            )
            
            logger.info(f"Named pipe created: {self.pipe_name}")
            logger.info("Waiting for client connection...")
            
            # 클라이언트 연결 대기
            win32pipe.ConnectNamedPipe(self.pipe, None)
            logger.info("Client connected")
            
            while True:
                try:
                    # 데이터 읽기
                    result, data = win32file.ReadFile(self.pipe, 65536)
                    
                    if result == 0:
                        # 데이터 파싱
                        message = data.decode('utf-8').strip()
                        if not message:
                            continue
                            
                        command = json.loads(message)
                        logger.debug(f"Received command: {command.get('command')}")
                        
                        # 명령 처리
                        response = self.handle_command(command)
                        
                        # shutdown 명령인 경우
                        if command.get("command") == "shutdown":
                            win32file.WriteFile(self.pipe, json.dumps(response).encode('utf-8'))
                            break
                        
                        # 응답 전송
                        response_data = json.dumps(response).encode('utf-8')
                        win32file.WriteFile(self.pipe, response_data)
                        
                except pywintypes.error as e:
                    if e.args[0] == 109:  # ERROR_BROKEN_PIPE
                        logger.info("Client disconnected")
                        break
                    else:
                        logger.error(f"Pipe error: {e}")
                        break
                except Exception as e:
                    logger.error(f"Error processing request: {e}")
                    logger.error(traceback.format_exc())
                    
        except Exception as e:
            logger.error(f"Server error: {e}")
            logger.error(traceback.format_exc())
        finally:
            if self.pipe:
                win32file.CloseHandle(self.pipe)
            logger.info("Server stopped")


if __name__ == "__main__":
    server = FaceDetectionServer()
    server.run()