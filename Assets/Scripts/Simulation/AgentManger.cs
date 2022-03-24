using System;
using Unity.Mathematics;
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
        // Agent
        [SerializeField] private int _agentCnt;
        private Agent[] _agents;
        // Agent Shader
        [SerializeField] private ComputeShader _agentShader;
        private ComputeBuffer _agentsBuffer;
        private int _threadGroupX;
        

        private void Awake()
        {
            InitAgents();
            InitShader();
        }

        private void Update()
        {
            // tell agentShader to run
            _agentShader.Dispatch(0, _threadGroupX, 1, 1);
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
            // put agents to shader(GPU)
            _agentsBuffer = new ComputeBuffer(_agents.Length, 16);
            _agentsBuffer.SetData(_agents);
            _agentShader.SetBuffer(0, "Agents", _agentsBuffer);
            // compute threadGroupX
            _threadGroupX = Mathf.CeilToInt(_agents.Length / 64f);
        }
    }
}