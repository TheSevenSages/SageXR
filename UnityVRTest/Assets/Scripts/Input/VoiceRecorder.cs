using System.Threading.Tasks;
using UnityEngine;

public class VoiceRecorder : MonoBehaviour
{
    public AudioSource source;
    AudioClip clip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(Microphone.devices[0]);
        int min_freq = 0;
        int max_freq = 0;
        Microphone.GetDeviceCaps(Microphone.devices[0], out min_freq, out max_freq);
        clip = Microphone.Start(Microphone.devices[0], false, 120, max_freq);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(Microphone.GetPosition(Microphone.devices[0]));

        if (Input.GetKeyDown("space"))
        {
            if (Microphone.IsRecording(Microphone.devices[0]))
            {
                Microphone.End(Microphone.devices[0]);
            }
            else
            {
                source.clip = clip;
                source.Play();
            }  
        }
    }
}
