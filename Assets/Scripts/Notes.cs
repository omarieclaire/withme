// Fulldome camera notes: https://github.com/rsodre/FulldomeCameraForUnity?tab=readme-ov-file


// OSC message examples
// e9f60290.e0e11dbb /livepose/blobs/0/4/center ff 253.067078 410.732697
// e9f60290.e0e26d47 /livepose/blobs/0/1/start_pose ff 9.922527 0.021084
// e9f60290.e0e35828 /livepose/blobs/0/6/start_pose ff 5.049118 0.019473
// e9f60290.e0e4755f /livepose/blobs/0/2/blob_id i 2
// e9f60290.e0e52deb /livepose/blobs/0/0/size f 36.285938
// e9f60290.e0e67d77 /livepose/blobs/0/1/size f 41.297497
// e9f60290.e0e746ca /livepose/blobs/0/3/start_pose ff 9.922585 0.021064
// e9f60290.e0e8b7e3 /livepose/blobs/0/1/end_pose ff 9.922559 0.021083
// e9f60290.e0e991fe /livepose/blobs/0/4/blob_id i 4
// e9f60290.e0ea4a8b /livepose/blobs/0/5/start_pose ff 6.202342 0.024471
// e9f60290.e0eb7888 /livepose/blobs/0/6/end_pose ff 5.049118 0.019473
// e9f60290.e0ec84f8 /livepose/blobs/0/6/center ff 611.052124 678.033813
// e9f60290.e0ed9167 /livepose/blobs/0/3/size f 44.825001
// e9f60290.e0ee5abb /livepose/end_frame ff 9.922914 0.019697
// e9f60290.e0ef672a /livepose/blobs/0/4/start_pose ff 9.156663 0.019922
// e9f60290.e0f04145 /livepose/blobs/0/5/end_pose ff 6.202348 0.024471
// e9f60290.e0f13ced /livepose/frame ff 9.923358 0.017343
// e9f60290.e0f21708 /livepose/blobs/0/0/start_pose ff 9.922510 0.021084
// e9f60290.e0f2f122 /livepose/blobs/0/6/size f 38.368988
// e9f60290.e0f40e59 /livepose/blobs/0/4/size f 43.618443
// e9f60290.e0f4d7ac /livepose/blobs/0/5/center ff 254.903549 412.116241
// e9f60290.e0f5d355 /livepose/blobs/0/0/end_pose ff 9.922522 0.021084
// e9f60290.e0f69ca9 /livepose/blobs/0/2/size f 26.210253
// e9f60290.e0f75535 /livepose/blobs/0/1/center ff 1033.551636 1176.246460
// e9f60290.e0f82f50 /livepose/blobs/0/1/blob_id i 1
// e9f60290.e0f8d715 /livepose/blobs/0/3/center ff 747.215698 1007.438843
// e9f60290.e3b80a9d /livepose/blobs/0/2/end_pose ff 9.934248 0.016985
// e9f60290.e3ba12b5 /livepose/blobs/0/2/center ff 1142.763794 443.286377
// e9f60290.e3bb0e5d /livepose/blobs/0/2/start_pose ff 9.934231 0.016985
// e9f60290.e3bbf93f /livepose/blobs/0/6/blob_id i 6
// e9f60290.e3bc903d /livepose/blobs/0/3/end_pose ff 9.934278 0.016985
// e9f60290.e3bd7b1f /livepose/blobs/0/4/end_pose ff 9.162563 0.019911
// e9f60290.e3be5539 /livepose/blobs/0/5/size f 44.861584
// e9f60290.e3befcff /livepose/blobs/0/0/blob_id i 0
// e9f60290.e3bfd71a /livepose/blobs/0/3/blob_id i 3
// e9f60290.e3c06e18 /livepose/blobs/0/5/blob_id i 5
// e9f60290.e3c115de /livepose/blobs/0/0/center ff 1097.406494 675.808167
// e9f60290.e3c1df32 /livepose/blobs/0/4/center ff 253.070511 410.735413
// e9f60290.e3c27630 /livepose/blobs/0/1/start_pose ff 9.934200 0.016985
// e9f60290.e3c30d2f /livepose/blobs/0/6/start_pose ff 5.049118 0.019472
// e9f60290.e3c3b4f5 /livepose/blobs/0/2/blob_id i 2
// e9f60290.e3c45cba /livepose/blobs/0/0/size f 35.896210
// e9f60290.e3c621b6 /livepose/blobs/0/1/size f 41.459831
// e9f60290.e3c70c98 /livepose/blobs/0/3/start_pose ff 9.934258 0.016985
// e9f60290.e3c7d5ec /livepose/blobs/0/1/end_pose ff 9.934232 0.016985
// e9f60290.e3c87db1 /livepose/blobs/0/4/blob_id i 4




// Notes on OSC strings https://gitlab.com/sat-mtl/tools/livepose/research-and-development/blob-tracking-in-livepose 

// Namespace:
// The OSC messages related to blobs all start with "/livepose/blobs/0". The 0 here means that the blobs are tracked on camera with ID 0.

// Then, each blob detected on the frame is assigned an ID, starting from 0. For example, the center location (in pixel unit) of the blob identified as ID #2 is given by the following message:

// /livepose/blobs/0/2/center ff 1114.656860 1042.972778

// Two data items are then associated to each detected blob ID: "center", and "size".

// center
// this osc message contains the center location of the detected blob in pixel units (2 floats)

// size
// this osc message contains the diameter of the blob detected in pixel units (1 float).

