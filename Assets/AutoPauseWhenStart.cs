using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPauseWhenStart : MonoBehaviour
{
    public PauseMenuManager pauseMenuManager;
    // Start is called before the first frame update
    void Start()
    {
        pauseMenuManager.PauseGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
