using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Newtonsoft.Json;
using UnityEngine;

public class AIChatbot : MonoBehaviour
{
    [SerializeField] private SpeechServiceConfig _speechServiceConfig;
    [SerializeField] private ChatbotAnimator _chatbotAnimator;

    private MicrosoftSpeechService _speechService;
    private List<Message> _messages;
    
    private async void Awake()
    {
        _speechService = new MicrosoftSpeechService(_speechServiceConfig, this);
        _speechService.Recognizer.Recognized += OnRecognized;
        
        _messages = new List<Message>
        {
            new("system", _speechServiceConfig.SystemPrompt)
        };
        
        await _speechService.Recognizer.StartContinuousRecognitionAsync();
    }

    private void OnRecognized(object sender, SpeechRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.RecognizedSpeech)
            GenerateResponseFromTextAsync(e.Result.Text, OnPromptSuccess);
    }

    public void QueueFrameData(FrameData newFrames)
    {
        _chatbotAnimator.QueueFrames(newFrames.BlendShapes);
    }
    
    private void OnPromptSuccess(string response)
    {
        GenerateAudioClip(response);
    }

    async void GenerateAudioClip(string response)
    {
        var audioDataStream = await _speechService.ConvertTextToSpeechAsync(response);
        _chatbotAnimator.PlaySpeechAudioStream(audioDataStream);
    }

    private async void GenerateResponseFromTextAsync(string text, Action<string> onSuccess)
    {
        var response = await GenerateResponseFromTextAsync(text);
        onSuccess?.Invoke(response);
    }

    private async Task<string> GenerateResponseFromTextAsync(string prompt)
    {
        _messages.Add(new Message("user", prompt));
        var jsonPayload = JsonConvert.SerializeObject(_messages);

        var response = await _speechService.GetResponse(jsonPayload);
        return response;
    }
}