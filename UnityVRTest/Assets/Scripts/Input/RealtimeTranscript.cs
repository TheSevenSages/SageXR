using UnityEngine;
using UnityEngine.UI;
using Whisper;
using Whisper.Utils;

public class RealtimeTranscript : MonoBehaviour
{
    public WhisperManager whisperManager;
    public MicrophoneRecord microphoneRecord;
    public TMPro.TMP_Text outputText;

    // A flag to check if the mic has started
    private bool isRecording = false;

    void Start()
    {
        Debug.Log("RealtimeTranscript: Start() called.");

        if (whisperManager == null)
        {
            Debug.LogError("RealtimeTranscript: WhisperManager is NOT set in the Inspector!");
            return;
        }

        if (microphoneRecord == null)
        {
            Debug.LogError("RealtimeTranscript: MicrophoneRecord is NOT set in the Inspector!");
            return;
        }

        Debug.Log("RealtimeTranscript: Subscribing to OnNewSegment event.");

        // Subscribe to the event
        whisperManager.OnNewSegment += OnNewSegmentHandler;

        // Check if the microphone is already recording (it shouldn't be, but good to check)
        if (microphoneRecord.IsRecording)
        {
            Debug.LogWarning("RealtimeTranscript: Mic was already recording.");
            isRecording = true;
            return;
        }

        // Start recording
        microphoneRecord.StartRecord();
        isRecording = true;
        Debug.Log("RealtimeTranscript: Microphone recording started.");
    }

    private void Update()
    {
        // This will print every 5 seconds to let you know the app hasn't frozen
        if (Time.frameCount % 300 == 0)
        {
            if (isRecording)
            {
                Debug.Log("RealtimeTranscript: Update() running. Still recording...");
            }
            else
            {
                Debug.LogWarning("RealtimeTranscript: Update() running, but mic is NOT recording.");
            }
        }
    }

    // This is the event handler function
    private void OnNewSegmentHandler(WhisperSegment segment)
    {
        // This is the most important log!
        Debug.LogWarning($"==> NEW SEGMENT: {segment.Text}");

        string text = segment.Text;
        if (outputText != null)
        {
            outputText.text += text;
        }
    }

    private void OnDestroy()
    {
        Debug.Log("RealtimeTranscript: OnDestroy() called. Unsubscribing from event.");
        if (whisperManager != null)
        {
            whisperManager.OnNewSegment -= OnNewSegmentHandler;
        }
    }
}