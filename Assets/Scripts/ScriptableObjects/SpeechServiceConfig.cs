using UnityEngine;

[CreateAssetMenu(fileName = "SpeechConfig", menuName = "SpeechService/Speech Service Config", order = 0)]
public class SpeechServiceConfig : ScriptableObject
{
    public string OpenAIEndPoint;
    public string Region = "westeurope";
    public string SpeechKey;
    public string OpenAIKey;

    public string Voice = "en-US-AndrewMultilingualNeural";
    [TextArea(1, 99)] public string SystemPrompt;
}