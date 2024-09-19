using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [Header("Camera Live Pose Feed Info")]
    [Tooltip("Resolution of the live pose camera.")]
    public int cameraResolution = 640;

    [Header("BIG INFO")]
    [Tooltip("Radius of the 'playsphere' where dots and players exist, used to visually scale the game.")]
    public float sphereSize = 10;
    [Tooltip("Maximum degrees for positioning calculations.")]
    public float maxDegrees = 210;

    [Tooltip("Factor for pushing positions towards the bottom of the dome.")]
    public float pushTowardsBottom = .5f;

    [Tooltip("Initial size for players.")]
    public float startSize;

    [Header("Positioning Info")]
    [Tooltip("Vertical positioning value; higher values keep objects closer to the top of the dome.")]
    public float verticalValue = 1;

    // note, Y becomes Z because the osc only has two values and we need 3
    [Tooltip("Remap values for camera input: maps 0 to the negative remap value and 640 to positive.")]
    public Vector2 RemapValues = new Vector2(1, -1);

    [Tooltip("Maximum size for position scaling along each axis.")]
    public Vector3 maxSize = new Vector3(1, 0, 1);


    [Header("Player Info")]
    [Tooltip("List of player GameObjects.")]
    public List<GameObject> players;

    [Tooltip("List of player avatars.")]
    public List<PlayerAvatar> playerAvatars;

    [Tooltip("List of player IDs.")]
    public List<int> playerIDS;

    [Tooltip("List of last seen timestamps for players.")]
    public List<float> playerLastSeenTimestamp;
    public float soundTimeout = 600f; // Time in seconds to stop sound after inactivity

    [Tooltip("List of sound states for players.")]
    public List<bool> playerSoundStates;

    [Tooltip("List of scaling factors for player visibility.")]
    public List<float> playerSeenScaler;


    [Tooltip("List of target positions for players.")]
    public List<Vector3> playerTargetPositions;

    [Tooltip("Prefab for player instances.")]
    public GameObject playerPrefab;

    [Tooltip("Parent transform for player instances.")]
    public Transform playerHolder;

    [Header("Movement Info")]
    [Tooltip("Speed for player movement lerping. 0 would be it doesn't move.")]
    public float playerLerpSpeed;

    [Tooltip("Time to wait until fading out player.")]
    public float Time2Wait4PlayerFadeOut = 0.3f;

    [Tooltip("Speed for player fade-in.")]
    public float playerFadeInSpeed;

    [Tooltip("Speed for player fade-out.")]
    public float playerFadeOutSpeed;

    [Tooltip("Average position of active players.")]
    public Vector3 averagePosition;

    [Tooltip("Number of active players.")]
    public int numActivePlayers;

    [Tooltip("List of active player GameObjects.")]
    public List<PlayerAvatar> activePlayers;

    // New settings for shrinking and reassigning players
    [Header("Stationary Player Logic")]
    [Tooltip("Minimum scale factor to shrink a player down to when stationary.")]
    public float minPlayerScale = 0.01f;

    [Tooltip("Time in seconds before we remove a stationary player entirely.")]
    public float removePlayerAfterStationaryTime = 300f;

    [Tooltip("Enable or disable player reassignment logic.")]
    public bool enablePlayerReassignment = true;


    // Dictionary to track stationary times
    private Dictionary<int, float> playerStationaryTimes = new Dictionary<int, float>();

    private void ShrinkPlayer(int playerIndex)
    {
        float timeStationary = playerStationaryTimes[playerIDS[playerIndex]];

        if (timeStationary >= removePlayerAfterStationaryTime)
        {
            float newScale = Mathf.Max(minPlayerScale, players[playerIndex].transform.localScale.x - playerFadeOutSpeed * Time.deltaTime);
            players[playerIndex].transform.localScale = new Vector3(newScale, newScale, newScale);

            // Hide the player when they're nearly invisible
            if (newScale <= minPlayerScale)
            {
                players[playerIndex].SetActive(false);
                // Debug.Log($"[CONFIRMED] Player {playerIDS[playerIndex]} has been hidden due to low scale.");
            }
        }
    }

    // private void RegrowPlayer(int playerIndex)
    // {
    //     float currentScale = players[playerIndex].transform.localScale.x;
    //     float targetScale = startSize * playerSeenScaler[playerIndex]; // Allow scale to regrow to startSize

    //     // Smoothly scale up towards the target scale using playerFadeInSpeed
    //     float newScale = Mathf.Min(targetScale, currentScale + playerFadeInSpeed * Time.deltaTime);

    //     players[playerIndex].transform.localScale = new Vector3(newScale, newScale, newScale);

    //     // Show the player again if scale is above the threshold
    //     if (!players[playerIndex].activeSelf && newScale > minPlayerScale)
    //     {
    //         players[playerIndex].SetActive(true);
    //         Debug.Log($"[CONFIRMED] Player {playerIDS[playerIndex]} has been shown again after regrowing.");
    //     }
    // }

    // private void CheckForPlayerReassignment(int playerIndex)
    // {
    //     if (!enablePlayerReassignment)
    //     {
    //         return; // Do nothing if reassignment is disabled
    //     }

    //     // Check nearby players when a player has stopped moving
    //     Vector3 playerPosition = playerTargetPositions[playerIndex];
    //     foreach (var otherPlayer in playerAvatars)
    //     {
    //         // Calculate distance to other players
    //         if (Vector3.Distance(playerPosition, otherPlayer.transform.position) < 1f && otherPlayer.id != playerIDS[playerIndex])
    //         {
    //             Debug.Log($"[CONFIRMED] Reassigning player {playerIDS[playerIndex]} to new player nearby.");
    //             playerIDS[playerIndex] = otherPlayer.id; // Assign to the nearby player
    //             break;
    //         }
    //     }
    // }


    [Header("Skybox Info")]
    [Header("Re Enable in play mode to update")]
    [Tooltip("Skybox stuff")]
    public Material skyboxMaterial;
    [Tooltip("Sun position for skybox. Y IS NEGATIVE SORRY")]
    public Vector3 SunPosition = new Vector3(0, -1, 0);
    public float SunHue = 0;
    public float sunIntensity = 1;
    public float sunHueSize = .1f;
    public float auroraIntensity = 1;
    public float auroraSpeed = 1;
    public float auroraHueStart = 0;
    public float auroraHueSize = 1;
    public float auroraHorizonImportance = .1f;
    public float auroraNoiseSize = 1;
    public float auroraVibrantness = 1;

    public virtual void _SetUp()
    {

        skyboxMaterial.SetVector("_LightDir", SunPosition.normalized);
        skyboxMaterial.SetFloat("_SunIntensity", sunIntensity);
        skyboxMaterial.SetFloat("_SunHue", SunHue);
        skyboxMaterial.SetFloat("_SunHueSize", sunHueSize);
        skyboxMaterial.SetFloat("_AuroraIntensity", auroraIntensity);
        skyboxMaterial.SetFloat("_AuroraSpeed", auroraSpeed);
        skyboxMaterial.SetFloat("_AuroraHueStart", auroraHueStart);
        skyboxMaterial.SetFloat("_AuroraHueSize", auroraHueSize);
        skyboxMaterial.SetFloat("_AuroraHorizonImportance", auroraHorizonImportance);
        skyboxMaterial.SetFloat("_AuroraNoiseSize", auroraNoiseSize);
        skyboxMaterial.SetFloat("_AuroraVibrantness", auroraVibrantness);

        players.Clear();
        playerAvatars.Clear();
        playerIDS.Clear();
        playerLastSeenTimestamp.Clear();
        playerSeenScaler.Clear();
        playerTargetPositions.Clear();
        playerSoundStates.Clear();

        // Now initialize the lists again
        players = new List<GameObject>();
        playerAvatars = new List<PlayerAvatar>();
        playerIDS = new List<int>();
        playerLastSeenTimestamp = new List<float>();
        playerSeenScaler = new List<float>();
        playerTargetPositions = new List<Vector3>();
        playerSoundStates = new List<bool>();
    }

    public virtual void SetUp()
    {
        _SetUp();
    }

    public Transform center;

    [Header("Sound Event Sender")]
    public SoundEventSender soundEventSender;
    public void OnEnable()
    {
        SetUp();
    }

    // Called when a new player joins the game, and it handles everything from 
    // creating the player object to setting up its initial state.
    public void OnPlayerCreate(int playerID)
    {
        // Debug.Log($"Creating player with ID: {playerID}");
        GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        player.transform.position = getFinalPosition(player.transform.position);
        player.transform.localScale = Vector3.one * startSize;
        player.transform.SetParent(playerHolder);
        player.name = $"Player_{playerID}_Sphere";

        PlayerAvatar playerAvatar = player.GetComponent<PlayerAvatar>();

        playerAvatar.controller = this;
        playerAvatar.id = playerID;
        playerAvatar.SetData("P" + playerID.ToString());

        player.SetActive(true);
        players.Add(player);
        playerAvatars.Add(playerAvatar);
        playerIDS.Add(playerID);
        playerLastSeenTimestamp.Add(Time.time);
        playerSeenScaler.Add(.001f);
        playerTargetPositions.Add(player.transform.position);
        playerAvatar.Reset();
        playerStationaryTimes[playerID] = 0f; // Initialize the stationary time when the player is created
        Vector3 initialPosition = getFinalPosition(Vector3.zero);


        string soundID = $"p{playerID}";

        // Vector3 fPos = new Vector3(0, 0, 0);
        // soundEventSender.SendSoundEvent("abletonTrack11", fPos, SoundType.Continuous, playerID);
        Vector3 soundPosition = new Vector3(1.0f, 2.0f, -3.0f);
        soundEventSender.SendSoundEvent(soundID, soundPosition);





    }

    // public void ReactivatePlayer(int playerID)
    // {
    //     Debug.Log($"[EXPECTED] Attempting to reactivate player with ID: {playerID}. Player should exist in the list.");

    //     int index = playerIDS.IndexOf(playerID);
    //     if (index != -1)
    //     {
    //         players[index].SetActive(true);

    //         if (players[index].activeSelf)
    //         {
    //             Debug.Log($"[CONFIRMED] Player with ID: {playerID} successfully reactivated.");
    //         }
    //         else
    //         {
    //             Debug.LogError($"[FAILED] Player with ID: {playerID} was not reactivated as expected.");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError($"[FAILED] Player with ID: {playerID} not found in the list.");
    //     }
    // }

    // public void DeactivatePlayer(int playerID)
    // {
    //     Debug.Log($"[EXPECTED] OOO Attempting to deactivate player with ID: {playerID}. Player should exist in the list.");

    //     int index = playerIDS.IndexOf(playerID);
    //     if (index != -1)
    //     {
    //         players[index].SetActive(false);

    //         if (!players[index].activeSelf)
    //         {
    //             Debug.Log($"[CONFIRMED] OOO Player with ID: {playerID} successfully deactivated.");
    //         }
    //         else
    //         {
    //             Debug.LogError($"[FAILED] OOO Player with ID: {playerID} was not deactivated as expected.");
    //         }

    //     }
    //     else
    //     {
    //         Debug.LogError($"[FAILED] Player with ID: {playerID} not found in the list.");
    //     }
    // }

    public void OnPlayerPositionUpdate(int playerID, Vector2 blobPosition)
    {

        // Debug.Log($"[EXPECTED] Received position update for player {playerID} with blob position: {blobPosition}. We expect to remap the position and apply it.");

        float v1 = blobPosition.x / cameraResolution;
        float v2 = blobPosition.y / cameraResolution;

        // Initialize the player's stationary time if it doesn't exist in the dictionary
        if (!playerStationaryTimes.ContainsKey(playerID))
        {
            playerStationaryTimes[playerID] = 0f;
            // Debug.Log($"[INITIALIZED] Player {playerID}'s stationary time initialized.");
        }

        // Reset stationary time when the player moves
        playerStationaryTimes[playerID] = 0f;

        // Remap the X and Y values from the camera to fit within the dome's space
        v1 = Mathf.Lerp(-RemapValues.x, RemapValues.x, v1);
        v2 = Mathf.Lerp(-RemapValues.y, RemapValues.y, v2);

        Vector3 remappedPosition = new Vector3(v1, 0, v2);
        Vector3 fPos = getFinalPosition(remappedPosition);
        // Debug.Log($"[CONFIRMED] Player {playerID}'s position has been remapped to: {fPos}.");

        int id = playerIDS.IndexOf(playerID);
        if (id != -1)
        {
            // Only change position if it's different from the current target
            if (playerTargetPositions[id] != fPos)
            {
                // Debug.Log($"[EXPECTED] Player {playerID}'s new position differs from the target. Updating position.");
                playerTargetPositions[id] = fPos;
                playerLastSeenTimestamp[id] = Time.time;

                // findme 
                // soundEventSender.SendSoundEvent("abletonTrack11", fPos, SoundType.Continuous, playerID);
                //    soundEventSender.SendSoundEvent("p0", remappedPosition);

                // Debug.Log($"[CONFIRMED] Player {playerID}'s target position updated to: {fPos}.");
            }
            else
            {
                // Debug.Log($"[EXPECTED] Player {playerID}'s position has not changed, no update required.");
            }

            // Smoothly move player to the new target position
            players[id].transform.position = Vector3.Lerp(players[id].transform.position, playerTargetPositions[id], playerLerpSpeed);

            // Debug.Log($"[CONFIRMED] Player {playerID} is moving towards the new target position: {playerTargetPositions[id]}.");
        }
        else
        {
            // Debug.LogError($"[FAILED] Player with ID: {playerID} was not found in the list.");
            OnPlayerCreate(playerID); // Recreate player if not found - UNTESTED

        }
    }

    // public void OnPlayerDestroy(int playerID)
    // {
    //     Debug.Log($"Destroying player with ID: {playerID}");

    //     int index = playerIDS.IndexOf(playerID);
    //     if (index != -1)
    //     {
    //         Destroy(players[index]);
    //         players.RemoveAt(index);
    //         playerIDS.RemoveAt(index);
    //     }
    // }


    public virtual void OnPlayersWithDotsCollided(PlayerAvatar p1, PlayerAvatar p2)
    {

    }

    public virtual void OnPlayerTrigger(PlayerAvatar player, GameObject collider)
    {
    }

    // Gets the scale of the player, different games will scale different ways
    public virtual Vector3 GetScale(int i)
    {
        // Debug.Log($"[DEBUG] GetScale for player {i}: startSize = {startSize}, playerSeenScaler = {playerSeenScaler[i]}");
        return Vector3.one * playerSeenScaler[i] * startSize;
    }

void Update()
{
    // Debug.Log("[EXPECTED] Update called. Resetting active players and calculating average position.");

    numActivePlayers = 0;
    averagePosition = Vector3.zero;
    activePlayers.Clear();

    for (int i = 0; i < players.Count; i++)
    {
        int playerID = playerIDS[i];
        float timeSinceLastSeen = Time.time - playerLastSeenTimestamp[i];

        // if (playerTargetPositions[i] == players[i].transform.position)
        // {
        //     // Player is stationary
        //     if (playerStationaryTimes.ContainsKey(playerID))
        //     {
        //         playerStationaryTimes[playerID] += Time.deltaTime;
        //     }
        //     else
        //     {
        //         playerStationaryTimes[playerID] = Time.deltaTime;  // Start tracking
        //     }
        // }
        // else
        // {
        //     // Reset stationary time when the player moves
        //     playerStationaryTimes[playerID] = 0;
        // }

        // Check if player is inactive for a specific period
        if (timeSinceLastSeen > soundTimeout)
        {
            // Stop sound if player has been inactive for longer than the soundTimeout
            string soundID = $"p{playerID}";
            soundEventSender.StopContinuousSound(soundID);
            Debug.Log($"[INFO] Player {playerID} has been inactive for {soundTimeout} seconds. Stopping sound.");
        }

        // Start fading only if the player hasn't been seen for a few frames
        if (timeSinceLastSeen > Time2Wait4PlayerFadeOut)
        {
            // Fade out
            playerSeenScaler[i] = Mathf.Lerp(playerSeenScaler[i], 0, playerFadeOutSpeed * Time.deltaTime);
            // Debug.Log($"[DEBUG] Fading out player {playerID}, playerSeenScaler = {playerSeenScaler[i]}");

            // Check if player is inactive for a specific period and stop the sound
            // if (timeSinceLastSeen > soundTimeout && playerSoundStates[i])
            {
                // Debug.Log($"[EXPECTED] Player {playerID} is inactive for more than {soundTimeout} seconds. Stopping sound.");
            }
        }
        else
        {
            // Fade in
            playerSeenScaler[i] = Mathf.Lerp(playerSeenScaler[i], 1, playerFadeInSpeed * Time.deltaTime);
            playerSeenScaler[i] = Mathf.Clamp(playerSeenScaler[i], 0, 1); // Ensures value stays between 0 and 1

            // Debug.Log($"[DEBUG] Fading in player {playerID}, playerSeenScaler = {playerSeenScaler[i]}");

            // if (!playerSoundStates[i])
            {
                // Debug.Log($"[EXPECTED] Player {playerID} has been seen again. Starting sound.");
                // playerSoundStates[i] = true; // Mark the sound as playing
            }
        }

        // Update player scale based on playerSeenScaler
        players[i].transform.localScale = GetScale(i);
        // Debug.Log($"[CONFIRMED] Player {playerID}'s scale updated to: {players[i].transform.localScale}");

        // Hide player if it has faded below threshold - the player is hidden using SetActive(false).
        if (playerSeenScaler[i] < .03f)
        {
            if (players[i].activeSelf)
            {
                players[i].SetActive(false);
                // Debug.Log($"[CONFIRMED] Player {playerID} has been hidden due to low visibility.");
            }
        }

        // Show the player if it's above the threshold and make them visible again using SetActive(true)
        else
        {
            // Only turn on if not already active
            if (!players[i].activeSelf)
            {
                players[i].SetActive(true);
                // Debug.Log($"[CONFIRMED] Player {playerID} has been shown as visibility increased.");
            }

            // Call RegrowPlayer to scale up the player
            // RegrowPlayer(i); // This was commented out
        }

        // Track active players and calculate average position
        if (players[i].activeSelf)
        {
            numActivePlayers++;
            activePlayers.Add(playerAvatars[i]);
            averagePosition += players[i].transform.position;
            // Debug.Log($"[CONFIRMED] Player {playerID} is active. Adding to active players list and updating average position.");
        }
    }

    // Calculate the average position of active players
    if (numActivePlayers > 0)
    {
        averagePosition /= numActivePlayers;
        // Debug.Log($"[CONFIRMED] Average position of {numActivePlayers} active players: {averagePosition}");
    }
    else
    {
        averagePosition = Vector3.zero;
        // Debug.Log("[CONFIRMED] No active players. Average position reset to zero.");
    }
}


    // Converts spherical coordinates to Cartesian coordinates
    public Vector3 SphericalToCartesian(float radius, float polar, float elevation)
    {
        // Debug.Log($"[EXPECTED] Converting spherical coordinates to Cartesian: Radius = {radius}, Polar = {polar}, Elevation = {elevation}.");
        float a = radius * Mathf.Cos(elevation);
        Vector3 outCart = new Vector3();
        outCart.x = a * Mathf.Cos(polar);
        outCart.y = radius * Mathf.Sin(elevation);
        outCart.z = a * Mathf.Sin(polar);
        // Debug.Log($"[CONFIRMED] Cartesian coordinates calculated: X = {outCart.x}, Y = {outCart.y}, Z = {outCart.z}.");
        return outCart;
    }


    // Calculates the final position of an object in the dome
    public Vector3 getFinalPosition(Vector3 position)
    {
        // Debug.Log($"[EXPECTED] Calculating final position inside the dome for input position: {position}");

        Vector3 finalPosition = position;

        float nX = finalPosition.x / maxSize.x;
        float nZ = finalPosition.z / maxSize.z;

        Vector2 nXZ = new Vector2(nX, nZ);

        /* 1. Calculate l(the distance from the center):
             l is a value that represents the player's distance from the center of the dome on 
             the flat (XZ) plane. Think of it like a straight line from the middle of the dome 
             to where the player is.
             "l" tells us how far the player is from the center, and it will be used 
             to figure out how "high" or "low" they should be.
         */
        float l = nXZ.magnitude;  // Get the distance from the center
        // Debug.Log($"[CONFIRMED] Normalized XZ values: nX = {nX}, nZ = {nZ}. Distance from center: l = {l}");

        /* 2. Apply the "push towards bottom" factor:
            l = Mathf.Pow(l, pushTowardsBottom); takes the value of l and modifies it using 
            the pushTowardsBottom factor. This step controls how much the player gets 
            "pulled" towards the bottom of the dome. The higher the value of 
            pushTowardsBottom, the more the player’s position gets skewed toward the bottom.
        */
        l = Mathf.Pow(l, pushTowardsBottom);  // Push towards the bottom based on a factor
        // Debug.Log($"[CONFIRMED] Adjusted distance using pushTowardsBottom factor: l = {l}");

        // angleAround is how far around the dome the player is. Imagine you’re looking down on the 
        // dome from above and drawing a line from the center to the player. 
        // This angle tells you the direction of that line.
        float angleAround = Mathf.Atan2(nZ, nX);  // Calculate angle for XZ plane

        // angleDown tells you how far "down" the player is from the top of the dome.
        // If the player is close to the top, angleDown is small. 
        // As the player moves toward the edge of the dome, angleDown increases.
        float angleDown = l * (maxDegrees / 360) * 2 * Mathf.PI;

        // Debug.Log($"[CONFIRMED] Calculated angles: angleAround = {angleAround}, angleDown = {angleDown}");

        // Convert from spherical coordinates to 3D Cartesian coordinates
        Vector3 fPosition = SphericalToCartesian(sphereSize, angleAround, Mathf.PI - angleDown);
        fPosition = transform.TransformPoint(fPosition);

        // Debug.Log($"[CONFIRMED] Final position calculated and transformed to world coordinates: {fPosition}");

        return fPosition;
    }
}