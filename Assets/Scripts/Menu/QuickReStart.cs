using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickReStart : MonoBehaviour
{
    public PauseMenuManager pauseMenuManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.Instance.controls.GamePlay.Restart != null &&
            InputManager.Instance.controls.GamePlay.Restart.WasPressedThisFrame())
        {
            Debug.Log("Tab key pressed, restarting game.");
            pauseMenuManager.RestartGame();
        }
    }
}
