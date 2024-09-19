using UnityEngine;
using extOSC;
using System.Collections.Generic;

public class SoundEventSender : MonoBehaviour
{
    // OSC Transmitter to send messages 
    public OSCTransmitter Transmitter;

    // Constant OSC address for sound play events
    private const string _soundAddress = "/sound/play";

    // Sphere size reference for scaling the sound positioning -- I might want to just get this programmatically
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
            Debug.LogError("Transmitter is not assigned in the SoundEventSender script.");
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
            Debug.Log(message);
        }
    }

    // Helper method to add OSC values (Azimuth, Elevation, Radius, etc.) into a message without sending the sourceIndex
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


    /* Example: Call SendSoundEvent to play a continuous sound with soundID "abletonTrack11"
    Vector3 soundPosition = new Vector3(1.0f, 2.0f, -3.0f);
    soundEventSender.SendSoundEvent("abletonTrack11", soundPosition);

    // Call: SendSoundEvent to play a one-shot sound with soundID "abletonTrack12"
    Vector3 soundPosition = new Vector3(0.0f, 1.5f, -2.0f);
    soundEventSender.SendSoundEvent("abletonTrack12", soundPosition);

    // No specific sound position available (optional, use null for no position)
    soundEventSender.SendSoundEvent("abletonTrack13", null);

    // Stop the continuous sound (make sure the soundID matches the one you started)
    soundEventSender.StopContinuousSound("abletonTrack11");
    */

    // Method to send sound events with a given sound ID and position to TouchDesigner or SpatGRIS
    public void SendSoundEvent(string soundID, Vector3? position)
    {
        // Create OSC message
        string oscAddress = "/sound/play";
        OSCMessage message = new OSCMessage(oscAddress);

        // If position is available, convert and add the spatial attributes to the message
        if (position.HasValue)
        {
            SoundPosition soundPos = new SoundPosition(position.Value, sphereSize);
            AddOSCValues(message, soundPos.Azimuth, soundPos.Elevation, soundPos.Radius, soundID);
        }
        else
        {
            LogMessage($"No position provided for sound {soundID}. Skipping spatial data.");
            AddOSCValues(message, 0, 0, 0, soundID);  // Default values for spatial data
        }

        // Add to active continuous sounds (only if not already added)
        if (!activeContinuousSounds.ContainsKey(soundID))
        {
            activeContinuousSounds.Add(soundID, message);  // Track continuous sounds
        }

        // Send the message using the Transmitter
        Transmitter.Send(message);
        LogMessage($"Sending OSC message to TouchDesigner: {oscAddress}, {soundID}");
    }

    // Method to stop a continuous sound using its sound ID
    public void StopContinuousSound(string soundID)
    {
        // Check if the sound ID exists in the active continuous sounds dictionary
        if (!activeContinuousSounds.ContainsKey(soundID))
        {
            Debug.LogWarning($"Trying to stop sound {soundID}, but it was not found in active continuous sounds.");
            return;
        }

        // Create a stop message for the sound
        var stopMessage = new OSCMessage("/sound/stop");
        stopMessage.AddValue(OSCValue.String(soundID));  // Use soundID to stop the correct sound

        // Send the stop message
        SafeSend(stopMessage);

        // Remove the sound from the active continuous sounds dictionary
        activeContinuousSounds.Remove(soundID);
        LogMessage($"Stopped and removed continuous sound {soundID}.");
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
            Debug.LogWarning($"Trying to update position for sound {soundID}, but it was not found in active continuous sounds.");
        }
    }

    // Method to safely send an OSC message and handle potential exceptions
    private void SafeSend(OSCMessage message)
    {
        try
        {
            // Attempt to send the message using the Transmitter
            Transmitter.Send(message);
            LogMessage($"Successfully sent OSC message: {SerializeOSCMessage(message)}");
        }
        catch (System.Exception ex)
        {
            // Log an error if the message fails to send
            Debug.LogError($"Failed to send OSC message: {ex.Message}");
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
