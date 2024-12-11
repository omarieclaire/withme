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
        self.last_osc_send_time = 0  # Add this to track last send time
        self.osc_send_interval = 0.1  # 100ms interval

    
    def update_tracking(self, current_positions, frame_time):
        # Clean up old tracks
        self._remove_stale_tracks(frame_time)
        
        # Match new positions to existing tracks
        matched_positions = []
        new_tracked_people = {}

        # Update existing tracks
        for person_id, (last_pos, last_time) in self.tracked_people.items():
            best_match = None
            min_dist = float('inf')
            
            for current_pos in current_positions:
                if current_pos in matched_positions:
                    continue
                    
                dist = np.linalg.norm(np.array(current_pos) - np.array(last_pos))
                if dist < min_dist and dist < 100:  # Maximum distance threshold
                    min_dist = dist
                    best_match = current_pos

            if best_match is not None:
                new_tracked_people[person_id] = (best_match, frame_time)
                matched_positions.append(best_match)

        # Assign new IDs to unmatched positions
        for pos in current_positions:
            if pos not in matched_positions:
                if self.available_ids:
                    new_id = min(self.available_ids)
                    self.available_ids.remove(new_id)
                    new_tracked_people[new_id] = (pos, frame_time)

        self.tracked_people = new_tracked_people
        return self.tracked_people

    def _remove_stale_tracks(self, current_time):
        expired_ids = []
        for person_id, (_, last_time) in self.tracked_people.items():
            if current_time - last_time > self.grace_period:
                expired_ids.append(person_id)

        for expired_id in expired_ids:
            del self.tracked_people[expired_id]
            self.available_ids.append(expired_id)
        self.available_ids.sort()  # Keep IDs ordered

    def reset(self):
        self.tracked_people.clear()
        self.available_ids = list(range(20))

def detection_context():
    # Initialize MediaPipe
    mp_pose = mp.solutions.pose
    pose = mp_pose.Pose(
        min_detection_confidence=0.5,
        min_tracking_confidence=0.5,
        model_complexity=1
    )

    # Initialize camera
    camera_index = 0
    cap = cv2.VideoCapture(camera_index)
    if not cap.isOpened():
        print("Error: Could not open camera")
        return

    # Initialize variables
    person_tracker = PersonTracker(grace_period=1.0)
    show_debug = True
    invert_lr = False

    # Control information
    controls = [
        "Controls:",
        "q: Quit",
        "1-3: Switch cameras",
        "i: Invert left/right",
        "d: Toggle debug view",
        "+/-: Adjust detection threshold"
    ]

    while True:
        osc_process()
        ret, frame = cap.read()
        if not ret:
            print("Failed to grab frame")
            break

        # Apply inversion if enabled
        if invert_lr:
            frame = cv2.flip(frame, 1)

        # Process frame with MediaPipe
        frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        results = pose.process(frame_rgb)

        # Get current positions
        current_positions = []
        if results.pose_landmarks:
            landmarks = results.pose_landmarks.landmark
            mid_hip_x = int(landmarks[23].x * frame.shape[1])  # Using mid-hip point
            mid_hip_y = int(landmarks[23].y * frame.shape[0])
            current_positions.append((mid_hip_x, mid_hip_y))

        # Update tracking
        current_time = time.time()
        tracked_people = person_tracker.update_tracking(current_positions, current_time)



        # Send OSC messages
        for person_id, (position, _) in tracked_people.items():
            # Send X coordinate with decimal precision
            x_value = float(f"{position[0]:.5g}")  # Using g instead of f to avoid trailing zeros
            y_value = float(f"{position[1]:.5g}")
            
            osc_path_x = f"/livepose/blobs/0/{person_id}/center1"
            osc_msg_x = oscbuildparse.OSCMessage(osc_path_x, None, [x_value])
            
            # Send Y coordinate with decimal precision
            osc_path_y = f"/livepose/blobs/0/{person_id}/center2"
            osc_msg_y = oscbuildparse.OSCMessage(osc_path_y, None, [y_value])
            
            try:
                print(f"Attempting to send OSC: {osc_path_x}")
                osc_send(osc_msg_x, "localhost")
                print(f"Attempting to send OSC: {osc_path_y}")
                osc_send(osc_msg_y, "localhost")
                if show_debug:
                    print(f"Sent OSC: {osc_path_x} {x_value}")
                    print(f"Sent OSC: {osc_path_y} {y_value}")
            except Exception as e:
                print(f"OSC send error: {e}")


        # Debug visualization
        if show_debug:
            if results.pose_landmarks:
                mp_drawing = mp.solutions.drawing_utils
                mp_drawing.draw_landmarks(
                    frame,
                    results.pose_landmarks,
                    mp_pose.POSE_CONNECTIONS
                )

            # Draw tracked points and IDs
            for person_id, (position, _) in tracked_people.items():
                cv2.circle(frame, position, 10, (0, 255, 0), -1)
                cv2.putText(frame, f"ID: {person_id}", 
                           (position[0] - 20, position[1] - 20),
                           cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)

            # Draw controls
            for i, control in enumerate(controls):
                cv2.putText(frame, control, (10, 30 + i*30),
                           cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 255), 2)

        cv2.imshow('Tracking', frame)

        # Handle keyboard input
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
            invert_lr = not invert_lr
        elif key == ord('d'):
            show_debug = not show_debug

    # Cleanup
    cap.release()
    cv2.destroyAllWindows()
    pose.close()
    osc_terminate()

if __name__ == '__main__':
    try:
        osc_startup()
        osc_udp_client("127.0.0.1", 12001, "localhost")
        print("OSC initialized successfully")
        detection_context()  # Add this line to start the main program
    except Exception as e:
        print(f"OSC initialization error: {e}")