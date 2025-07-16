using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // 确保引入 TextMeshPro 命名空间
public class BindKeyText : MonoBehaviour
{
    public TMP_Text myTMP;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        InputManager inputManager = InputManager.Instance;
        if (KeyCode.W == inputManager.getJumpBinding())
        {
            myTMP.text = "Current:\nW";
        }
        else
        {
            myTMP.text = "Current:\nSpace";
        }

    }
}
