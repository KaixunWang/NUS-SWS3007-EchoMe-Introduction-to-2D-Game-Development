using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBindKey : MonoBehaviour
{
    InputManager inputManager;

    void Awake()
    {
        inputManager = InputManager.Instance;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void BindJumpToW()
    {
        inputManager.RebindJumptoW();
    }
    public void BindJumpToSpace()
    {
        inputManager.RebindJumptoSpace();
    }
}
