using System;
using UnityEngine;

namespace Simulation
{
    public class SimRunner : MonoBehaviour, IRenTexUpdater
    {
        [SerializeField] private ComputeShader _mainShader;
        private int _simRenderKernel;
        private int _threadGroupX;
        private int _threadGroupY;
         
        // reference 
        private AgentManger _agentManger;
        
        [Header("Setting")]
        [SerializeField] private float _diffusionRate;
        [SerializeField] private float _evaporate;

        public void RunSim()
        {
            _mainShader.SetBool("isRunning", true);
        }

        public void PauseSim()
        {
            _mainShader.SetBool("isRunning", false);
        }
        
        private void Awake()
        {
            _threadGroupX = Mathf.CeilToInt(Screen.width / 8f);
            _threadGroupY = Mathf.CeilToInt(Screen.height / 8f);
            _simRenderKernel = _mainShader.FindKernel("SimRender");
        }

        private void Start()
        {
            _agentManger = AgentManger.Singleton;
            InitSim();
        }

        public void RenTextUpdate(RenderTexture target)
        {
            UpdateSim();
            _mainShader.SetTexture(_simRenderKernel, "trailMap", _agentManger.TrailMap);
            _mainShader.SetTexture(_simRenderKernel, "Result", target);
            _mainShader.Dispatch(_simRenderKernel, _threadGroupX, _threadGroupY, 1);
        }

        private void OnDestroy()
        {
            EndSim();
        }

        private void InitSim()
        {
            InitGlobShaderVar();
            _agentManger.Init();
        }

        private void InitGlobShaderVar()
        {

        }

        private void UpdateSim()
        {
            UpdateGlobShaderVar();
            _agentManger.UpdateAgents();
        }

        private void UpdateGlobShaderVar()
        {
            // delta time
            _mainShader.SetFloat("deltaTime", Time.deltaTime);
            // setting
            _mainShader.SetFloat("diffusionRate", _diffusionRate);
            _mainShader.SetFloat("evaporate", _evaporate);
            // screen width & height
            _mainShader.SetInt("width", Screen.width);
            _mainShader.SetInt("height", Screen.height);
        }

        private void EndSim()
        {
            _agentManger.Release();
        }
    }
}