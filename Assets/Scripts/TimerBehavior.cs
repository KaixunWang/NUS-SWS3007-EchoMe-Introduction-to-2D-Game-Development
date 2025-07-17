using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerBehavior : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    private float remainingTime = 10f;
    public GameObject player;
    private float elapsedTime = 0f;
    private bool isRecording = true;
    private int minutes, seconds;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(timerText != null, "Timer Text is not assigned in the inspector.");
        Debug.Assert(player != null, "Player GameObject is not assigned in the inspector.");
    }

    // Update is called once per frame
    void Update()
    {
        if (player.GetComponent<PlayerBehaviour>().getState())//isShadow
        {
            timerText.color = Color.red;
            minutes = Mathf.FloorToInt(remainingTime / 60);
            seconds = Mathf.FloorToInt(remainingTime % 60);
            if (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
            }
            else if (remainingTime <= 0)
            {
                remainingTime = 0;
            }
        }
        else
        {
            remainingTime = 10f;
            minutes = Mathf.FloorToInt(elapsedTime / 60);
            seconds = Mathf.FloorToInt(elapsedTime % 60);
            timerText.color = Color.white;
        }
        if (isRecording)
        {
            elapsedTime += Time.deltaTime;
        }
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public int GetElapsedTime()
    {
        return Mathf.FloorToInt(elapsedTime);
    }
    public float GetPreciseElapsedTime()
    {
        return elapsedTime;
    }
    public void SetTimer(bool set)
    {
        isRecording = set;
    }
}
