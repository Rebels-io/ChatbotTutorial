using System.Collections.Generic;
using Microsoft.CognitiveServices.Speech;
using UnityEngine;

public class ChatbotAnimator : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField] private AudioSource _audioSource;

    private List<List<float>> _blendshapeQueue = new();
    private AudioDataStream _audioDataStream;
    
    private bool _newAudioLoaded;
    private int _currentFrame = 0;
    private float _timer = 0f;
    private bool _isAnimationQueued;
    
    private void OnEnable()
    {
        _isAnimationQueued = false;
    }
    
    private void Update()
    {
        if (_newAudioLoaded)
        {
            _newAudioLoaded = false;
            _audioSource.clip = AudioHelper.ConvertAudioStreamToAudioClip(_audioDataStream);
            _audioSource.Play();
        }
    }
    
    void LateUpdate()
    {
        if (_blendshapeQueue.Count < 1 || !_isAnimationQueued)
            return;

        if (ShouldUpdateCurrentFrame())
            UpdateBlendshapes();
    }
    
    private bool ShouldUpdateCurrentFrame()
    {
        if (_currentFrame >= _blendshapeQueue.Count - 1)
        {
            ResetAnimation();
            return false;
        }

        _timer += Time.deltaTime;
        _currentFrame = Mathf.FloorToInt(_timer * 60);
        return true;
    }
    
    private void UpdateBlendshapes()
    {
        for (int weightIndex = 0; weightIndex < _blendshapeQueue[_currentFrame].Count; weightIndex++)
        {
            var blendShapeName = BlendshapeMapping.AzureBlendshapes[weightIndex];
            var index = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(blendShapeName);

            if (index >= 0)
            {
                var setWeight = _blendshapeQueue[_currentFrame][weightIndex];
                _skinnedMeshRenderer.SetBlendShapeWeight(index, setWeight);
            }
        }
    }
    
    private void ResetAnimation()
    {
        _currentFrame = 0;
        _timer = 0;
        _isAnimationQueued = false;
        _blendshapeQueue.Clear();
        
        for (int i = 0; i < _skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
            _skinnedMeshRenderer.SetBlendShapeWeight(i, 0f);
    }

    public void PlaySpeechAudioStream(AudioDataStream audioDataStream)
    {
        _audioDataStream = audioDataStream;
        _newAudioLoaded = true;
    }

    public void QueueFrames(List<List<float>> newFrames)
    {
        _blendshapeQueue.AddRange(newFrames);
        _isAnimationQueued = true;
    }
}