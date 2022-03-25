using System;
using UnityEngine;

namespace Simulation
{
    public class DisplaySim : MonoBehaviour, IRenTexUpdater
    {
        [SerializeField] private ComputeShader _mainShader;
        private int _simRenderKernel;
        private int _threadGroupX;
        private int _threadGroupY;
        
        // reference
        private AgentManger _agentManger; 

        private void Awake()
        {
            _threadGroupX = Mathf.CeilToInt(Screen.width / 8f);
            _threadGroupY = Mathf.CeilToInt(Screen.height / 8f);
            _simRenderKernel = _mainShader.FindKernel("SimRender");
        }

        private void Start()
        {
            _agentManger = AgentManger.Singleton;
        }

        public void RenTextUpdate(RenderTexture target)
        {
            _mainShader.SetTexture(_simRenderKernel, "trailMap", _agentManger.TrailMap);
            _mainShader.SetTexture(_simRenderKernel, "Result", target);
            _mainShader.Dispatch(_simRenderKernel, _threadGroupX, _threadGroupY, 1);
        }
    }
}