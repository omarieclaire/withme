// OSC Address: /spat/serv
// OSC Message: ["deg", 61, 45.0, 30.0, 1.0, 180.0, 90.0]

using UnityEngine;
using extOSC;
using System.Collections.Generic;

public class SoundEventSender : MonoBehaviour
{
    // OSC Transmitter to send messages 
    public OSCTransmitter Transmitter;

    // Constant OSC address for sound play events
    private const string _soundAddress = "/sound/play";

    // Sphere size reference for scaling the sound positioning
    public float sphereSize = 10f;

    // Debug flag to control whether logging is enabled or disabled
    public bool enableDebugLogging = true;

    // Dictionary to keep track of continuous sounds currently playing
    private Dictionary<string, OSCMessage> activeContinuousSounds = new Dictionary<string, OSCMessage>();

    void Start()
    {
        // Check if the Transmitter is assigned, log an error if it's missing
        if (Transmitter == null)
        {
            // Debug.LogError("Transmitter is not assigned in the SoundEventSender script.");
        }
    }

    // Struct to store sound spatial attributes for clean handling
    public struct SoundPosition
    {
        public float Azimuth;
        public float Elevation;
        public float Radius;

        // Constructor to convert a Unity Vector3 into SpatGRIS spherical coordinates
        public SoundPosition(Vector3 position, float sphereSize)
        {
            Radius = Mathf.Clamp(position.magnitude / sphereSize, -3.0f, 3.0f);
            Azimuth = Mathf.Atan2(position.x, position.z) * Mathf.Rad2Deg;
            Elevation = Mathf.Atan2(position.y, Radius) * Mathf.Rad2Deg;
            if (Mathf.Abs(Elevation) < 0.01f) Elevation = 0.0f;  // Avoid extremely small values
        }
    }

    // Helper method to send logs based on the debug flag
    private void LogMessage(string message)
    {
        if (enableDebugLogging)
        {
            // Debug.Log(message);
        }
    }

    // Helper method to add OSC values (Azimuth, Elevation, Radius, etc.) into a message
    private void AddOSCValues(OSCMessage message, float azimuth, float elevation, float radius, string soundID)
    {
        message.AddValue(OSCValue.String(soundID));       // The ID of the sound to be triggered
        message.AddValue(OSCValue.Float(azimuth));        // Azimuth: The horizontal angle for the sound.
        message.AddValue(OSCValue.Float(elevation));      // Elevation: The vertical position
        message.AddValue(OSCValue.Float(radius));         // Radius: The distance from the center (e.g., how far the sound source is from the listener)
        message.AddValue(OSCValue.Float(0.1f));           // Horizontal span (optional)
        message.AddValue(OSCValue.Float(0.1f));           // Vertical span (optional)
    }

    // Method to send one-shot sound events without tracking them in the dictionary
    public void SendOneShotSound(string soundID, Vector3? position)
{
    Debug.Log($"[SendOneShotSound] Entering method. SoundID: {soundID}, Position: {(position.HasValue ? position.Value.ToString() : "null")}");

    try
    {
        string oscAddress = "/sound/play";
        OSCMessage message = new OSCMessage(oscAddress);
        Debug.Log($"[SendOneShotSound] Created OSC message with address: {oscAddress}");

        if (position.HasValue)
        {
            SoundPosition soundPos = new SoundPosition(position.Value, sphereSize);
            Debug.Log($"[SendOneShotSound] Calculated sound position. Azimuth: {soundPos.Azimuth}, Elevation: {soundPos.Elevation}, Radius: {soundPos.Radius}");
            AddOSCValues(message, soundPos.Azimuth, soundPos.Elevation, soundPos.Radius, soundID);
        }
        else
        {
            Debug.Log($"[SendOneShotSound] No position provided for sound {soundID}. Using default spatial data.");
            AddOSCValues(message, 0, 0, 0, soundID);
        }

        Debug.Log($"[SendOneShotSound] Attempting to send OSC message...");
        SafeSend(message);
        Debug.Log($"[SendOneShotSound] Successfully sent OSC message to TouchDesigner: {oscAddress}, {soundID}");
    }
    catch (System.Exception e)
    {
        Debug.LogError($"[SendOneShotSound] Error occurred: {e.Message}\nStack Trace: {e.StackTrace}");
    }
}


    // Method to send or update continuous sound events
public void SendOrUpdateContinuousSound(string soundID, Vector3? position)
{
    if (position.HasValue)
    {
        // Check if the sound is already active
        if (IsSoundActive(soundID))
        {
            // Update the existing continuous sound's position
            UpdateContinuousSound(soundID, position.Value);
        }
        else
        {
            // Debug.Log("music? sound is NOT active so we'll send SendNewContinuousSound");
            // Debug.LogWarning($"music? sound is NOT active so we'll send SendNewContinuousSound for {soundID}.");


            // Send the initial continuous sound event (position may be null for background music)
            SendNewContinuousSound(soundID, position);
        }
    }
    else
    {
        // Debug.LogWarning($"Position for {soundID} not available.");
    }
}


    // Method to send a new continuous sound
    private void SendNewContinuousSound(string soundID, Vector3? position)
    {
        string oscAddress = _soundAddress;
        OSCMessage message = new OSCMessage(oscAddress);

        if (position.HasValue)
        {
            // Debug.Log("music? we're in SendNewContinuousSound and position has value");
            // For sounds with spatial data
            SoundPosition soundPos = new SoundPosition(position.Value, sphereSize);
            AddOSCValues(message, soundPos.Azimuth, soundPos.Elevation, soundPos.Radius, soundID);
        }
        else
        {
            // Debug.Log("music? we're in SendNewContinuousSound and position has not been provided");
            // If no position is provided, send a default or static value for non-spatial sounds
            AddOSCValues(message, 0, 0, 0, soundID);  // Default values
        }

        // Add to active continuous sounds if it's not already playing
        activeContinuousSounds.Add(soundID, message);

        // Send the message
        SafeSend(message);
        LogMessage($"Sent new continuous sound OSC message: {oscAddress}, {soundID}");
    }

    public bool IsSoundActive(string soundID)
    {
        return activeContinuousSounds.ContainsKey(soundID);
    }



    // Method to update the position of an already active continuous sound in SpatGRIS
    private void UpdateContinuousSound(string soundID, Vector3 position)
    {
        // Check if the sound exists in the dictionary
        if (activeContinuousSounds.TryGetValue(soundID, out OSCMessage message))
        {
            // Clear old values and update with the new position (SpatGRIS)
            SoundPosition soundPos = new SoundPosition(position, sphereSize);
            message.Values.Clear();  // Clear old values

            // Update spatial values in SpatGRIS without changing Ableton playback
            AddOSCValues(message, soundPos.Azimuth, soundPos.Elevation, soundPos.Radius, soundID);

            // Send the updated spatial information to SpatGRIS
            SafeSend(message);
            LogMessage($"Updated position for sound {soundID} in SpatGRIS.");
        }
        else
        {
            // Debug.LogWarning($"Trying to update position for sound {soundID}, but it was not found in active continuous sounds.");
        }
    }

    // Method to stop a continuous sound using its sound ID
    public void StopContinuousSound(string soundID, Vector3? position)
    {
        // Check if the sound ID exists in the active continuous sounds dictionary
        if (!activeContinuousSounds.ContainsKey(soundID))
        {
            // Debug.LogWarning($"background ? Trying to stop sound {soundID}, but it was not found in active continuous sounds.");
            return;
        }

        // Create a stop message for the sound
        var stopMessage = new OSCMessage("/sound/stop");
        stopMessage.AddValue(OSCValue.String(soundID));  // Use soundID to stop the correct sound

        // Send the stop message
        // SafeSend(stopMessage);

        // Remove the sound from the active continuous sounds dictionary
        activeContinuousSounds.Remove(soundID);
        LogMessage($"background ? Stopped and removed continuous sound {soundID}.");
    }

    // Method to safely send an OSC message and handle potential exceptions
    private void SafeSend(OSCMessage message)
    {
        try
        {
            // Attempt to send the message using the Transmitter
            Transmitter.Send(message);
            // LogMessage($"Successfully sent OSC message: {SerializeOSCMessage(message)}");
        }
        catch (System.Exception ex)
        {
            // Log an error if the message fails to send
            // Debug.LogError($"Failed to send OSC message: {ex.Message}");
        }
    }

    // Helper method to serialize an OSC message into a string format (useful for logging)
    private string SerializeOSCMessage(OSCMessage message)
    {
        if (message == null) return "Message is null";

        // Start the serialization with the OSC message address
        string result = message.Address;

        // Append each value as a raw value (no OSCValue type info)
        foreach (var value in message.Values)
        {
            if (value.Type == OSCValueType.Int)
                result += $" {value.IntValue}";
            else if (value.Type == OSCValueType.Float)
                result += $" {value.FloatValue}";
            else if (value.Type == OSCValueType.String)
                result += $" {value.StringValue}";
        }

        return result;
    }
}
