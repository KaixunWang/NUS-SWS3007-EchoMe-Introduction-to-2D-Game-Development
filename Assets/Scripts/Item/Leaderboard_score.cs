using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;

public class leaderboard_score : MonoBehaviour
{
    public string leaderboardKey = "";

    private List<int> scores = new List<int>(10);

    // 获取分数
    public List<int> GetScores()
    {
        return scores;
    }

    public IEnumerator SubmitScoreRoutine(int scoreToUpload)
    {
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
        bool done = false;

        LootLockerSDKManager.GetScoreList(leaderboardKey, 10, 0, (response) =>
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

                    // 添加到列表
                    scores.Add(scoreValue);
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
