using System.Collections;
using System.Collections.Generic;
using Simulation;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SimRunner))]
class SimRunnerEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if(GUILayout.Button("Run"))
            ((SimRunner) (target)).RunSim();
        if(GUILayout.Button("Pause"))
            ((SimRunner) (target)).PauseSim();
    }
}
