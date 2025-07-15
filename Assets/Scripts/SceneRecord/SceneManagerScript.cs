using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    private SceneState currentState;
    private bool isRecording = false;
    public GameObject player;
    public GameObject clock;
    public GameObject win;
    public GameObject lose;
    public GameObject coinSystem; // Reference to the CoinSystemScript
    public List<GameObject> switches; // List of switch GameObjects
    public List<GameObject> pressurePlates; // List of pressure plate GameObjects
    public List<GameObject> doors; // List of door GameObjects
    public List<GameObject> boxes; // List of box GameObjects
    public List<GameObject> lifts; // List of lift GameObjects
    public List<GameObject> spikes; // List of spike GameObjects
    private int score = 0;
    public int levelGoodTime = 60;
    // Start is called before the first frame update

    public leaderboard leaderboard;
    void Start()
    {
        
        Debug.Assert(player != null, "Player GameObject is not assigned in the SceneManagerScript.");
        Debug.Assert(clock != null, "Clock GameObject is not assigned in the SceneManagerScript.");
        Debug.Assert(win != null, "Win GameObject is not assigned in the SceneManagerScript.");
        Debug.Assert(lose != null, "Lose GameObject is not assigned in the SceneManagerScript.");
        Debug.Assert(coinSystem != null, "CoinSystemScript GameObject is not assigned in the SceneManagerScript.");
        win.SetActive(false);
        lose.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        var playerBehaviour = player.GetComponent<PlayerBehaviour>();
        if (playerBehaviour.getState() && !isRecording)
        {
            SaveState();
        }
        if (!(playerBehaviour.getState()) && isRecording)
        {
            LoadState();
        }
        if (playerBehaviour.IsWin())
        {
            GameObject.Find("PauseObject").GetComponent<PauseMenuManager>().SetEndState(); // 设置游戏结束状态
            win.SetActive(true);
            string message = "";
            score = 1;

            if (coinSystem.GetComponent<CoinSystemScript>().GetCoinCount() == 3)
            {
                score++;
                message += "Collect Coins:3/3\n";
            } else {
                message += "Collect Coins:" + coinSystem.GetComponent<CoinSystemScript>().GetCoinCount() + "/3\n";
            }
            if (clock.GetComponent<TimerBehavior>().GetElapsedTime() <= levelGoodTime)
            {
                score++;
                //如果当前scene是Level_4，则触发成就PassLevel4
                if (SceneManager.GetActiveScene().name == "Level_4" && clock.GetComponent<TimerBehavior>().GetElapsedTime() <= 20)
                {
                    AchievementManager.Instance.UnlockAchievement("PassLevel4_Under20");
                }
                message += "Time: " + clock.GetComponent<TimerBehavior>().GetElapsedTime() + "/" + levelGoodTime + "s\n";
            }
            else
            {
                message += "Time: " + clock.GetComponent<TimerBehavior>().GetElapsedTime() + "/" + levelGoodTime + "s\n";
            }
            win.GetComponent<WinScript>().SetStars(score);
            win.GetComponent<WinScript>().ShowWinPanel(message);
            clock.GetComponent<TimerBehavior>().SetTimer(false);
            // clock.SetActive(false);
            // player.SetActive(false);
            Debug.Log("You Win!");

            // ----------- 新增：保存星星数到PlayerPrefs -----------
            // 获取当前关卡编号
            int currentLevelIndex = 0;
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName.StartsWith("Level_"))
            {
                int.TryParse(sceneName.Substring("Level_".Length), out currentLevelIndex);
            }
            // 只保存更高的星星数
            int oldStars = PlayerPrefs.GetInt($"Level_{currentLevelIndex}_Stars", 0);
            if (score > oldStars)
            {
                PlayerPrefs.SetInt($"Level_{currentLevelIndex}_Stars", score);
                PlayerPrefs.Save();
                Debug.Log($"保存关卡{currentLevelIndex}星星数: {score}");
            }

            if (leaderboard != null) leaderboard.SubmitScoreRoutine(score, clock.GetComponent<TimerBehavior>().GetElapsedTime());
            // ---------------------------------------------
        }else if (playerBehaviour.IsLose())
        {
            GameObject.Find("PauseObject").GetComponent<PauseMenuManager>().SetEndState(); // 设置游戏结束状态
            lose.SetActive(true);
            clock.GetComponent<TimerBehavior>().SetTimer(false);
            // clock.SetActive(false);
            // player.SetActive(false);
            Debug.Log("You Lose!");
        }
    }

    void SaveState()
    {
        currentState = new SceneState
        {
            playerPosition = player.transform.position
            // switchState = switchState, // Assuming you have a switch state
            // pressurePlateState = pressurePlateState, // Assuming you have a pressure plate state
            // doorState = doorState, // Assuming you have a door state
            // boxPosition = boxPosition // Assuming you have a box position
        };
        foreach (var switchObj in switches)
        {
            var switchComponent = switchObj.GetComponent<Cainos.PixelArtPlatformer_Dungeon.Switch>();
            if (switchComponent != null)
            {
                currentState.switchStates.Add(switchComponent.IsOn);
                currentState.switchRemainingTimes.Add(switchComponent.GetRemainingTime());
                Debug.Log($"Switch {switchObj.name} state: {switchComponent.IsOn}");
            }
        }
        foreach (var plateObj in pressurePlates)
        {
            var plateComponent = plateObj.GetComponent<BoardBehavior>();
            if (plateComponent != null)
            {
                currentState.pressurePlateStates.Add(plateComponent.GetBoardState());
                Debug.Log($"Pressure Plate {plateObj.name} state: {plateComponent.GetBoardState()}");
            }
        }
        foreach (var doorObj in doors)
        {
            var doorComponent = doorObj.GetComponent<Cainos.PixelArtPlatformer_Dungeon.Door>();
            if (doorComponent != null)
            {
                currentState.doorStates.Add(doorComponent.IsOpened);
                Debug.Log($"Door {doorObj.name} state: {doorComponent.IsOpened}");
            }
        }
        foreach (var boxObj in boxes)
        {
            currentState.boxPositions.Add(boxObj.transform.position);
            currentState.boxSpeed.Add(boxObj.GetComponent<Rigidbody2D>().velocity); // Assuming you want to save the speed of the box
        }
        // Debug.Log("lifts.Count: " + lifts.Count);
        foreach (var liftObj in lifts)
        {
            var platformController = liftObj.GetComponent<Bundos.MovingPlatforms.PlatformController>();
            if (platformController != null)
            {
                currentState.lifts.Add(platformController.GetStatus());
                Debug.Log("Lift " + liftObj.name + " state saved.");
            }
            else
            {
                Debug.LogWarning("Lift " + liftObj.name + " does not have a PlatformController component.");
            }
        }
        foreach (var spikeObj in spikes)
        {
            var spikeBehaviour = spikeObj.GetComponent<SpikeBehaviour>();
            if (spikeBehaviour != null)
            {
                currentState.spikes.Add(spikeBehaviour.getState());
                Debug.Log("Spike " + spikeObj.name + " state saved.");
            }
            else
            {
                Debug.LogWarning("Spike " + spikeObj.name + " does not have a SpikeBehaviour component.");
            }
        }
        isRecording = true;
        Debug.Log("Scene state saved");
    }
    void LoadState()
    {
        if (currentState != null)
        {
            player.transform.position = currentState.playerPosition;
            for (int i = 0; i < switches.Count && i < currentState.switchStates.Count; i++)
            {
                var switchComponent = switches[i].GetComponent<Cainos.PixelArtPlatformer_Dungeon.Switch>();
                if (switchComponent != null)
                {
                    if (currentState.switchStates[i])
                    {
                        switchComponent.TurnOn();
                        switchComponent.SetRemainingTime(currentState.switchRemainingTimes[i]);
                    }
                    else
                    {
                        switchComponent.TurnOff();
                        switchComponent.SetRemainingTime(0f); // Reset remaining time if switch is off
                    }
                    // else
                    // {
                    //     switchComponent.SetRemainingTime(0f); // Reset remaining time if switch is off
                    // }
                }
            }
            for (int i = 0; i < pressurePlates.Count && i < currentState.pressurePlateStates.Count; i++)
            {
                var plateComponent = pressurePlates[i].GetComponent<BoardBehavior>();
                if (plateComponent != null)
                {
                    plateComponent.SetBoardState(currentState.pressurePlateStates[i]);
                }
            }
            for (int i = 0; i < doors.Count && i < currentState.doorStates.Count; i++)
            {
                var doorComponent = doors[i].GetComponent<Cainos.PixelArtPlatformer_Dungeon.Door>();
                if (doorComponent != null)
                {
                    doorComponent.IsOpened = currentState.doorStates[i];
                    if (doors[i].tag == "gate") doors[i].GetComponent<Collider2D>().isTrigger = currentState.doorStates[i];
                }
            }
            for (int i = 0; i < boxes.Count && i < currentState.boxPositions.Count; i++)
            {
                boxes[i].transform.position = currentState.boxPositions[i];
                boxes[i].GetComponent<Rigidbody2D>().velocity = currentState.boxSpeed[i]; // Restore box speed
                // var boxComponent = boxes[i].GetComponent<BoxesBehavior>();
                // if (boxComponent != null)
                // {
                //     boxComponent.transform.position = currentState.boxPositions[i];
                // }
            }
            Debug.Log("lifts.Count: " + lifts.Count);
            Debug.Log("currentState.lifts.Count: " + currentState.lifts.Count);
            for (int i = 0; i < lifts.Count && i < currentState.lifts.Count; i++)
            {
                var platformController = lifts[i].GetComponent<Bundos.MovingPlatforms.PlatformController>();
                if (platformController != null)
                {
                    platformController.SetStatus(currentState.lifts[i]);
                    Debug.Log("Lift " + lifts[i].name + " state loaded.");
                }
            }
            for (int i = 0; i < spikes.Count && i < currentState.spikes.Count; i++)
            {
                var spikeBehaviour = spikes[i].GetComponent<SpikeBehaviour>();
                if (spikeBehaviour != null)
                {
                    spikeBehaviour.setState(currentState.spikes[i]);
                    Debug.Log("Spike " + spikes[i].name + " state loaded.");
                }
                else
                {
                    Debug.LogWarning("Spike " + spikes[i].name + " does not have a SpikeBehaviour component.");
                }
            }
            // Restore other states as needed
            isRecording = false;
            Debug.Log("Scene state loaded");
        }
        else
        {
            Debug.LogWarning("No saved state to load");
        }
    }
    public int GetScore()
    {
        return score;
    }
}
