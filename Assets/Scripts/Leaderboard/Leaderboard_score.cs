using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;

public class leaderboard_score : MonoBehaviour
{
    public string leaderboardKey = "passlevelnumber";

    private List<Player_LevelPassed> scores = new List<Player_LevelPassed>(10);

    // 获取分数
    public List<Player_LevelPassed> GetScores()
    {
        return scores;
    }

    public IEnumerator SubmitScoreRoutine(int scoreToUpload)
    {
        // 确保LootLocker已初始化
        yield return StartCoroutine(LootLockerManager.Instance.EnsureInitialized());
        
        bool done = false;
        string playerID = PlayerPrefs.GetString("PlayerID");
        LootLockerSDKManager.SubmitScore(playerID, scoreToUpload, leaderboardKey, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully uploaded score");
                done = true;
            }
            else
            {
                Debug.Log("Failed" + response.errorData.ToString());
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
    }
    
    // 通过这个函数，获取相应的分数，并保存在scores中
    public IEnumerator FetchTopHighScoresRoutine()
    {
        // 确保LootLocker已初始化
        yield return StartCoroutine(LootLockerManager.Instance.EnsureInitialized());
        
        bool done = false;

        LootLockerSDKManager.GetScoreList(leaderboardKey, 100, 0, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully fetched top scores");
                done = true;

                // 清空旧数据
                scores.Clear();

                // 遍历获取前十项并解析
                foreach (var item in response.items)
                {
                    int player_id = item.player.id;
                    int level_passed = item.score;
                    string player_name = item.player.name; // 获取玩家名字
                    
                    // 使用LootLockerManager获取更好的显示名称
                    string displayName = LootLockerManager.Instance.GetPlayerDisplayName(player_id, player_name);

                    // 添加到列表
                    scores.Add(new Player_LevelPassed(player_id, level_passed, displayName));
                }

                Debug.Log($"Fetched and saved {scores.Count} scores.");
            }
            else
            {
                Debug.Log("Failed: " + response.errorData.ToString());
                done = true;
            }
        });

        yield return new WaitWhile(() => done == false);
    }
}

public class Player_LevelPassed{
    public int player_id;
    public int level_passed;
    public string player_name; // 添加玩家名字字段
    
    public Player_LevelPassed(int playerId, int levelPassed, string playerName = ""){
        this.player_id = playerId;
        this.level_passed = levelPassed;
        this.player_name = playerName;
    }
}
