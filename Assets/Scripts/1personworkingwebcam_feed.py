import cv2
import mediapipe as mp
import numpy as np
import time
from collections import defaultdict
from osc4py3.as_eventloop import *
from osc4py3 import oscbuildparse

class PersonTracker:
    def __init__(self, grace_period=1.0):
        self.tracked_people = {}
        self.available_ids = list(range(10))
        self.grace_period = grace_period
        self.last_osc_send_time = 0
        self.osc_send_interval = 0.1

    def should_send_osc(self, current_time):
        if current_time - self.last_osc_send_time >= self.osc_send_interval:
            self.last_osc_send_time = current_time
            return True
        return False

    def remove_stale_tracks(self, current_time):  
        expired_ids = []
        for person_id, (_, last_time) in self.tracked_people.items():
            if current_time - last_time > self.grace_period:
                expired_ids.append(person_id)

        for expired_id in expired_ids:
            del self.tracked_people[expired_id]
            self.available_ids.append(expired_id)
        self.available_ids.sort()

    def update_tracking(self, current_positions, frame_time):
        self.remove_stale_tracks(frame_time) 
        
        matched_positions = []
        new_tracked_people = {}

        for person_id, (last_pos, last_time) in self.tracked_people.items():
            best_match = None
            min_dist = float('inf')
            
            for current_pos in current_positions:
                if current_pos in matched_positions:
                    continue
                    
                dist = np.linalg.norm(np.array(current_pos) - np.array(last_pos))
                if dist < min_dist and dist < 100:
                    min_dist = dist
                    best_match = current_pos

            if best_match is not None:
                new_tracked_people[person_id] = (best_match, frame_time)
                matched_positions.append(best_match)

        for pos in current_positions:
            if pos not in matched_positions:
                if self.available_ids:
                    new_id = min(self.available_ids)
                    self.available_ids.remove(new_id)
                    new_tracked_people[new_id] = (pos, frame_time)

        self.tracked_people = new_tracked_people
        return self.tracked_people

    def reset(self):
        self.tracked_people.clear()
        self.available_ids = list(range(10))

def draw_debug_overlay(frame, nose_x, nose_y, fps, options, person_id, latest_messages):
    h, w = frame.shape[:2]
    
    # Draw coordinate system visualization
    cv2.rectangle(frame, (10, 10), (30, h - 10), (0, 0, 0), -1)
    cv2.rectangle(frame, (40, 10), (60, h - 10), (0, 0, 0), -1)
    
    y_height = int((h - 20) * nose_y)
    x_height = int((h - 20) * nose_x)
    cv2.rectangle(frame, (10, h - 10 - y_height), (30, h - 10), (0, 255, 0), -1)
    cv2.rectangle(frame, (40, h - 10 - x_height), (60, h - 10), (0, 255, 0), -1)
    
    cv2.putText(frame, "Y", (15, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 255), 2)
    cv2.putText(frame, "X", (45, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 255), 2)

    nose_pixel_x = int(nose_x * w)
    nose_pixel_y = int(nose_y * h)
    cv2.circle(frame, (nose_pixel_x, nose_pixel_y), 30, (0, 0, 255), 2)
    if person_id is not None:
        cv2.putText(frame, f"ID: {person_id}", (nose_pixel_x - 20, nose_pixel_y + 5), 
                   cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 0, 255), 2)

    debug_text = [
        f"Raw X: {nose_x:.3f}",
        f"Raw Y: {nose_y:.3f}",
        f"Player ID: {person_id if person_id is not None else 'None'}",
        f"FPS: {fps:.1f}" if options['fps_limit'] else "FPS: Unlimited"
    ]
    
    y_offset = 50
    for i, text in enumerate(debug_text):
        cv2.putText(frame, text, (70, y_offset + i * 25), 
                   cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 255), 2)

    messages_y_offset = y_offset + (len(debug_text) + 1) * 25
    cv2.putText(frame, "Latest OSC Messages:", (70, messages_y_offset), 
               cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 255), 2)
    
    for i, msg in enumerate(latest_messages[-5:]):
        cv2.putText(frame, msg, (70, messages_y_offset + (i + 1) * 25), 
                   cv2.FONT_HERSHEY_SIMPLEX, 0.6, (200, 200, 200), 1)

    controls = [
        "Controls:",
        "Q: Quit",
        "1-3: Switch cameras",
        "I: Invert left/right",
        "D: Toggle debug view",
        "L: Toggle FPS limit",
        "R: Reset tracking",
    ]
    
    legend_height = len(controls) * 25 + 10
    overlay = frame.copy()
    cv2.rectangle(overlay, (w - 250, 10), (w - 10, legend_height), (0, 0, 0), -1)
    cv2.addWeighted(overlay, 0.7, frame, 0.3, 0, frame)
    
    for i, text in enumerate(controls):
        cv2.putText(frame, text, (w - 240, 30 + i * 25), 
                   cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 255), 2)

import cv2
import mediapipe as mp
import numpy as np
import time
from collections import defaultdict
from osc4py3.as_eventloop import *
from osc4py3 import oscbuildparse

# ... [Keep PersonTracker class and draw_debug_overlay function the same] ...

def detection_context():
    mp_pose = mp.solutions.pose
    # Simple pose detector with stable settings
    pose = mp_pose.Pose(
        min_detection_confidence=0.7,  # Increased confidence threshold
        min_tracking_confidence=0.7,   # Increased tracking confidence
        model_complexity=1,            # Medium complexity model
        smooth_landmarks=True          # Enable landmark smoothing
    )

    camera_index = 1
    cap = cv2.VideoCapture(camera_index)
    if not cap.isOpened():
        print("Error: Could not open camera")
        return

    person_tracker = PersonTracker(grace_period=0.5)  # Reduced grace period for quicker updates
    latest_messages = []
    options = {
        'show_debug': True,
        'invert_lr': True,
        'fps_limit': False
    }

    fps_target = 30.0
    frame_time = 1.0 / fps_target
    last_frame_time = time.time()
    fps_update_time = time.time()
    frame_count = 0
    current_fps = 0

    # Position smoothing
    prev_positions = []
    smoothing_window = 3

    while True:
        current_time = time.time()
        if options['fps_limit'] and (current_time - last_frame_time) < frame_time:
            continue

        frame_count += 1
        if current_time - fps_update_time >= 1.0:
            current_fps = frame_count
            frame_count = 0
            fps_update_time = current_time
        
        last_frame_time = current_time
        
        osc_process()
        ret, frame = cap.read()
        if not ret:
            print("Failed to grab frame")
            break

        if options['invert_lr']:
            frame = cv2.flip(frame, 1)

        frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        results = pose.process(frame_rgb)

        current_positions = []
        if results.pose_landmarks:
            landmarks = results.pose_landmarks.landmark
            nose_x = landmarks[0].x
            nose_y = landmarks[0].y
            
            # Basic position validation
            if 0 <= nose_x <= 1 and 0 <= nose_y <= 1:
                current_positions.append((nose_x, nose_y))

        # Apply smoothing to positions
        if current_positions:
            prev_positions.append(current_positions[0])
            if len(prev_positions) > smoothing_window:
                prev_positions.pop(0)
            
            # Calculate smoothed position
            if len(prev_positions) == smoothing_window:
                smoothed_x = sum(pos[0] for pos in prev_positions) / smoothing_window
                smoothed_y = sum(pos[1] for pos in prev_positions) / smoothing_window
                current_positions = [(smoothed_x, smoothed_y)]

        tracked_people = person_tracker.update_tracking(current_positions, current_time)

        if options['show_debug'] and tracked_people:
            debug_frame = frame.copy()
            for person_id, (position, _) in tracked_people.items():
                draw_debug_overlay(debug_frame, position[0], position[1], current_fps, 
                                 options, person_id, latest_messages)
            cv2.imshow('Tracking', debug_frame)
        elif options['show_debug']:
            cv2.imshow('Tracking', frame)  # Show frame even when no one is tracked

        if person_tracker.should_send_osc(current_time):
            for person_id, (position, _) in tracked_people.items():
                messages = [
                    (f"/livepose/blobs/0/{person_id}/center1", 1.0 - position[0]),
                    (f"/livepose/blobs/0/{person_id}/center2", 1.0 - position[1])
                ]

                for path, value in messages:
                    msg = oscbuildparse.OSCMessage(path, None, [value])
                    try:
                        osc_send(msg, "localhost")
                        print(f"Sent: {path}: {value}")  
                        latest_messages.append(f"{path}: {value:.3f}")
                    except Exception as e:
                        print(f"OSC send error: {e}")
                        latest_messages.append(f"Error sending: {path}")
            
            latest_messages = latest_messages[-10:]

        key = cv2.waitKey(1) & 0xFF
        if key == ord('q'):
            break
        elif key in [ord('1'), ord('2'), ord('3')]:
            new_index = int(chr(key)) - 1
            if new_index != camera_index:
                cap.release()
                camera_index = new_index
                cap = cv2.VideoCapture(camera_index)
                if not cap.isOpened():
                    print(f"Failed to open camera {camera_index}")
                    camera_index = 0
                    cap = cv2.VideoCapture(camera_index)
                person_tracker.reset()
        elif key == ord('i'):
            options['invert_lr'] = not options['invert_lr']
        elif key == ord('d'):
            options['show_debug'] = not options['show_debug']
        elif key == ord('l'):
            options['fps_limit'] = not options['fps_limit']
        elif key == ord('r'):
            person_tracker.reset()

    cap.release()
    cv2.destroyAllWindows()
    pose.close()
    osc_terminate()

if __name__ == '__main__':
    try:
        osc_startup()
        osc_udp_client("127.0.0.1", 12001, "localhost")
        print("OSC initialized successfully")
        detection_context()
    except Exception as e:
        print(f"OSC initialization error: {e}")