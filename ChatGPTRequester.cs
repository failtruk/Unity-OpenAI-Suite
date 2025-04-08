using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Text;
using System.IO;

public class ChatGPTRequester : MonoBehaviour
{
    // Optional UI elements
    public Button requestButton;
    public TMP_Text responseText;
    public TMP_InputField inputField;
    public RawImage displayImage; // For displaying the generated image (optional)

    // API Key and URLs
    private string apiKey; // API key will be loaded from a file
    private string chatGptApiUrl = "https://api.openai.com/v1/chat/completions";
    private string dallEApiUrl = "https://api.openai.com/v1/images/generations";

    // AI Model Selection
    public enum AIModel
    {
        GPT_35_Turbo,
        GPT_4,
        GPT_4o,
        GPT_4o_Mini
    }

    [SerializeField]
    private AIModel selectedModel = AIModel.GPT_35_Turbo;

    // Additional prompts and styles
    [TextArea]
    public string themeInstructions = "Please ensure the response fits a dark fantasy theme, using archaic language and vivid imagery.";
    [TextArea]
    public string textNegativePrompt = "ENSURE TWO PARAGRAPHS MAX";
    [TextArea]
    public string imageNegativePrompt = "ENSURE NO TEXT";
    [TextArea]
    public string artStyle = "in the style of a pencil sketch";
    public float temperature = 0.7f;
    public int maxTokens = 300;

    // Delegate and event for handling AI responses
    public delegate void AIResponseHandler(string aiResponse);
    public event AIResponseHandler OnAIResponseReceived;

    void Awake()
    {
        Debug.Log("ChatGPTRequester Awake");
        LoadApiKey();
    }

    void Start()
    {
        Debug.Log("ChatGPTRequester Start");
        if (requestButton != null)
        {
            requestButton.onClick.AddListener(SendRequestFromInputField);
        }
    }

    // Method to load the API key from a file
    void LoadApiKey()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "api_key.txt");
        Debug.Log("Attempting to load API key from: " + filePath);

        if (File.Exists(filePath))
        {
            Debug.Log("API key file found.");
            apiKey = File.ReadAllText(filePath).Trim();
            Debug.Log("API key loaded successfully.");
        }
        else
        {
            Debug.LogError("API key file not found at: " + filePath);
        }
    }

    // Method to send a request using the text from the input field
    public void SendRequestFromInputField()
    {
        if (inputField != null)
        {
            string prompt = inputField.text;
            SendRequest(prompt);
        }
    }

    // Public method to send a request with a custom prompt
    public void SendRequest(string prompt)
    {
        // Check if API key is loaded
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API key is not loaded. Cannot send request.");
            return;
        }

        // Combine the prompt with theme instructions
        string modifiedPrompt = prompt + ". " + themeInstructions + ". NEGATIVE PROMPT - " + textNegativePrompt;
        StartCoroutine(PostChatGptRequest(modifiedPrompt));
    }

    IEnumerator PostChatGptRequest(string prompt)
    {
        // Determine the model name based on the selected model
        string modelName = GetModelName();

        // Construct the JSON payload for ChatGPT
        string json = "{\"model\": \"" + modelName + "\", \"messages\": [{\"role\": \"user\", \"content\": \"" + EscapeJsonString(prompt) + "\"}], \"max_tokens\": " + maxTokens + ", \"temperature\": " + temperature + "}";

        Debug.Log("ChatGPT JSON Payload: " + json);

        // Create a new UnityWebRequest for ChatGPT
        var request = new UnityWebRequest(chatGptApiUrl, "POST");

        // Convert the JSON string to bytes
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // Set the upload and download handlers
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Set the request headers
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        Debug.Log("Authorization Header: Bearer " + apiKey);

        // Send the request and wait for a response
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("ChatGPT Error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
            if (responseText != null)
            {
                responseText.text = "Error: " + request.error;
            }
        }
        else
        {
            Debug.Log("ChatGPT Response: " + request.downloadHandler.text);
            var response = JsonUtility.FromJson<OpenAIResponse>(request.downloadHandler.text);
            string gptResponse = response.choices[0].message.content;

            if (responseText != null)
            {
                responseText.text = gptResponse;
            }

            // Invoke the event to notify subscribers
            OnAIResponseReceived?.Invoke(gptResponse);

            // Conditionally request an image from DALL-E if displayImage is assigned
            if (displayImage != null)
            {
                StartCoroutine(PostDallERequest(gptResponse));
            }
        }
    }

    // Method to get the model name based on the selected model
    private string GetModelName()
    {
        switch (selectedModel)
        {
            case AIModel.GPT_35_Turbo:
                return "gpt-3.5-turbo";
            case AIModel.GPT_4:
                return "gpt-4";
            case AIModel.GPT_4o:
                return "gpt-4o";
            case AIModel.GPT_4o_Mini:
                return "gpt-4o-mini";
            default:
                return "gpt-3.5-turbo"; // Default to gpt-3.5-turbo if not set
        }
    }

    // Method to send a request to DALL-E for image generation
    IEnumerator PostDallERequest(string prompt)
    {
        // Check if API key is loaded
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API key is not loaded. Cannot send DALL-E request.");
            yield break;
        }

        // Combine prompt with art style and negative prompt
        string imagePrompt = prompt + ". " + artStyle + ". NEGATIVE PROMPT = " + imageNegativePrompt;

        // Enforce 1000-character limit on the image prompt
        if (imagePrompt.Length > 1000)
        {
            Debug.LogWarning("Image prompt exceeds 1000 characters. Truncating to 1000 characters.");
            imagePrompt = imagePrompt.Substring(0, 1000);

            // Optionally, ensure we don't cut off in the middle of a word
            int lastSpaceIndex = imagePrompt.LastIndexOf(' ');
            if (lastSpaceIndex > 0)
            {
                imagePrompt = imagePrompt.Substring(0, lastSpaceIndex);
            }
        }

        // Construct the JSON payload for DALL-E
        string json = "{\"prompt\": \"" + EscapeJsonString(imagePrompt) + "\", \"n\": 1, \"size\": \"512x512\"}";

        Debug.Log("DALL-E JSON Payload: " + json);

        // Create a new UnityWebRequest for DALL-E
        var request = new UnityWebRequest(dallEApiUrl, "POST");

        // Convert the JSON string to bytes
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // Set the upload and download handlers
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Set the request headers
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        // Send the request and wait for a response
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("DALL-E Error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
            if (responseText != null)
            {
                responseText.text += "\nDALL-E Error: " + request.error;
            }
        }
        else
        {
            Debug.Log("DALL-E Response: " + request.downloadHandler.text);
            var response = JsonUtility.FromJson<DallEResponse>(request.downloadHandler.text);
            string imageUrl = response.data[0].url;

            // Load the image from the URL
            StartCoroutine(LoadImage(imageUrl));
        }
    }

    // Method to load the image from the URL
    IEnumerator LoadImage(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Image Load Error: " + request.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            if (displayImage != null)
            {
                displayImage.texture = texture;
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

[System.Serializable]
public class OpenAIResponse
{
    public Choice[] choices;

    [System.Serializable]
    public class Choice
    {
        public Message message;
        public string finish_reason;
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }
}

[System.Serializable]
public class DallEResponse
{
    public DallEData[] data;

    [System.Serializable]
    public class DallEData
    {
        public string url;
    }
}
