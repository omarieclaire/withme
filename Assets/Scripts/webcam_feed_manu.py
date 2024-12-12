import cv2
import math

# Import needed modules from osc4py3
from osc4py3.as_eventloop import *
from osc4py3 import oscbuildparse


from dataclasses import dataclass
import numpy as np
import time

@dataclass
class Blobs:
    def __init__(self, id, center, size):
        self.id = id 
        self.center = center
        self.size = size

def smoothing_factor(t_e, cutoff):
    r = 2 * math.pi * cutoff * t_e
    return r / (r + 1)


def exponential_smoothing(a, x, x_prev):
    return a * x + (1 - a) * x_prev


class OneEuroFilter:
    def __init__(self, t0, x0, dx0=0.0, min_cutoff=1.0, beta=0.0,
                 d_cutoff=1.0):
        """Initialize the one euro filter."""
        # The parameters.
        self.min_cutoff = float(min_cutoff)
        self.beta = float(beta)
        self.d_cutoff = float(d_cutoff)
        # Previous values.
        self.x_prev = float(x0)
        self.dx_prev = float(dx0)
        self.t_prev = float(t0)

    def __call__(self, t, x):
        """Compute the filtered signal."""
        
        t_e = max(t - self.t_prev, 0.001) 
        #t_e = t - self.t_prev
        #sometimes the nearest neighbor finds two values that matches the same id

        # The filtered derivative of the signal.
        a_d = smoothing_factor(t_e, self.d_cutoff)
        dx = (x - self.x_prev) / t_e
        dx_hat = exponential_smoothing(a_d, dx, self.dx_prev)

        # The filtered signal.
        cutoff = self.min_cutoff + self.beta * abs(dx_hat)
        a = smoothing_factor(t_e, cutoff)
        x_hat = exponential_smoothing(a, x, self.x_prev)

        # Memorize the previous values.
        self.x_prev = x_hat
        self.dx_prev = dx_hat
        self.t_prev = t

        return x_hat


class NearestBlob: 
    def __init__(self):

        self._nearest_indices = {}

    def nearest(self, blobs_detected, filtered_blobs):

        nearest_indices = {}
        inverted_nearest_indices = {}
        min_dist_prev = {}


        if len(filtered_blobs) > 0:

            for index in filtered_blobs:
                min_dist_prev[index] = -1.0


            for blob_index, blob in blobs_detected.items():
                #remove_blob: int = -1
                center = blob.center
                index: int = -1
                nearest_index: int = -1
                min_dist_blob: float = -1.0

                for i in filtered_blobs:
                    index += 1
                    new_dist = pow(center[0] - filtered_blobs[i][0].x_prev, 2) + pow(center[1] - filtered_blobs[i][1].x_prev,2)

                    if new_dist < min_dist_blob or min_dist_blob < 0:
                        min_dist_blob = new_dist
                        nearest_index_dict = i
                        nearest_index= index
                          

                if nearest_index_dict > -1 and nearest_index in min_dist_prev:

                    if min_dist_blob < min_dist_prev[nearest_index] or min_dist_prev[nearest_index] < 0:
                        min_dist_prev[nearest_index] = min_dist_blob

                        #update filtered indices
                        nearest_indices[blob_index] = nearest_index_dict

                        #remove previously nearest index if there is one
                        if nearest_index_dict in inverted_nearest_indices.items():
                            nearest_indices[inverted_nearest_indices[nearest_index_dict]] = -1

                        # update inverted_nearest_indices
                        inverted_nearest_indices[nearest_index_dict] = blob_index

                    else: 
                        nearest_indices[blob_index] = -1
                       
                        # this means that the last occurence of the nearest index is the nearest one


            self._nearest_indices = nearest_indices

            print(len(blobs_detected))
            print(self._nearest_indices)
            print(min_dist_prev)

        return self._nearest_indices, min_dist_prev


class BlobTracking():

    def __init__(self):


        # Create a SimpleBlobDetector parameters object
        self._params = cv2.SimpleBlobDetector_Params()

        # Modify the parameters as needed
        self._params.minThreshold = 10
        self._params.maxThreshold = 60
        self._params.filterByArea = True
        self._params.minArea = 2
        self._params.maxArea = 50
        self._params.filterByColor = True
        self._params.blobColor = 255
        self._params.minDistBetweenBlobs = 20
        self._params.filterByCircularity = False
        self._params.filterByInertia = False
        self._params.filterByConvexity = False


        # Create a SimpleBlobDetector with the parameters
        self._detector = cv2.SimpleBlobDetector_create(self._params)

        self._cv2_keypoints = []
        self._nearestBlob = NearestBlob()

        # for automatic background subtraction
        self._num_frame = 0

        self._pose_lifetime = 1.0
        self._index_counter: Dict = {}

        self._available_indices = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20}

        self._blobs_detected = {}
        self._filtered_blobs = {}
        self._max_blob = 0


    def filtering_blobs(self, now):

        if self._num_frame > 1: 
            nearest_indices, min_dist_prev = self._nearestBlob.nearest( blobs_detected=self._blobs_detected, filtered_blobs=self._filtered_blobs)

            if self._num_frame == 2:

                for index, blob in self._blobs_detected.items():

                    self._max_blob += 1

                    if index not in self._filtered_blobs: ## this here is sadly a bad approximation. I'm not sure how to fix it.
                        self._filtered_blobs[index] = (
                        OneEuroFilter(now, blob.center[0]),
                        OneEuroFilter(now, blob.center[1])
                        )

                        self._available_indices.discard(index)

            else:

                for index, blob in self._blobs_detected.items():

                        if index not in self._filtered_blobs: ## this here is sadly a bad approximation. I'm not sure how to fix it.
                            self._filtered_blobs[index] = (
                            OneEuroFilter(now, blob.center[0]),
                            OneEuroFilter(now, blob.center[1])
                            )

                        else: 
                            if index in nearest_indices:
                                if nearest_indices[index] > -1:

                                    self._index_counter[index] = 0
                                    #if min_dist_prev[nearest_indices[index]] < 50: # to be determined if this actually helps or not
                                    self._filtered_blobs[nearest_indices[index]][0](now, blob.center[0])
                                    self._filtered_blobs[nearest_indices[index]][1](now, blob.center[1])

                                    self._available_indices.discard(nearest_indices[index])


                                else: 

                                    if index in self._index_counter:

                                        # self._index_counter[index] = 1 

                                        # if self._index_counter[index] > 2:

                                            if self._available_indices:

                                                new_id = next(iter(self._available_indices))
                                                self._filtered_blobs[new_id] = (
                                                OneEuroFilter(now, blob.center[0]),
                                                OneEuroFilter(now, blob.center[1])
                                                )

                                                self._available_indices.discard(new_id)

                                #             self._max_blob += 1
                                    else:
                                        self._index_counter[index] = 1 


            filtered_indices= []
            for index, filtered_position in self._filtered_blobs.items():
                if now - filtered_position[0].t_prev > self._pose_lifetime:
                    filtered_indices.append(index)

            for index in filtered_indices:
                self._filtered_blobs.pop(index)

                self._available_indices.add(index)


        if self._num_frame > 1: 

                # Fill blob data per camera
            blobs = {}
            for index, filtered_position in self._filtered_blobs.items():

                blob: Blobs = Blobs(
                    id = index,
                    center = [filtered_position[0].x_prev, filtered_position[1].x_prev],
                    size = 1.0
                )

                blobs[index] = blob


            self._blobs_detected = blobs

            print(self._available_indices)

        


    def detection_context(self, dev_id, waittime):
        # Open a video capture object
        cap = cv2.VideoCapture(dev_id)  # Replace 'your_video_file.avi' with your video file path

        if not cap.isOpened():
            print("Error: Could not open video file")
            exit()


        #initialize keypoints
        keypoints = ([[]])





        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break
            

            ## SOUSTRACTION DYNAMIQUE ##
            # if has_background_img:

            #     temp_background_img = frame
            #     frame = cv2.subtract(frame, background_img)
            #     background_img = temp_background_img

            # if not has_background_img:

            #     background_img = frame  
            #     has_background_img = True 

            ## SOUSTRACTION INITIALE ##
            if self._num_frame == 0:
                background_image = frame
            self._num_frame += 1

            frame = cv2.subtract(frame, background_image)
        
            keypoints = self._detector.detect(frame)

            for id, keypoint in enumerate(keypoints):

                blob = Blobs(id = id, center = keypoint.pt, size = keypoint.size)

                self._blobs_detected[id] = blob

            self.filtering_blobs(now = time.time())

            

            




            

            # Draw the keypoints on the frame
            frame_with_keypoints = cv2.drawKeypoints(frame, keypoints, outImage=None, color=(0, 0, 255),
                                                    flags=cv2.DRAW_MATCHES_FLAGS_DRAW_RICH_KEYPOINTS)

            for id, blob in self._blobs_detected.items():

                osc_process()
                osc_msg = oscbuildparse.OSCMessage("/blobs/0/" + str(blob.id) + "/center1", None, [blob.center[0]])
                osc_send(osc_msg, "localhost")

                osc_process()
                osc_msg = oscbuildparse.OSCMessage("/blobs/0/" + str(blob.id) + "/center2", None, [blob.center[1]])
                osc_send(osc_msg, "localhost")
                cv2.putText(
                    frame_with_keypoints,
                    str(blob.id),
                    (int(blob.center[0]), int(blob.center[1])),
                    cv2.FONT_HERSHEY_SIMPLEX,
                    1,
                    (255,255,255),
                    5,
                    cv2.LINE_AA
                )


            # Display the frame with keypoints
            cv2.imshow('frame', frame_with_keypoints)


            # Press 'q' to exit the loop
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break
            
            cv2.waitKey(waittime)

        # Release the video capture object and close the OpenCV windows
        cap.release()
        cv2.destroyAllWindows()
        osc_terminate()

if __name__ == '__main__':

    osc_startup()
    osc_udp_client("127.0.0.1", 12001, "localhost")
    blobtracking = BlobTracking()
    blobtracking.detection_context(dev_id = 0, waittime = 25)
