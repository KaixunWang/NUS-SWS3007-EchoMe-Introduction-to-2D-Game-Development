using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;


// 这个是 leaderboard 的脚本，scores 中包含了排名，玩家名，星星数，时间
// 排序方式是先按照星星排，然后按照时间排

public class leaderboard : MonoBehaviour
{
    public string leaderboardKey = "globalhighscore";

    public struct Score
    {
        public int rank;
        public string player;
        public int star;
        public int time;
    }

    private List<Score> scores = new List<Score>(10);

    // 获取分数
    public List<Score> GetScores()
    {
        return scores;
    }

    public IEnumerator SubmitScoreRoutine(int star, int time)
    {
        // 确保LootLocker已初始化
        yield return StartCoroutine(LootLockerManager.Instance.EnsureInitialized());
        
        bool done = false;
        string playerID = PlayerPrefs.GetString("PlayerID");
        // 我们怎么记录分数，万位是3星星数，然后后四位是时间的秒数，这样我们可以根据相应的分数进行排序，分数越小越好。
        int scoreToUpload = (3-star) * 100 + time;
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
                    int scoreValue = item.score;
                    string playerName = item.player.name;
                    int rank = item.rank;
                    int star = 3 - (scoreValue / 1000);
                    int time = scoreValue % 1000;
                    
                    // 使用LootLockerManager获取更好的显示名称
                    string displayName = LootLockerManager.Instance.GetPlayerDisplayName(item.player.id, playerName);

                    // 添加到列表
                    scores.Add(new Score
                    {
                        rank = rank,
                        player = displayName,
                        star = star,
                        time = time
                    });
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
