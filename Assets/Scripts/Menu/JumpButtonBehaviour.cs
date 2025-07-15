using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpButtonBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // InputManager.Instance.LoadBindings();
    }
    void OnEnable()
    {
        InputManager.Instance.LoadBindings();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
