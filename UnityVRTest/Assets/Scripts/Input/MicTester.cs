using UnityEngine;

public class MicTester : MonoBehaviour
{
    private string micDevice;
    private AudioClip micClip;
    private float[] samples = new float[1024]; // Buffer to check audio data
    private int lastMicPosition = 0;

    void Start()
    {
        Debug.Log("--- MicTester: Start() ---");

        // 1. List all available microphones
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("MicTester: FATAL ERROR: No microphone devices found!");
            return;
        }

        Debug.Log("MicTester: Available microphone devices:");
        foreach (string device in Microphone.devices)
        {
            Debug.Log("- " + device);
        }

        // 2. Start recording with the default device
        micDevice = Microphone.devices[0]; // Use the first one
        Debug.Log("MicTester: Starting recording on default device: " + micDevice);

        // Start a 10-second looping clip
        micClip = Microphone.Start(micDevice, true, 10, 44100);

        if (micClip == null)
        {
            Debug.LogError("MicTester: FATAL ERROR: Microphone.Start() failed to return an AudioClip.");
            return;
        }

        Debug.Log("MicTester: Microphone.Start() successful. Recording... SPEAK NOW!");
    }

    void Update()
    {
        if (micClip == null) return;

        int currentMicPosition = Microphone.GetPosition(micDevice);

        // This check is to see if the mic is actually running
        if (currentMicPosition == lastMicPosition)
        {
            return;
        }

        // Check for loudness every 30 frames
        if (Time.frameCount % 30 == 0)
        {
            // Get the latest chunk of data
            micClip.GetData(samples, currentMicPosition);

            float totalLoudness = 0f;
            foreach (float sample in samples)
            {
                totalLoudness += Mathf.Abs(sample);
            }
            float averageLoudness = totalLoudness / samples.Length;

            // This is the number we care about. 0.001 is a reasonable "silence" threshold.
            if (averageLoudness > 0.001f)
            {
                // This is the "SUCCESS" message we are looking for
                Debug.LogWarning($"MicTester: LOUDNESS DETECTED! Avg: {averageLoudness}");
            }
            else
            {
                Debug.Log("MicTester: ...silence detected... (Avg: " + averageLoudness + ")");
            }
        }

        lastMicPosition = currentMicPosition;
    }

    void OnDestroy()
    {
        if (Microphone.IsRecording(micDevice))
        {
            Microphone.End(micDevice);
        }
    }
}