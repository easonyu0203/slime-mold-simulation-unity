using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Simulation
{
    public struct Agent
    {
        public float2 Position;
        public float Angle;
        public float Dummy1;
    }

    public class AgentManger : MonoBehaviour
    {
        // setting
        [SerializeField] private float _agentMoveSpeed; // pixel/sec
        [SerializeField] private int _agentCnt;
        [SerializeField] private float _trailDecaySpeed;
        [SerializeField] private float _diffusionSpeed;
        [SerializeField] private float _agentTurnSpeed;
        [SerializeField] private int _sensorSize;
        [SerializeField] private float _sensorAngleOffset;
        [SerializeField] private float _sensorOffsetDst;

        // Agents
        private Agent[] _agents;
        private RenderTexture _trailMap;

        // Agent Shader
        [SerializeField] private ComputeShader _mainShader;
        private ComputeBuffer _agentsBuffer;
        private int _threadGroupX;
        private int _agentUpdateKernel;
        
        // Singleton    
        public static AgentManger Singleton;

        // Property
        public RenderTexture TrailMap => _trailMap;

        private void Awake()
        {
            InitAgents();
            InitShader();
            AgentManger.Singleton = this;
        }

        private void Update()
        {       
            // update agentShader variable
            _mainShader.SetTexture(_agentUpdateKernel, "trailMap", _trailMap);
            _mainShader.SetFloat("deltaTime", Time.deltaTime);
            // tell agentShader to run 
            _mainShader.Dispatch(0, _threadGroupX, 1, 1);
        }

        private void InitAgents()
        {
            // random position, angle
            // TODO: let user select other init method
            _agents = new Agent[_agentCnt];
            for (int i = 0; i < _agentCnt; i++)
            {
                _agents[i].Position = new float2(
                    Random.Range(1, Screen.width - 1),
                    Random.Range(1, Screen.height - 1)
                );
                _agents[i].Angle = Random.Range(0f, 360f);
            }
        }

        private void InitShader()
        {
            _agentUpdateKernel = _mainShader.FindKernel("AgentUpdate");
            // put agents to shader(GPU)
            _agentsBuffer = new ComputeBuffer(_agents.Length, 16);
            _agentsBuffer.SetData(_agents);
            _mainShader.SetBuffer(_agentUpdateKernel, "agents", _agentsBuffer);
            // agentCnt
            _mainShader.SetInt("agentCnt",_agentCnt);
            // setting
            _mainShader.SetFloat("moveSpeed", _agentMoveSpeed);
            _mainShader.SetFloat("agentTurnSpeed", _agentTurnSpeed);
            _mainShader.SetFloat("sensorOffsetDst", _sensorOffsetDst);
            _mainShader.SetFloat("sensorAngleOffset", _sensorAngleOffset / 360f);
            _mainShader.SetInt("sensorSize", _sensorSize);
            _mainShader.SetFloat("trailDecaySpeed", _trailDecaySpeed);
            _mainShader.SetFloat("diffusedSpeed", _diffusionSpeed);
            // screen width & height
            _mainShader.SetInt("width", Screen.width);
            _mainShader.SetInt("height", Screen.height);
            // init trail map
            _trailMap = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true
            };
            // compute threadGroupX
            _threadGroupX = Mathf.CeilToInt(_agents.Length / 64f);
        }

        private void OnDestroy()
        {
            _agentsBuffer.Release();
        }
    }
}