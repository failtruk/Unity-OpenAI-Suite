# Unity OpenAI Integration

A comprehensive toolkit for integrating OpenAI's powerful API services directly into Unity projects.

## Features

- **Text Generation** via GPT models (3.5-Turbo, GPT-4, GPT-4o, GPT-4o-Mini)
- **Image Generation** via DALL-E
- **Text-to-Speech** conversion using OpenAI's TTS API
- **Utility functions** for easy integration into any Unity project

## Components

### ChatGPTRequester

This core component handles communication with OpenAI's ChatGPT and DALL-E APIs.

#### Key Features:
- Support for multiple GPT models (GPT-3.5 Turbo, GPT-4, GPT-4o, GPT-4o-Mini)
- Customizable temperature and token settings
- Theme instructions and negative prompts for style control
- Optional UI integration with input fields, buttons, and display areas
- Event system for handling AI responses
- Automatic image generation through DALL-E based on text responses

#### Usage:
1. Add the `ChatGPTRequester` component to a GameObject
2. Optionally connect UI elements (Button, TMP_InputField, TMP_Text, RawImage)
3. Customize theme instructions and negative prompts
4. Place your OpenAI API key in `StreamingAssets/api_key.txt`

#### Example:
```csharp
// Get a reference to the ChatGPTRequester
ChatGPTRequester requester = GetComponent<ChatGPTRequester>();

// Send a custom prompt
requester.SendRequest("Create a short story about a magical forest");

// Subscribe to AI responses
requester.OnAIResponseReceived += (response) => {
    Debug.Log("Received response: " + response);
};
```

### ChatGPTTTSHandler

This component converts text responses from ChatGPT into spoken audio using OpenAI's Text-to-Speech API.

#### Key Features:
- Automatic conversion of AI text responses to speech
- Integration with Unity's AudioSource component
- High-quality "onyx" voice preset

#### Usage:
1. Add the `ChatGPTTTSHandler` component to the same GameObject as `ChatGPTRequester`
2. Assign an AudioSource component in the Inspector
3. Ensure your OpenAI API key has TTS permissions

### ChatGPTUtility

A static utility class for making synchronous requests to OpenAI's API from anywhere in your code.

#### Key Features:
- Simple one-line API for ChatGPT integration
- Customizable model, temperature, and token settings
- Editor-friendly synchronous requests

#### Usage:
```csharp
// Make a simple request to ChatGPT
string response = ChatGPTUtility.SendChatGPTRequest(
    "Explain quantum computing in simple terms", 
    "gpt-4",                // Model
    0.7f,                   // Temperature
    512                     // Max tokens
);

Debug.Log(response);
```

### WavUtility

A utility class for converting WAV audio data to Unity AudioClip objects.

#### Key Features:
- WAV file parsing and conversion
- Creates Unity-compatible AudioClip objects

## Setup Instructions

1. Import the package into your Unity project
2. Create a `StreamingAssets` folder in your Assets directory if it doesn't exist
3. Create a text file named `api_key.txt` inside `StreamingAssets`
4. Paste your OpenAI API key into this file (with no extra spaces or line breaks)
5. Add the `ChatGPTRequester` component to a GameObject in your scene
6. (Optional) Add the `ChatGPTTTSHandler` component to the same GameObject
7. (Optional) Connect UI elements in the Inspector

## API Key Security

**Important**: The current implementation stores the API key in plaintext in the StreamingAssets folder. For production applications, consider implementing:

- Encryption for the API key file
- A more secure key storage mechanism
- A server-side proxy to avoid exposing API keys in client applications

## Requirements

- Unity 2021.3 LTS or newer
- TMPro package for UI text elements
- Active OpenAI API key with access to the required models

## License

[Your License Information]

## Credits

[Your Credits Information]
