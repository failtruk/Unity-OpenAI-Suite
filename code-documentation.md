# Code Documentation - Unity OpenAI Integration

This document provides a detailed breakdown of each component in the Unity OpenAI Integration toolkit.

## ChatGPTRequester.cs

The main component responsible for communicating with OpenAI's API services for text and image generation.

### Class Properties

| Property | Type | Description |
|----------|------|-------------|
| `requestButton` | Button | UI button that triggers the request |
| `responseText` | TMP_Text | Text component for displaying responses |
| `inputField` | TMP_InputField | Input field for entering prompts |
| `displayImage` | RawImage | UI element for displaying generated images |
| `apiKey` | string (private) | OpenAI API key loaded from file |
| `chatGptApiUrl` | string (private) | URL endpoint for ChatGPT API |
| `dallEApiUrl` | string (private) | URL endpoint for DALL-E API |
| `selectedModel` | AIModel | Enum for selecting the GPT model |
| `themeInstructions` | string | Instructions for styling the response |
| `textNegativePrompt` | string | Items to avoid in text responses |
| `imageNegativePrompt` | string | Items to avoid in generated images |
| `artStyle` | string | Style description for DALL-E generations |
| `temperature` | float | Controls randomness (0.0-1.0) |
| `maxTokens` | int | Maximum tokens in the response |

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnAIResponseReceived` | `AIResponseHandler` | Invoked when an AI response is received |

### Methods

| Method | Parameters | Return | Description |
|--------|------------|--------|-------------|
| `LoadApiKey()` | none | void | Loads API key from StreamingAssets |
| `SendRequestFromInputField()` | none | void | Sends the text from inputField to API |
| `SendRequest()` | string prompt | void | Sends a prompt to ChatGPT |
| `PostChatGptRequest()` | string prompt | IEnumerator | Coroutine for API communication |
| `GetModelName()` | none | string | Converts enum to API model name |
| `PostDallERequest()` | string prompt | IEnumerator | Requests image generation |
| `LoadImage()` | string url | IEnumerator | Loads image from URL |
| `EscapeJsonString()` | string str | string | Escapes special characters for JSON |

### Helper Classes

- `OpenAIResponse`: Serializable class for parsing ChatGPT responses
- `DallEResponse`: Serializable class for parsing DALL-E responses

## ChatGPTTTSHandler.cs

Component for converting text responses to spoken audio using OpenAI's TTS API.

### Class Properties

| Property | Type | Description |
|----------|------|-------------|
| `audioSource` | AudioSource | Component for playing audio |
| `apiKey` | string (private) | OpenAI API key loaded from file |
| `ttsApiUrl` | string (private) | URL endpoint for TTS API |

### Methods

| Method | Parameters | Return | Description |
|--------|------------|--------|-------------|
| `LoadApiKey()` | none | void | Loads API key from StreamingAssets |
| `HandleAIResponseForTTS()` | string textResponse | void | Processes text for TTS conversion |
| `PostTTSRequest()` | string prompt | IEnumerator | Coroutine for TTS API communication |
| `EscapeJsonString()` | string str | string | Escapes special characters for JSON |

## ChatGPTUtility.cs

Static utility class for making ChatGPT requests from anywhere in code.

### Constants

| Constant | Type | Description |
|----------|------|-------------|
| `OpenAIChatApiUrl` | string | URL endpoint for ChatGPT API |

### Methods

| Method | Parameters | Return | Description |
|--------|------------|--------|-------------|
| `SendChatGPTRequest()` | string prompt, string model, float temperature, int maxTokens | string | Synchronous request to ChatGPT |
| `LoadApiKey()` | none | string | Loads API key from StreamingAssets |

### Helper Classes

- `OpenAIChatRequest`: Serializable class for creating request payloads
- `OpenAIResponse`: Serializable class for parsing responses

## WavUtility.cs

Utility for converting WAV audio data to Unity AudioClip objects.

### Methods

| Method | Parameters | Return | Description |
|--------|------------|--------|-------------|
| `ToAudioClip()` | byte[] wavFile | AudioClip | Converts WAV byte data to AudioClip |

## Workflow Diagram

```
┌─────────────────┐     ┌─────────────────────┐     ┌────────────────┐
│                 │     │                     │     │                │
│  User Input     │────▶│  ChatGPTRequester   │────▶│  Display Text  │
│  (Prompt)       │     │  (API Processing)   │     │  Response      │
│                 │     │                     │     │                │
└─────────────────┘     └──────────┬──────────┘     └────────────────┘
                                   │
                                   │ OnAIResponseReceived Event
                                   │
                     ┌─────────────▼──────────────┐
                     │                            │
                     │                            │
         ┌───────────▼────────────┐    ┌─────────▼──────────┐
         │                        │    │                    │
         │  ChatGPTTTSHandler     │    │  DALL-E Request    │
         │  (Text-to-Speech)      │    │  (Image Generation)│
         │                        │    │                    │
         └───────────┬────────────┘    └─────────┬──────────┘
                     │                           │
                     │                           │
         ┌───────────▼────────────┐    ┌─────────▼──────────┐
         │                        │    │                    │
         │  Play Audio via        │    │  Display Image     │
         │  AudioSource           │    │  via RawImage      │
         │                        │    │                    │
         └────────────────────────┘    └────────────────────┘
```

## API Models

| Model Name | API Value | Description |
|------------|-----------|-------------|
| GPT-3.5 Turbo | "gpt-3.5-turbo" | Fastest model, good for simple tasks |
| GPT-4 | "gpt-4" | More capable model, better reasoning |
| GPT-4o | "gpt-4o" | Multimodal capabilities, latest model |
| GPT-4o Mini | "gpt-4o-mini" | Smaller, faster version of GPT-4o |

## Error Handling

All components include error handling that:
1. Logs detailed errors to the Unity console
2. Updates UI elements with error information when available
3. Provides context about which API call failed (ChatGPT, DALL-E, or TTS)

## Thread Safety

- All API communications use Unity's coroutine system to prevent blocking the main thread
- The static `ChatGPTUtility.SendChatGPTRequest()` method is synchronous and should be used carefully

## Memory Management

- All web requests properly dispose of resources using `using` statements
- Downloaded textures are properly assigned to UI elements

## Security Notes

- API keys are stored in plaintext within the StreamingAssets folder
- For production applications, implement a more secure approach for API key management
- Consider using a server-side proxy to avoid exposing API keys in client applications
