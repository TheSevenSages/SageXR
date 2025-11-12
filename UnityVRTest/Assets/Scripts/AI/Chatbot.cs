using LLMUnity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Chatbot : MonoBehaviour
{
    public TMP_Text output;
    public UnityEvent<string> onDoneGenerating;

    public async void SendAIMessage(string message)
    {
        try
        {
            LLMCharacter character = gameObject.GetComponent<LLMCharacter>();
            if (character == null)
            {
                throw new Exception("LLMCharacter component is missing from this GameObject");
            }
            else
            {
                character.CancelRequests(); // Cancel any requests to this character that are in progress
                var response = await character.Chat(message, null, null);
                onDoneGenerating.Invoke(response);
                output.text = response;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
