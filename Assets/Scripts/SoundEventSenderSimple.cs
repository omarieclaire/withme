// using UnityEngine;
// using extOSC;
// using System.Collections.Generic;

// public class SoundEventSender : MonoBehaviour
// {
//     public OSCTransmitter Transmitter;

//     private const string _soundAddress = "/sound/play";
//     private Vector3 _position = Vector3.zero;
//     private Dictionary<string, OSCMessage> activeContinuousSounds = new Dictionary<string, OSCMessage>();

//     protected virtual void Start()
//     {
//         // Comment out the lines that reference Text components
//         // TextSoundID.text = "Sound ID: None";
//         // TextPosition.text = $"{Vector3.zero}";
//     }

//     public void SendSoundEvent(string soundID, Vector3 position, SoundType soundType)
//     {
//         _position = position;
//         switch (soundType)
//         {
//             case SoundType.OneShot:
//                 SendSound(_soundAddress, soundID, _position);
//                 break;
//             case SoundType.Continuous:
//                 if (!activeContinuousSounds.ContainsKey(soundID))
//                 {
//                     OSCMessage message = CreateSoundMessage(_soundAddress, soundID, _position);
//                     activeContinuousSounds[soundID] = message;
//                     Transmitter.Send(message);
//                 }
//                 else
//                 {
//                     UpdateContinuousSound(soundID, position);
//                 }
//                 break;
//         }

//         // Comment out the lines that reference Text components
//         // TextSoundID.text = $"Sound ID: {soundID}";
//         // TextPosition.text = _position.ToString();
//     }

//     public void StopContinuousSound(string soundID)
//     {
//         if (activeContinuousSounds.ContainsKey(soundID))
//         {
//             var stopMessage = new OSCMessage(_soundAddress + "/stop");
//             stopMessage.AddValue(OSCValue.String(soundID));
//             Transmitter.Send(stopMessage);
//             activeContinuousSounds.Remove(soundID);
//         }
//     }

//     private void UpdateContinuousSound(string soundID, Vector3 position)
//     {
//         if (activeContinuousSounds.TryGetValue(soundID, out OSCMessage message))
//         {
//             message.Values.Clear();
//             message.AddValue(OSCValue.String(soundID));
//             message.AddValue(OSCValue.Float(position.x));
//             message.AddValue(OSCValue.Float(position.y));
//             message.AddValue(OSCValue.Float(position.z));
//             Transmitter.Send(message);
//         }
//     }

//     private void SendSound(string address, string soundID, Vector3 position)
//     {
//         if (Transmitter == null)
//         {
//             Debug.LogError("Transmitter is not assigned in the SoundEventSender script.");
//             return;
//         }

//         OSCMessage message = CreateSoundMessage(address, soundID, position);

//         // Log the OSC message
//         Debug.Log($"Sending OSC Message: {SerializeOSCMessage(message)}");

//         Transmitter.Send(message);
//     }

//     private OSCMessage CreateSoundMessage(string address, string soundID, Vector3 position)
//     {
//         var message = new OSCMessage(address);
//         message.AddValue(OSCValue.String(soundID));
//         message.AddValue(OSCValue.Float(position.x));
//         message.AddValue(OSCValue.Float(position.y));
//         message.AddValue(OSCValue.Float(position.z));
//         return message;
//     }

//     private string SerializeOSCMessage(OSCMessage message)
//     {
//         string result = message.Address;
//         foreach (var value in message.Values)
//         {
//             result += $" {value}";
//         }
//         return result;
//     }
// }
