using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Bundos.MovingPlatforms;
[System.Serializable]
public class SpikeState
{
    public bool singleMove; // 是否单次移动
    public int cntMove; // 移动计数
    public bool inMove; // 是否开启移动
    public Vector3 Destination; // 目标位置
    public Vector3 StartPosition; // 起始位置
    public Vector3 pos; // 当前位置信息

}
