using UnityEngine;
using Whisper;
using Whisper.Utils;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent (typeof(WhisperManager))]
[RequireComponent (typeof(MicrophoneRecord))]

public class SpeechToText : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text outputText;
    public Button button;
    public TMP_Text buttonText;

    [Header("Events")]
    public UnityEvent<string> onDoneSpeaking;

    // Whisper components, needed for STT to work
    WhisperManager whisper;
    MicrophoneRecord recorder;

    string _buffer = " ";

    void Awake()
    {
        whisper = gameObject.GetComponent<WhisperManager>();
        recorder = gameObject.GetComponent<MicrophoneRecord>();

        // Whisper delegates
        recorder.OnRecordStop += OnRecordStop; // For some reason this is the syntax for C# delegates I guess?
        recorder.OnVadChanged += OnVadChanged;
        whisper.OnNewSegment += OnNewSegment;

        // EventSystems
        if (button == null) { throw new System.Exception("Button has not been assigned"); }
        button.onClick.AddListener(OnButtonPressed);
    }

    private void OnButtonPressed()
    {
        if (buttonText == null) { throw new System.Exception("Button Text has not been assigned"); }
        if (!recorder.IsRecording)
        {
            recorder.StartRecord();
            buttonText.text = "STOP LISTENING";
        }
        else
        {
            recorder.StopRecord();
        }
    }

    private async void OnRecordStop(AudioChunk recordedAudio)
    {
        var result = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
        if (outputText == null) { throw new System.Exception("Output Text has not been assigned"); }
        outputText.text = result.Result;
        onDoneSpeaking.Invoke(result.Result);

        if (buttonText == null) { throw new System.Exception("Button Text has not been assigned"); }
        buttonText.text = "START LISTENING";
    }

    private void OnVadChanged(bool speechDetected)
    {
        if (speechDetected)
        {
            Debug.Log("Voice detected!");

        }
        else
        {
            Debug.Log("Voice no longer detected.");
        }
    }

    private void OnNewSegment(WhisperSegment segment)
    {
        if (outputText == null) { throw new System.Exception("Output Text has not been assigned"); }
        _buffer += segment.Text;
        outputText.text = _buffer;
        Debug.Log($"Segment processed: {segment.Text}");
    }
}
