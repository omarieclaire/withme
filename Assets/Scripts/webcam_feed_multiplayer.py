#!/usr/bin/env python3

# /Users/marie/Documents/gam
# es/KYLECV/LuxonisSample/venv/bin/python /Users/marie/Documents/games/KYLECV/LuxonisSample
# /src/trackblobs/hello_world.py

# Getting Started Luxonis OAK

## Prerequisites

# Python build and setuptools are required. It's recommended to use a virtual environment.

# ```bash
# python3.10 -m venv venv
# source ./venv/bin/activate
# python -m pip install -U pip
# python -m pip install build setuptools
# ```

# (optional) Luxonis provides pre-built wheels of depthai which can be installed as follows:

# ```bash
# python -m pip install --extra-index-url https://artifacts.luxonis.com/artifactory/luxonis-python-snapshot-local/ depthai
#  ```

# ## Development

# Install dependencies with an editable package for development:

# ```bash
# python -m pip install --editable .
# ```

# ## Run luxonis depthai viewer

# ```bash
# python3 -m pip install depthai-viewer
# python3 -m depthai_viewer

# ```

# ## Build package

# Run build command:

# ```bash
# python -m build
# ```


import cv2
import depthai
import numpy as np
import blobconverter

from pythonosc import udp_client
import argparse

parser = argparse.ArgumentParser()
parser.add_argument("--ip", default="127.0.0.1", help="The ip to send OSC to")
parser.add_argument("--port", type=int, default=12001, help="The port to send OSC to")
args = parser.parse_args()

client = udp_client.SimpleUDPClient(args.ip, args.port)

pipeline = depthai.Pipeline()

# Color camera setup
cam_rgb = pipeline.createColorCamera()
cam_rgb.setPreviewSize(300, 300)
cam_rgb.setInterleaved(False)

detection_nn = pipeline.createMobileNetDetectionNetwork()
detection_nn.setBlobPath(blobconverter.from_zoo(name='mobilenet-ssd', shaves=6))
detection_nn.setConfidenceThreshold(0.5)

xout_rgb = pipeline.createXLinkOut()
xout_rgb.setStreamName("rgb")

xout_nn = pipeline.createXLinkOut()
xout_nn.setStreamName("nn")

cam_rgb.preview.link(xout_rgb.input)
cam_rgb.preview.link(detection_nn.input)
detection_nn.out.link(xout_nn.input)

def send_person_position(person_id, x, y):
    """Send OSC messages for person position in the specified format"""
    # Normalize coordinates to 0-1 range
    norm_x = x / 300  # Normalized by preview width
    norm_y = y / 300  # Normalized by preview height
    
    # Send X coordinate
    client.send_message(f"/livepose/blobs/0/{person_id}/center1", norm_x)
    
    # Send Y coordinate
    client.send_message(f"/livepose/blobs/0/{person_id}/center2", norm_y)

with depthai.Device(pipeline) as device:
    q_rgb = device.getOutputQueue("rgb")
    q_nn = device.getOutputQueue("nn")
    
    frame = None
    detections = []
    person_tracks = {}
    person_count = 0

    def frameNorm(frame, bbox):
        normVals = np.full(len(bbox), frame.shape[0])
        normVals[::2] = frame.shape[1]
        return (np.clip(np.array(bbox), 0, 1) * normVals).astype(int)

    while True:
        
        in_rgb = q_rgb.tryGet()
        in_nn = q_nn.tryGet()

        if in_rgb is not None:
            frame = in_rgb.getCvFrame()

        if in_nn is not None:
            detections = in_nn.detections

        if frame is not None:
            for detection in detections:
                if detection.label == 15:  # Person class
                    bbox = frameNorm(frame, (detection.xmin, detection.ymin, detection.xmax, detection.ymax))
                    center = (int(0.5 * (bbox[0] + bbox[2])), int(0.5 * (bbox[1] + bbox[3])))
                    
                    # Tracking logic
                    matched = False
                    for track_id, track_pos in person_tracks.items():
                        distance = np.sqrt((center[0] - track_pos[0])**2 + (center[1] - track_pos[1])**2)
                        if distance < 50:
                            matched = True
                            person_tracks[track_id] = center
                            current_id = track_id
                            # Send OSC update for tracked person
                            send_person_position(current_id, center[0], center[1])
                            break
                    
                    if not matched:
                        current_id = person_count
                        person_tracks[current_id] = center
                        # Send OSC update for new person
                        send_person_position(current_id, center[0], center[1])
                        person_count += 1
                    
                    # Visualization
                    cv2.rectangle(frame, (bbox[0], bbox[1]), (bbox[2], bbox[3]), (255, 0, 0), 2)
                    cv2.rectangle(frame, center, (center[0] + 5, center[1] + 5), (0, 150, 125), 2)
                    cv2.putText(frame, f"Person {current_id}", (bbox[0] + 10, bbox[1] + 20), 
                              cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 2)

            cv2.imshow("preview", frame)

        if cv2.waitKey(1) == ord('q'):
            break

    # Cleanup
    cv2.destroyAllWindows()