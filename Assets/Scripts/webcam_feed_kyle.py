#!/usr/bin/env python3

import cv2
import depthai
import numpy as np
import blobconverter

pipeline = depthai.Pipeline()

# First, we want the Color camera as the output
cam_rgb = pipeline.createColorCamera()
cam_rgb.setPreviewSize(300, 300)  # 300x300 will be the preview frame size
cam_rgb.setInterleaved(False)

detection_nn = pipeline.createMobileNetDetectionNetwork()
# Use the same model as the working example
detection_nn.setBlobPath(blobconverter.from_zoo(name='mobilenet-ssd', shaves=6))
detection_nn.setConfidenceThreshold(0.5)

# XLinkOut is a "way out" from the device. Any data you want to transfer to host need to be send via XLink
xout_rgb = pipeline.createXLinkOut()
xout_rgb.setStreamName("rgb")

xout_nn = pipeline.createXLinkOut()
xout_nn.setStreamName("nn")

cam_rgb.preview.link(xout_rgb.input)
cam_rgb.preview.link(detection_nn.input)
detection_nn.out.link(xout_nn.input)

with depthai.Device(pipeline) as device:
    q_rgb = device.getOutputQueue("rgb")
    q_nn = device.getOutputQueue("nn")
    
    frame = None
    detections = []
    person_tracks = {}  # Dictionary to store person positions and IDs

    def frameNorm(frame, bbox):
        normVals = np.full(len(bbox), frame.shape[0])
        normVals[::2] = frame.shape[1]
        return (np.clip(np.array(bbox), 0, 1) * normVals).astype(int)

    person_count = 0

    while True:
        in_rgb = q_rgb.tryGet()
        in_nn = q_nn.tryGet()

        if in_rgb is not None:
            frame = in_rgb.getCvFrame()

        if in_nn is not None:
            detections = in_nn.detections

        if frame is not None:
            for detection in detections:
                if detection.label == 15:  # Person class in COCO dataset
                    bbox = frameNorm(frame, (detection.xmin, detection.ymin, detection.xmax, detection.ymax))
                    
                    # Calculate center point of the detection
                    center = (int(0.5 * (bbox[0] + bbox[2])), int(0.5 * (bbox[1] + bbox[3])))
                    
                    # Simple tracking: assign new ID or match to existing one
                    matched = False
                    for track_id, track_pos in person_tracks.items():
                        # If the center point is close to a tracked position, use that ID
                        distance = np.sqrt((center[0] - track_pos[0])**2 + (center[1] - track_pos[1])**2)
                        if distance < 50:  # Threshold for matching
                            matched = True
                            person_tracks[track_id] = center
                            current_id = track_id
                            break
                    
                    if not matched:
                        current_id = person_count
                        person_tracks[current_id] = center
                        person_count += 1
                    
                    # Draw detection box and ID
                    cv2.rectangle(frame, (bbox[0], bbox[1]), (bbox[2], bbox[3]), (255, 0, 0), 2)
                    cv2.rectangle(frame, center, (center[0] + 5, center[1] + 5), (0, 150, 125), 2)
                    cv2.putText(frame, f"Person {current_id}", (bbox[0] + 10, bbox[1] + 20), 
                              cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 2)

            cv2.imshow("preview", frame)

        if cv2.waitKey(1) == ord('q'):
            break

cv2.destroyAllWindows()