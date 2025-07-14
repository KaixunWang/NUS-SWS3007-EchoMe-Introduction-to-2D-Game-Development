using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeaconsystemScript : MonoBehaviour
{
    public GameObject playerBeacon; // 玩家信标
    public GameObject echoBeacon; // 回声信标
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(player != null, "Player GameObject is not assigned in BeaconsystemScript.");
        Debug.Assert(playerBeacon != null, "Player Beacon is not assigned in BeaconsystemScript.");
        Debug.Assert(echoBeacon != null, "Echo Beacon is not assigned in BeaconsystemScript.");
        var playerBehaviour = player.GetComponent<PlayerBehaviour>();
        var pbeaconBehaviour = playerBeacon.GetComponent<BeaconBehaviour>();
        var ebeaconBehaviour = echoBeacon.GetComponent<BeaconBehaviour>();
        pbeaconBehaviour.setSystem(true); // 设置玩家信标为系统信标
        ebeaconBehaviour.setSystem(true);
        ebeaconBehaviour.setcallbeacon(false); // 设置回声信标调用状态为false
        playerBehaviour.setBeaconpos(ebeaconBehaviour.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        var ebeaconBehaviour = echoBeacon.GetComponent<BeaconBehaviour>();
        var pbeaconBehaviour = playerBeacon.GetComponent<BeaconBehaviour>();
        var playerBehaviour = player.GetComponent<PlayerBehaviour>();
        ebeaconBehaviour.sc.sprite = pbeaconBehaviour.sc.sprite; // 设置回声信标的精灵与玩家信标一致
    }
}
