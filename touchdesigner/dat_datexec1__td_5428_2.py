import time

# Initialize dictionary to map soundID to sourceIndex
spat_source_map = {
   "music": 1,
   "BuddyBGMusic": 1, 
   "DriveBGMusic": 1, 
   "HerdBGMusic": 1, 
   "HugBGMusic": 1, 
   "KaliBGMusic": 1, 
   "MimicShapeBGMusic": 1, 
   "OrientationBGMusic": 1, 
   "OSCSoundTestBGMusic": 1, 
   "PaintBGMusic": 1, 
   "ShareBGMusic": 1, 
   "SoundTestBGMusic": 1, 
   "SoundTestBackupBGMusic": 1, 
   "SteerBGMusic": 1,
   "StickTogetherBGMusic": 1,
   "WithMeBGMusic": 1,
   # keeping #2 for stereo or jonsi in case I need it


   # sounds that don't move
   "MimicShapeBeat": 3,
   "StickTogetherBeat": 3,
   "StickTogetherPlayerInTarget": 3,
   "HerdPortalEntry": 3,
   "MimicShapeMatch": 3,
   "MimicShapeNewShapeGen": 3,
   "MimicShapeTimeout": 3,
   "WinGame": 3,
   "GameTimeout": 3,
   "MimicShapeBeat": 3,
   "MimicShapeBeat": 3,
   # keeping #4 for stereo


   #interactions 1
   "SteerMoveNorth": 5,
   "SteerMoveSouth": 5,
   "SteerMoveEast": 5,
   "SteerMoveWest": 5,
   "Herding": 5,


   #interactions 2
   "SteerCollision": 7,
   "SteerTeleport": 7,


   "sharepoints": 20,  # old thing here to not break things


   #all the things that play from the player location: player sound, player
	# Eats dot, player collides with another player in withme, player gets in constellation
	# Player flips a hugface, hug face sings (it’s been flipped), hug face sighs (it’s been
	# Matched, player herds things
   "p0": 21,
   "p0Point": 21,
   "p0Coll": 21,
   "p0InCons": 21,
   "p0hugfaceFlip": 21,
   "p0hugfaceSong": 21,
   "p0hugfaceSigh": 21,
   "p0Herd": 21,


   "p1": 22,
   "p1Point": 22,
   "p1Coll": 22,
   "p1InCons": 22,
   "p1hugfaceFlip": 22,
   "p1hugfaceSong": 22,
   "p1hugfaceSigh": 22,
   "p1Herd": 22,


   "p2": 23,
   "p2Point": 23,
   "p2Coll": 23,
   "p2InCons": 23,
   "p2hugfaceFlip": 23,
   "p2hugfaceSong": 23,
   "p2hugfaceSigh": 23,
   "p2Herd": 23,


   "p3": 24,
   "p3Point": 24,
   "p3Coll": 24,
   "p3InCons": 24,
   "p3hugfaceFlip": 24,
   "p3hugfaceSong": 24,
   "p3hugfaceSigh": 24,
   "p3Herd": 24,


   "p4": 25,
   "p4Point": 25,
   "p4Coll": 25,
   "p4InCons": 25,
   "p4hugfaceFlip": 25,
   "p4hugfaceSong": 25,
   "p4hugfaceSigh": 25,
   "p4Herd": 25,


   "p5": 26,
   "p5Point": 26,
   "p5Coll": 26,
   "p5InCons": 26,
   "p5hugfaceFlip": 26,
   "p5hugfaceSong": 26,
   "p5hugfaceSigh": 26,
   "p5Herd": 26,


   "p6": 27,
   "p6Point": 27,
   "p6Coll": 27,
   "p6InCons": 27,
   "p6hugfaceFlip": 27,
   "p6hugfaceSong": 27,
   "p6hugfaceSigh": 27,
   "p6Herd": 27,


   #end InConstellation music, but hugface continues
   "p7": 28,
   "p7Point": 28,
   "p7Coll": 28,
   # "p7InCons": 28,
   "p7hugfaceFlip": 28,
   "p7hugfaceSong": 28,
   "p7hugfaceSigh": 28,
   "p7Herd": 28,


   "p8": 29,
   "p8Point": 29,
   "p8Coll": 29,
   # "p8InCons": 29,
   "p8hugfaceFlip": 29,
   "p8hugfaceSong": 29,
   "p8hugfaceSigh": 29,
   "p8Herd": 29,


   "p9": 30,
   "p9Point": 30,
   "p9Coll": 30,
   # "p9InCons": 30,
   "p9hugfaceFlip": 30,
   "p9hugfaceSong": 30,
   "p9hugfaceSigh": 30,
   "p9Herd": 30,


   # reserve the next 10 too for future players


   # these "share" objects collide with points and spikes in share
   "share1": 41,
   "share2": 42,
   "share3": 43,
   "share4": 44,
   "share5": 45,
   "share6": 46,
   "share7": 47,
   "share8": 48,
   "share9": 49,
   "share10": 50,
   "share11": 51,
   "share12": 52,
   "share13": 53,
   "share14": 54,
   "share15": 55,
   "share16": 56,
   "share17": 57,
   "share18": 58,
   "share19": 59,
   "share20": 60

}

clip_mapping = {
    "OrientationBGMusic": 2,
    "WithMeBGMusic": 1,
    "OHMY": 1,  # Placeholder, as it doesn't match the names you provided earlier
    "HerdBGMusic": 3,
    "StickTogetherBGMusic": 4,
    "SteerBGMusic": 5,
    "DriveBGMusic": 6,
    "ShareBGMusic": 7,
    "PaintBGMusic": 8,
    "BuddyBGMusic": 9,
    "MimicShapeBGMusic": 10,
    "KaliBGMusic": 11,
    "HugBGMusic": 12
}

print("yooo")

# Set of active sound IDs for which updates are being sent
activeSoundIDs = set()

# Define sounds that should play multiple times even if already active
nonExclusiveSounds = {"music", "sharepoints", "BuddyBGMusic", "DriveBGMusic", "HerdBGMusic", "HugBGMusic", "KaliBGMusic", "MimicShapeBGMusic", "OrientationBGMusic", "OSCSoundTestBGMusic", "PaintBGMusic", "ShareBGMusic", "SoundTestBGMusic", "SoundTestBackupBGMusic", "SteerBGMusic", "StickTogetherBGMusic", "WithMeBGMusic"}

# Exclusive sounds that should not trigger play again if already playing
exclusiveSounds = {"p0", "p1", "p2", "p3", "p4", "p5"}  # Example exclusive sound list


# Categories for different types of sounds
backgroundMusicSounds = {
    "BuddyBGMusic", "DriveBGMusic", "HerdBGMusic", "HugBGMusic", "KaliBGMusic", 
    "MimicShapeBGMusic", "OrientationBGMusic", "OSCSoundTestBGMusic", "PaintBGMusic", 
    "ShareBGMusic", "SoundTestBGMusic", "SoundTestBackupBGMusic", "SteerBGMusic", 
    "StickTogetherBGMusic", "WithMeBGMusic"
}

playerSounds = {"p0", "p1", "p2", "p3", "p4", "p5"}  # Player-specific sounds
pointSounds = {"withmepoints"}  # Other specific sounds (e.g., for points)


def logMessage(logType, message):
    """ Helper function for better logging with timestamps. """
    timestamp = time.strftime("%Y-%m-%d %H:%M:%S", time.gmtime())
    if logType == "INFO":
        print(f"[{timestamp}][INFO]: {message}")
    elif logType == "ERROR":
        print(f"[{timestamp}][ERROR]: {message}")
    elif logType == "WARNING":
        print(f"[{timestamp}][WARNING]: {message}")
    elif logType == "SPATGRIS":
        print(f"[{timestamp}][SPATGRIS]: {message}")
    elif logType == "ABLETON":
        print(f"[{timestamp}][ABLETON]: {message}")

def stopAllPlayerSounds():
    for player in playerSounds:
        stopAbletonSound(player)
    logMessage("INFO", "Stopped all player sounds at game start.")


def onTableChange(dat):
    # stopAllPlayerSounds()
    print("yooo in table")
    for row in range(1, dat.numRows):

        osc_message = dat[row, 0]  # Get the message column (0)
        logMessage("INFO", f"MESSAGE {osc_message} incoming.")


        try:
            # Split the OSC message string
            split_message = osc_message.val.split(' ')
            sound_action = split_message[0]  # "/sound/play", "/sound/stop", or "/sound/update"
            sound_id = split_message[1].strip('"')  # Clean soundID by stripping extra quotes

            # Add explicit logging for "music"
            if sound_id == "music":
                logMessage("ABLETON", f"Received sound ID 'music', but no such clip exists. Skipping.")
                continue  # Skip this iteration since "music" is not a valid clip ID

            # Lookup source index for SpatGRIS from the dictionary using soundID
            source_index = spat_source_map.get(sound_id, None)

            if source_index is None:
                logMessage("WARNING", f"SoundID {sound_id} not found in mapping, skipping...")
                continue

            if sound_action == "/sound/play" or sound_action == "/sound/update":
                # Extract spatial parameters (azimuth, elevation, radius, etc.)
                azimuth = float(split_message[2])
                elevation = float(split_message[3])
                radius = float(split_message[4])
                horizontal_span = float(split_message[5])
                vertical_span = float(split_message[6])

                # Range check for radius (should be between -3.0 and 3.0)
                if not (-3.0 <= radius <= 3.0):
                    logMessage("WARNING", f"Radius {radius} is out of range [-3.0, 3.0] for soundID {sound_id}.")

                # Send spatialization data to SpatGRIS
                spat_osc_address = "/spat/serv"
                spat_osc_message = ["deg", source_index, azimuth, elevation, radius, horizontal_span, vertical_span]
                op('oscout1').sendOSC(spat_osc_address, spat_osc_message)
                # logMessage("SPATGRIS", f"Updated sound {sound_id} spatial data (Azimuth: {azimuth}, Elevation: {elevation}, Radius: {radius})")

                # Handle non-exclusive sounds (these can play multiple times even if already active)
                if sound_id in nonExclusiveSounds:
                    # logMessage("ABLETON", f"Triggering non-exclusive sound {sound_id} in Ableton.")
                    triggerAbletonSound(sound_id)

                # Handle exclusive sounds (check if they're already active in Ableton)
                elif sound_id in exclusiveSounds:
                    if sound_id not in activeSoundIDs:
                        # Trigger sound in Ableton only if it's not already playing
                        # logMessage("ABLETON", f"Triggering exclusive sound {sound_id} in Ableton.")
                        activeSoundIDs.add(sound_id)
                        triggerAbletonSound(sound_id)
                    # else:
                        # Do not trigger again, just update SpatGRIS
                        # logMessage("ABLETON", f"Exclusive sound {sound_id} already playing, no need to trigger again.")

            elif sound_action == "/sound/stop":
                # Handle stopping sound in Ableton
                if sound_id in activeSoundIDs:
                    activeSoundIDs.remove(sound_id)
                    # logMessage("ABLETON", f"Stopping exclusive sound {sound_id} in Ableton.")
                    stopAbletonSound(sound_id)
                else:
                    logMessage("ABLETON", f"Sound {sound_id} was not active, no need to stop.")

        except Exception as e:
            logMessage("ERROR", f"Error processing message: {e}")


def triggerBackgroundMusicSounds(sound_id):
    """ Handles background music sounds in the 'music' track """
    track_name = "music"  # Make sure this is the actual name of your Ableton track in the project
    clip_index = clip_mapping.get(sound_id, None)  # Fetch the clip index from the mapping

    if clip_index is not None:
        abletonTrack = op(track_name)  # Try to get the Ableton track operator
        if abletonTrack is None:
            logMessage("ERROR", f"Ableton Track {track_name} not found.")
            return

        if abletonTrack:
            try:
                # Trigger the clip to play using the clip index
                abletonTrack.FireClipSlot(clip_index)
                logMessage("ABLETON", f"Played Clip: {sound_id} in Track {track_name}, Clip Index {clip_index}")
            except Exception as e:
                logMessage("ERROR", f"Error playing clip: {e}")
        else:
            logMessage("ERROR", f"Ableton Track {track_name} is not valid.")
    else:
        logMessage("ERROR", f"Clip mapping for sound_id {sound_id} not found.")



def triggerPlayerSound(sound_id):
    """ Handles player-specific sounds like p0, p1, etc. """
    track_op_name = f"{sound_id}"  # Use track name like p0, p1, etc.
    abletonTrack = op(track_op_name)

    if abletonTrack:
        try:
            clip_index = 0  # Assume clip index 0 for players
            abletonTrack.FireClipSlot(clip_index)
            logMessage("ABLETON", f"Played Clip for Player: {sound_id}")
        except Exception as e:
            logMessage("ERROR", f"Error playing clip for Player: {e}")
    else:
        logMessage("ERROR", f"Ableton Track for Player {sound_id} not found.")

import random

def triggerPointSound(sound_id):
    """Handles point-related sounds by selecting from specified clips (0, 1, 2, 3, 4)."""
    track_name = "withmepoints"  # All point sounds are in the 'points' track
    clip_indices = [0, 1, 2, 3, 4]  # Specified clips
    abletonTrack = op(track_name)

    if abletonTrack:
        try:
            # Randomly select a clip index from the specified clips
            clip_index = random.choice(clip_indices)
            
            # Fire the selected clip
            abletonTrack.FireClipSlot(clip_index)
            
            # Log the clip information
            logMessage("ABLETON", f"Played Point Sound: {sound_id} in Track {track_name}, Clip Index {clip_index}")
        except Exception as e:
            logMessage("ERROR", f"Error playing point sound: {e}")
    else:
        logMessage("ERROR", f"Ableton Track for points not found.")


def triggerAbletonSound(sound_id):
    # Check if the sound_id is in the background music category
    if sound_id in backgroundMusicSounds:
        triggerBackgroundMusicSounds(sound_id)
    # Check if the sound_id belongs to a player (p0, p1, etc.)
    elif sound_id in playerSounds:
        triggerPlayerSound(sound_id)
    # Check if the sound_id is a point sound
    elif sound_id in pointSounds:
        triggerPointSound(sound_id)
    else:
        logMessage("INFO", f"Sound ID {sound_id} does not belong to any known category.")

def stopAbletonSound(sound_id):
    """ Stop the Ableton sound for a specific sound_id. """
    logMessage("ABLETON", f"Attempting to stop sound: {sound_id}")

    # Check if the sound is background music
    if sound_id in backgroundMusicSounds:
        track_name = "music"  # All background music clips are in the 'music' track
        clip_index = clip_mapping.get(sound_id, None)  # Get the clip index from the mapping

        if clip_index is not None:
            abletonTrack = op(track_name)

            if abletonTrack:
                try:
                    abletonTrack.StopClipSlot(clip_index)
                    logMessage("ABLETON", f"Stopped Clip: {sound_id} in Track {track_name}, Clip Index {clip_index}")
                except Exception as e:
                    logMessage("ERROR", f"Error stopping clip: {e}")
            else:
                logMessage("ERROR", f"Ableton Track for background music not found.")
        else:
            logMessage("ERROR", f"Clip mapping for sound_id {sound_id} not found.")

    # Handle player sounds
    elif sound_id in playerSounds:
        track_op_name = f"{sound_id}"  # Use track name like p0, p1, etc.
        abletonTrack = op(track_op_name)
        
        if abletonTrack:
            try:
                clip_index = 0  # Assuming all players have their sound in Clip 0
                abletonTrack.StopClipSlot(clip_index)
                logMessage("ABLETON", f"Stopped Clip for Player: {sound_id}")
                if sound_id in activeSoundIDs:
                    activeSoundIDs.remove(sound_id)  # Remove from active sounds
            except Exception as e:
                logMessage("ERROR", f"Error stopping clip for Player: {e}")
        else:
            logMessage("ERROR", f"Ableton Track for Player {sound_id} not found.")

    # Handle point sounds
    elif sound_id in pointSounds:
        track_name = "points"  # All point sounds are in the 'points' track
        clip_index = 0  # Assuming one point sound for now
        
        abletonTrack = op(track_name)
        
        if abletonTrack:
            try:
                abletonTrack.StopClipSlot(clip_index)
                logMessage("ABLETON", f"Stopped Point Sound: {sound_id} in Track {track_name}, Clip Index {clip_index}")
            except Exception as e:
                logMessage("ERROR", f"Error stopping point sound: {e}")
        else:
            logMessage("ERROR", f"Ableton Track for points not found.")
    
    # Fallback if no sound ID matches any category
    else:
        logMessage("INFO", f"Sound ID {sound_id} does not belong to any known category.")
