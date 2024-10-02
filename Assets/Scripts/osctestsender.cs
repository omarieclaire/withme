using UnityEngine;
using extOSC;

public class SimpleSoundTest : MonoBehaviour
{
    public OSCTransmitter Transmitter;
    public string soundID = "testSound";
    public int sourceIndex = 1;

    void Start()
    {
        // Ensure the transmitter is assigned
        if (Transmitter == null)
        {
            Debug.LogError("OSC Transmitter is not assigned!");
        }
    }

    // Play sound
    public void PlaySound()
    {
        OSCBundle b;
        OSCMessage message = new OSCMessage("/sound/play");
        message.AddValue(OSCValue.String(soundID));  // Add soundID to message
        Transmitter.Send(message);
        Debug.Log("Play sound sent for: " + soundID);
    }

    // Stop sound
    public void StopSound()
    {
        if (Transmitter != null)
        {
            OSCMessage message = new OSCMessage("/sound/stop");
            message.AddValue(OSCValue.String(soundID));
            Transmitter.Send(message);
            Debug.Log("Stop sound sent for: " + soundID);  // Add this line
        }
    }

    // Update method for keyboard input testing
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlaySound();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            StopSound();
        }
    }
}
