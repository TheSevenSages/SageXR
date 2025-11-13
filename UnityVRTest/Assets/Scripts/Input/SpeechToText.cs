using UnityEngine;
using Whisper;
using Whisper.Utils;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

[RequireComponent (typeof(WhisperManager))]
[RequireComponent (typeof(MicrophoneRecord))]
public class SpeechToText : MonoBehaviour
{
    [Header("VAD (Custom)")]
    [Tooltip("After how many seconds of silence we will count as 'done speaking'")]
    public float stopTime = 3.0f;

    [Header("UI")]
    public TMP_Text outputText;

    [Header("Events")]
    public UnityEvent<string> onDoneSpeaking;

    // Whisper components, needed for STT to work
    WhisperManager whisper;
    MicrophoneRecord recorder;

    // Used to track if the system should switch modes or not
    Coroutine countdown;
    bool _countdownRunning = false;

    // State variables
    // OFF -> The system is off, default state
    // DETECTING -> The system is detecting when the user starts speaking, at which point it goes to LISTENING mode
    // LISTENING -> The system has detected speech and is now listening to what the user is saying to be transcribed by whisper
    enum STTState {OFF, DETECTING, LISTENING};
    STTState state = STTState.OFF;

    void Awake()
    {
        whisper = gameObject.GetComponent<WhisperManager>();
        recorder = gameObject.GetComponent<MicrophoneRecord>();

        // Whisper delegates
        recorder.OnRecordStop += OnRecordStop; // For some reason this is the syntax for C# delegates I guess?
        recorder.OnVadChanged += OnVadChanged;
    }

    // Begins recording to the connected microphone to detect when the user starts speaking
    void StartDetecting()
    {
        if (state == STTState.DETECTING) { return; }
        state = STTState.DETECTING;

        if (!recorder.IsRecording)
        {
            recorder.StartRecord();
        }
    }

    // Turns the system off
    void TurnOff()
    {
        if (state == STTState.OFF) { return; }
        state = STTState.OFF;
        recorder.StopRecord();
    }

    public void ToggleOnOff()
    {
        if (state == STTState.OFF)
        {
            StartDetecting();
        }
        else
        {
            TurnOff();
        }
    }

    private void OnVadChanged(bool speechDetected)
    {
        // If no speech is detected in the given time frame we go from listening back to detecting
        if (speechDetected)
        {
            state = STTState.LISTENING;
            if (_countdownRunning && countdown != null)
            {
                StopCoroutine(countdown);
                _countdownRunning = false;
            }
        }
        else
        {
            if (!_countdownRunning)
            {
                countdown = StartCoroutine(Countdown());
            }
        }
    }
    IEnumerator Countdown()
    {
        _countdownRunning = true;
        yield return new WaitForSecondsRealtime(stopTime);
        _countdownRunning = false;

        // Silence detected for 3 seconds, user has stopped speaking
        recorder.StopRecord();
    }

    // Called by the recorder's delegate of the same name
    private async void OnRecordStop(AudioChunk recordedAudio)
    {
        if (state == STTState.LISTENING)
        {
            // 1. Send the audio to the whisper manager to get the transcribed speech
            var result = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            
            // 2. Do whatever we're going to do with the text
            if (outputText == null) { throw new System.Exception("Output Text has not been assigned"); }
            outputText.text = result.Result;
            onDoneSpeaking.Invoke(result.Result);

            // 3. Put the system back into DETECTING mode
            StartDetecting();
        }
        else if (state == STTState.DETECTING)
        {
            // We don't care about non-speech data, do nothing with the recorded audio
            return;
        }
        else
        {
            // State is OFF, do nothing with the recorded audio
            return;
        }
    }

}
