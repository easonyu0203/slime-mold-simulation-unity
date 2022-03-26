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
        [SerializeField] private int _agentCnt;
        [SerializeField] private float _agentMoveSpeed; // pixel/sec
        [SerializeField] private float _sensorDst;
        [SerializeField] private float _agentTurnSpeed;
    

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
            AgentManger.Singleton = this;
        }

        public void Init()
        {
            InitAgents();
            InitShader();
        }

        public void UpdateAgents() 
        {
            // setting
            _mainShader.SetFloat("moveSpeed", _agentMoveSpeed);
            _mainShader.SetFloat("sensorDst", _sensorDst);
            _mainShader.SetFloat("agentTurnSpeed", _agentTurnSpeed);
            // tell agentShader to run 
            _mainShader.Dispatch(0, _threadGroupX, 1, 1);
        }

        public void Release()
        {
            _agentsBuffer.Release();

        }

        private void InitAgents()
        {
            // random position, angle
            // TODO: let user select other init method
            _agents = new Agent[_agentCnt];
            for (int i = 0; i < _agentCnt; i++)
            {
                int radius = Screen.height / 2;
                int r = Random.Range(0, radius);
                float angle = Random.Range(0f, 2f * 3.14f);
                float x = r * (float)Math.Cos(angle);
                float y = r * (float)Math.Sin(angle);
                _agents[i].Position = new float2(
                    Screen.width / 2f + (int)(x), Screen.height / 2f + (int)(y)
                );
                _agents[i].Angle = angle + 3.14f;
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
            
            // init trail map
            _trailMap = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true
            };
            _mainShader.SetTexture(_agentUpdateKernel, "trailMap", _trailMap);
            // compute threadGroupX
            _threadGroupX = Mathf.CeilToInt(_agents.Length / 64f);
        }
    }
}