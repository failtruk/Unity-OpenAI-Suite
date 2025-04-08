using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.IO;

public class ChatGPTTTSHandler : MonoBehaviour
{
    public AudioSource audioSource; // Assign an AudioSource component in the Unity Inspector

    private string apiKey;
    private string ttsApiUrl = "https://api.openai.com/v1/audio/speech";

    void Awake()
    {
        LoadApiKey();
    }

    void Start()
    {
        // Assuming the ChatGPTRequester script is on the same GameObject
        ChatGPTRequester requester = GetComponent<ChatGPTRequester>();
        if (requester != null)
        {
            requester.OnAIResponseReceived += HandleAIResponseForTTS;
        }
    }

    // Method to load the API key from a file (same logic as in ChatGPTRequester)
    void LoadApiKey()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "api_key.txt");
        if (File.Exists(filePath))
        {
            apiKey = File.ReadAllText(filePath).Trim();
        }
        else
        {
            Debug.LogError("API key file not found at: " + filePath);
        }
    }

    // Method to handle AI response and convert it to audio narration
    private void HandleAIResponseForTTS(string textResponse)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API key is not loaded. Cannot send TTS request.");
            return;
        }

        StartCoroutine(PostTTSRequest(textResponse));
    }

    IEnumerator PostTTSRequest(string prompt)
    {
        // Construct the JSON payload for TTS, requesting MP3 format
        string json = "{\"input\": \"" + EscapeJsonString(prompt) + "\", \"model\": \"tts-1\", \"voice\": \"onyx\", \"format\": \"mp3\"}";

        Debug.Log("TTS JSON Payload: " + json);

        // Create a new UnityWebRequest for TTS
        var request = new UnityWebRequest(ttsApiUrl, "POST");

        // Convert the JSON string to bytes
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // Set the upload handler
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);

        // Use DownloadHandlerAudioClip to handle the audio data
        request.downloadHandler = new DownloadHandlerAudioClip(ttsApiUrl, AudioType.MPEG);

        // Set the request headers
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        // Send the request and wait for a response
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("TTS Error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("TTS Response Received");

            // Get the AudioClip from the download handler
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);

            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("Failed to load audio clip.");
            }
        }
    }

    // Utility method to escape JSON string
    string EscapeJsonString(string str)
    {
        StringBuilder sb = new StringBuilder(str);
        sb.Replace("\\", "\\\\");
        sb.Replace("\"", "\\\"");
        sb.Replace("\n", "\\n");
        sb.Replace("\r", "\\r");
        sb.Replace("\t", "\\t");
        return sb.ToString();
    }
}
