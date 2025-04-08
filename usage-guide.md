# Usage Guide - Unity OpenAI Integration

This guide provides detailed instructions for using the Unity OpenAI Integration toolkit in your projects.

## Getting Started

### Installation

1. Clone or download the repository to your local machine
2. Import the package into your Unity project via `Assets > Import Package > Custom Package`
3. Ensure all scripts are imported correctly

### API Key Setup

1. Create a folder named `StreamingAssets` in your project's Assets directory if it doesn't exist
2. Create a text file named `api_key.txt` inside the StreamingAssets folder
3. Paste your OpenAI API key into this file with no extra spaces or line breaks
4. Save the file

**Note:** For production applications, consider implementing a more secure method for API key storage.

## Setting Up the ChatGPTRequester

### Basic Setup

1. Create an empty GameObject in your scene
2. Add the `ChatGPTRequester` component to this GameObject
3. Configure the component settings in the Inspector:
   - Select your preferred AI model (GPT-3.5 Turbo, GPT-4, GPT-4o, GPT-4o-Mini)
   - Set the temperature (0.0 - 1.0) - lower values give more deterministic responses
   - Set the max tokens limit for responses
   - Add theme instructions and negative prompts as needed

### UI Integration

To add a complete UI for interaction:

1. Create a Canvas with UI elements:
   - A TMP_InputField for user prompts
   - A Button for sending requests
   - A TMP_Text component for displaying responses
   - (Optional) A RawImage for displaying generated images
2. Assign these UI elements to the corresponding fields in the ChatGPTRequester Inspector

### Custom Theming

The ChatGPTRequester includes several fields for customizing the AI's output:

- **Theme Instructions**: Directs the AI's writing style (e.g., "fantasy theme", "professional tone")
- **Text Negative Prompt**: Specifies what to avoid in text responses
- **Image Negative Prompt**: Specifies what to avoid in generated images
- **Art Style**: Defines the visual style for DALL-E image generation

## Adding Text-to-Speech

1. Add the `ChatGPTTTSHandler` component to the same GameObject as your ChatGPTRequester
2. Create and assign an AudioSource component to the same GameObject
3. Assign this AudioSource to the TTS Handler in the Inspector

The TTS Handler automatically subscribes to the ChatGPTRequester's responses and converts them to speech.

## Using ChatGPTUtility

The `ChatGPTUtility` class can be used from anywhere in your code without needing to set up a GameObject:

```csharp
using UnityEngine;

public class MyGameScript : MonoBehaviour
{
    void GenerateDialogue()
    {
        string characterDescription = "A wise old wizard with a sense of humor";
        string situation = "The player has just entered the wizard's tower uninvited";
        
        string prompt = $"Write a short dialogue line for {characterDescription}. {situation}";
        
        string response = ChatGPTUtility.SendChatGPTRequest(
            prompt,
            "gpt-4o-mini",  // Using a smaller, faster model for dialogue
            0.8f,           // Slightly higher temperature for creativity
            100             // Short response
        );
        
        Debug.Log("Wizard says: " + response);
    }
}
```

## Advanced Usage

### Event-Based Response Handling

Subscribe to the OnAIResponseReceived event to handle responses programmatically:

```csharp
void Start()
{
    ChatGPTRequester requester = GetComponent<ChatGPTRequester>();
    requester.OnAIResponseReceived += HandleAIResponse;
}

void HandleAIResponse(string response)
{
    // Process the AI response
    // For example, parse it for game content, dialogue options, etc.
    string[] paragraphs = response.Split('\n');
    
    // Do something with the content...
}
```

### Creating Custom TTS Implementations

If you need more control over the TTS process:

```csharp
public class CustomTTSHandler : MonoBehaviour
{
    public ChatGPTRequester requester;
    public AudioSource audioSource;
    
    // Character voice settings
    [SerializeField] private string voiceType = "alloy"; // options: alloy, echo, fable, onyx, nova, shimmer
    
    void Start()
    {
        requester.OnAIResponseReceived += ProcessForCharacter;
    }
    
    void ProcessForCharacter(string response)
    {
        // You could process the text here before sending to TTS
        // For example, extracting only dialogue parts, or adding emotion markers
        
        StartCoroutine(SendToTTS(response));
    }
    
    IEnumerator SendToTTS(string text)
    {
        // Implementation similar to ChatGPTTTSHandler but with custom logic
        // ...
    }
}
```

## Troubleshooting

### Common Issues

1. **"API key file not found"**
   - Ensure you've created the api_key.txt file in StreamingAssets
   - Check that the file name is exactly "api_key.txt"
   - Verify the StreamingAssets folder is in the correct location

2. **"Error: API key is not loaded. Cannot send request."**
   - Verify your API key is valid and active
   - Check that the api_key.txt file contains only the key with no extra characters

3. **"ChatGPT Error" or "DALL-E Error"**
   - Check the Unity console for the complete error message
   - Verify your OpenAI account has access to the requested models
   - Ensure your API key has sufficient credits/permissions

4. **Audio not playing from TTS**
   - Verify the AudioSource component is properly assigned
   - Check that the AudioSource is not muted and has appropriate volume
   - Make sure the audio output is properly configured in your Unity project

## Performance Considerations

- API calls are rate-limited by OpenAI, so be mindful of request frequency
- DALL-E image generation and TTS conversion add additional latency and API costs
- Consider caching responses for common prompts to reduce API usage
- For mobile applications, be aware of potential bandwidth and battery usage

## Security Best Practices

- Do not include your API key in version control systems
- Consider implementing a server-side proxy for production applications
- Add encryption for the API key file in built applications
- Implement user authentication for multiplayer or online games
