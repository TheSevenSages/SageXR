using LLMUnity;
using System;
using TMPro;
using UnityEngine;

public class Chatbot : MonoBehaviour
{
    public TMP_Text output;
    // Is called periodically while the response is being recieved
    void HandleReply(string reply)
    {
        //Debug.Log($"AI RESPONSE: {reply}");
        output.text = reply;
    }

    // Is called when the AI has finished formulating its response
    void ReplyCompleted()
    {
        Debug.Log("AI IS DONE RESPONDING");
    }

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
                var response = await character.Chat(message, null, ReplyCompleted);
                output.text = response;
                Debug.Log($"AI RESPONSE: {response}");
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
