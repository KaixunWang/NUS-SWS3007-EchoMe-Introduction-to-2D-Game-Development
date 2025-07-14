using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneState
{
    public Vector3 playerPosition;
    public List<bool> switchStates = new List<bool>();
    public List<float> switchRemainingTimes = new List<float>();
    public List<bool> pressurePlateStates = new List<bool>();
    public List<bool> doorStates = new List<bool>();
    public List<Vector3> boxPositions = new List<Vector3>();
    public List<Vector3> boxSpeed = new List<Vector3>();
    public List<PlatformState> lifts = new List<PlatformState>();
    public List<SpikeState> spikes = new List<SpikeState>();
}
