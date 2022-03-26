using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Simulation
{
    /// <summary>
    /// render texture to screen, this class expose a RenderTexture let other class to set it
    /// and this class will render it out
    /// </summary>
    public class SimRenderer : MonoBehaviour
    {
        // target texture to display
        private RenderTexture _targetTexture;
        // all class that want to change target texture
        private IRenTexUpdater[] _targetTexUpdaters;

        private void Awake()
        {
            _targetTexUpdaters = GetComponents<IRenTexUpdater>();
        }
        
        

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            // make sure we have a current DisplayedTexture
            MakeSureTargetTexture();
            
            // Update TargetTexture
            foreach (var texUpdater in _targetTexUpdaters)
            {
                texUpdater.RenTextUpdate(_targetTexture);
            }
            
            // Display targetTexture
            Graphics.Blit(_targetTexture, dest);
        }

        /// <summary>
        /// make sure DisplayedTexture is good to use
        /// </summary>
        private void MakeSureTargetTexture()
        {
            // check if need to create new RenderTexture
            if (_targetTexture != null && _targetTexture.width == Screen.width &&
                _targetTexture.height == Screen.height) return;
            
            // Release render texture if we already have one
            if (_targetTexture != null)
                _targetTexture.Release();
            
            // Create new RenderTexture
            _targetTexture = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true
            };
            _targetTexture.Create();
        }
    }
}




