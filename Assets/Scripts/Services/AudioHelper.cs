using System;
using System.IO;
using Microsoft.CognitiveServices.Speech;
using UnityEngine;

public static class AudioHelper
{
    public static AudioClip ConvertAudioStreamToAudioClip(AudioDataStream audioStream)
    {
        int sampleRate = 16000;
        int channels = 1;

        using var memoryStream = new MemoryStream();
        byte[] buffer = new byte[320];
        uint bytesRead;
        
        while ((bytesRead = audioStream.ReadData(buffer)) > 0)
            memoryStream.Write(buffer, 0, (int)bytesRead);
        
        byte[] audioData = memoryStream.ToArray();
        const int headerSize = 44; // standard WAV header size
        int pcmDataLength = audioData.Length - headerSize;
        int sampleCount = pcmDataLength / 2; // 16-bit samples

        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(audioData, headerSize + i * 2);
            samples[i] = sample / 32768f;
        }
        
        var audioClip = AudioClip.Create("clip", sampleCount, channels, sampleRate, false);
        audioClip.SetData(samples, 0);
        
        return audioClip;
    }
}