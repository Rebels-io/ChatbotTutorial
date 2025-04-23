using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Newtonsoft.Json;
using UnityEngine;

public class MicrosoftSpeechService
{
    public SpeechRecognizer Recognizer { get; private set; }

    private SpeechServiceConfig _serviceConfig;
    private SpeechConfig _speechConfig;
    private AIChatbot _chatbot;

    public MicrosoftSpeechService(SpeechServiceConfig serviceConfig, AIChatbot chatbot)
    {
        _chatbot = chatbot;
        _serviceConfig = serviceConfig;
        
        _speechConfig = SpeechConfig.FromSubscription(serviceConfig.SpeechKey, serviceConfig.Region);
        _speechConfig.SpeechSynthesisVoiceName = serviceConfig.Voice;
        
        InitializeRecognizer();
    }

    void InitializeRecognizer()
    {
        var audioInput = AudioConfig.FromDefaultMicrophoneInput();
        Recognizer = new SpeechRecognizer(_speechConfig, audioInput);
        
        Recognizer.Canceled += (s, e) => Debug.LogWarning($"Canceled: {e.Reason}");
        Recognizer.SessionStopped += (s, e) => Debug.Log("Session stopped.");
        
        Recognizer.Recognized += RecognizerOnRecognized;
    }

    private void RecognizerOnRecognized(object sender, SpeechRecognitionEventArgs e)
    {
        var recognizedText = e.Result.Text;
        Debug.Log(recognizedText);
    }

    public async Task<string> GetResponse(string prompt)
    {
        // build the request
        var content = new StringContent($@"{{""messages"":{prompt},""max_tokens"":100}}", Encoding.UTF8,
            "application/json");
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("api-key", _serviceConfig.OpenAIKey);
        var response = await httpClient.PostAsync(_serviceConfig.OpenAIEndPoint, content);
        
        // parse the result
        var result = await response.Content.ReadAsStringAsync();
        var chatbotResponse = JsonUtility.FromJson<ChatResponse>(result);
        return chatbotResponse.choices[0].message.content;
    }

    public async Task<AudioDataStream> ConvertTextToSpeechAsync(string text)
    {
        var streamOutput = AudioOutputStream.CreatePullStream();
        var audioConfig = AudioConfig.FromStreamOutput(streamOutput);

        using var synthesizer = new SpeechSynthesizer(_speechConfig, audioConfig);
        synthesizer.VisemeReceived += HandleVisemeReceived;

        var ssmlRequest = BuildSsmlRequest(text);
        var result = await synthesizer.SpeakSsmlAsync(ssmlRequest);
        return await HandleSpeechResult(result);
    }

    private void HandleVisemeReceived(object sender, SpeechSynthesisVisemeEventArgs e)
    {
        var animation = e.Animation;
        var frames = JsonConvert.DeserializeObject<FrameData>(animation);
        _chatbot.QueueFrameData(frames);
    }

    private async Task<AudioDataStream> HandleSpeechResult(SpeechSynthesisResult result)
    {
        if (result.Reason == ResultReason.SynthesizingAudioCompleted)
        {
            return AudioDataStream.FromResult(result);
        }
        // do some error handling

        return null;
    }

    private string BuildSsmlRequest(string text)
    {
        return $@"
            <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xmlns:mstts='https://www.w3.org/2001/mstts' xml:lang='en-US'>
                <voice name='{_serviceConfig.Voice}'>
                    <mstts:viseme type='FacialExpression'/>
                    {text}
                </voice>
            </speak>";
    }
}
