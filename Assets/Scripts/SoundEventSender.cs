/* 

TODO:

1. Ensure Proper Mapping of Azimuth, Elevation, and Radius:
I've normalized the radius to fit SpatGRIS’s expected range of -3.0 to 3.0. 
I need to ensure  that the mapping between Unity world position and SpatGRIS’s spherical coordinates (Azimuth, Elevation, Radius) matches what SpatGRIS expects.

2. Correct Source Indexing!
*/

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

    // Helper method to add OSC values (Azimuth, Elevation, Radius, etc.) into a message
    private void AddOSCValues(OSCMessage message, int sourceIndex, float azimuth, float elevation, float radius, string soundID)
    {
        message.AddValue(OSCValue.Int(sourceIndex + 1));  // Source index (spatGRIS requires 1-based indexing)
        message.AddValue(OSCValue.Float(azimuth));        // Azimuth: The horizontal angle for the sound.
        message.AddValue(OSCValue.Float(elevation));      // Elevation: The vertical position
        message.AddValue(OSCValue.Float(radius));         // Radius:The distance from the center (e.g., how far the sound source is from the listener)
        message.AddValue(OSCValue.Float(0.1f));           // Horizontal span (optional)
        message.AddValue(OSCValue.Float(0.1f));           // Vertical span (optional)
        message.AddValue(OSCValue.String(soundID));       // The ID of the sound to be triggered
    }

    // Method to send sound events with a given sound ID and position to TouchDesigner or SpatGRIS
    public void SendSoundEvent(string soundID, Vector3 position, SoundType soundType, int sourceIndex)
    {
        // Create OSC message
        string oscAddress = _soundAddress;
        OSCMessage message = new OSCMessage(oscAddress);

        // Convert Unity's position to SpatGRIS spherical coordinates using the SoundPosition struct
        SoundPosition soundPos = new SoundPosition(position, sphereSize);

        // Add the sound's spatial attributes to the OSC message
        AddOSCValues(message, sourceIndex, soundPos.Azimuth, soundPos.Elevation, soundPos.Radius, soundID);

        // Send the message using the Transmitter
        Transmitter.Send(message);

        // Log the message for debugging (only if logging is enabled)
        LogMessage($"Sending OSC message to TouchDesigner: {oscAddress}, {soundPos.Azimuth}, {soundPos.Elevation}, {soundPos.Radius}, {soundID}");
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

        // Create a stop message for the sound and send it
        var stopMessage = new OSCMessage(_soundAddress + "/stop");
        stopMessage.AddValue(OSCValue.String(soundID));
        SafeSend(stopMessage); // Send the stop message

        // Remove the sound from the active continuous sounds dictionary
        activeContinuousSounds.Remove(soundID);
        LogMessage($"Stopped and removed continuous sound {soundID}.");
    }

    // Method to update the position of an already active continuous sound
    private void UpdateContinuousSound(string soundID, Vector3 position)
    {
        // Check if the sound exists in the dictionary
        if (activeContinuousSounds.TryGetValue(soundID, out OSCMessage message))
        {
            // Clear old values and update with the new position
            SoundPosition soundPos = new SoundPosition(position, sphereSize);
            message.Values.Clear();  // Clear old values
            AddOSCValues(message, 1, soundPos.Azimuth, soundPos.Elevation, soundPos.Radius, soundID);
            SafeSend(message);  // Send the updated message
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
