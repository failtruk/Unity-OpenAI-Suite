using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class ChatGPTUtility
{
    private const string OpenAIChatApiUrl = "https://api.openai.com/v1/chat/completions";

    /// <summary>
    /// Sends a synchronous request to ChatGPT and returns the assistant's content.
    /// </summary>
    /// <param name="prompt">Prompt to send to ChatGPT</param>
    /// <param name="model">e.g., "gpt-3.5-turbo" or "gpt-4"</param>
    /// <param name="temperature">Controls randomness. 0 = deterministic</param>
    /// <param name="maxTokens">Maximum tokens for the response</param>
    /// <returns>The response content or null if an error occurred</returns>
    public static string SendChatGPTRequest(string prompt, string model = "gpt-3.5-turbo", float temperature = 0.7f, int maxTokens = 512)
    {
        string apiKey = LoadApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("No API key found. Cannot send ChatGPT request.");
            return null;
        }

        var requestPayload = new OpenAIChatRequest
        {
            model = model,
            messages = new OpenAIChatRequest.Message[]
            {
                new OpenAIChatRequest.Message { role = "user", content = prompt }
            },
            max_tokens = maxTokens,
            temperature = temperature
        };

        string jsonData = JsonUtility.ToJson(requestPayload);

        using (UnityWebRequest webRequest = new UnityWebRequest(OpenAIChatApiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);

            var operation = webRequest.SendWebRequest();
            while (!operation.isDone) { }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string responseText = webRequest.downloadHandler.text;
                OpenAIResponse response = JsonUtility.FromJson<OpenAIResponse>(responseText);
                if (response != null && response.choices != null && response.choices.Length > 0)
                {
                    return response.choices[0].message.content.Trim();
                }
                else
                {
                    Debug.LogWarning("Received response but no valid choices found.");
                    return null;
                }
            }
            else
            {
                Debug.LogError("ChatGPT Error: " + webRequest.error);
                Debug.LogError("Response: " + webRequest.downloadHandler.text);
                return null;
            }
        }
    }

    private static string LoadApiKey()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "api_key.txt");
        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath).Trim();
        }

        Debug.LogWarning("API key file not found at: " + filePath);
        return null;
    }

    [System.Serializable]
    private class OpenAIChatRequest
    {
        public string model;
        public Message[] messages;
        public int max_tokens;
        public float temperature;

        [System.Serializable]
        public class Message
        {
            public string role;
            public string content;
        }
    }

    [System.Serializable]
    private class OpenAIResponse
    {
        public Choice[] choices;

        [System.Serializable]
        public class Choice
        {
            public OpenAIChatRequest.Message message;
            public string finish_reason;
        }
    }
}
