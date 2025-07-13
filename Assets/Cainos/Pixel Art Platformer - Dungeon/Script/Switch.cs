using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cainos.LucidEditor;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

namespace Cainos.PixelArtPlatformer_Dungeon
{
    public class Switch : MonoBehaviour
    {
        [FoldoutGroup("Reference")] public Door target;
        [FoldoutGroup("Reference")] public Bundos.MovingPlatforms.PlatformController targetPlatform;
        [FoldoutGroup("Reference")] public SpikeBehavioudr targetSpike;
        [Space]
        [FoldoutGroup("Reference")] public SpriteRenderer spriteRenderer;
        [FoldoutGroup("Reference")] public Sprite spriteOn;
        [FoldoutGroup("Reference")] public Sprite spriteOff;

        [FoldoutGroup("Settings")] public float autoCloseDelay = 1.5f; // 自动关闭延迟时间
        private float remainingTime = 0f; // 保留的时间
        private Animator Animator
        {
            get
            {
                if (animator == null) animator = GetComponent<Animator>();
                return animator;
            }
        }
        private Animator animator;
        private Coroutine autoCloseCoroutine; // 自动关闭协程

        private void Start()
        {
            Animator.SetBool("IsOn", isOn);
            IsOn = isOn;
        }

        public void TriggerDoor()
        {
            if (target != null && target.tag == "door")
            {
                if (IsOn)
                {
                    Debug.Log("Switch: Open the door");
                    target.SetDoor(true);
                }
                else
                {
                    Debug.Log("Switch: Close the door");
                    target.SetDoor(false);
                }
            }
            if (target != null && target.tag == "gate")
            {
                if (IsOn)
                {
                    Debug.Log("Switch: Open the gate");
                    target.SetDoor(true); // 确保门关闭状态
                    target.SetGate(true);
                }
                else
                {
                    Debug.Log("Switch: Close the gate");
                    target.SetDoor(false); // 确保门关闭状态
                    target.SetGate(false);
                }
            }
        }
        void Update()
        {
            Animator.SetBool("IsOn", isOn);
            TriggerDoor();
        }

        // 自动关闭协程
        private IEnumerator AutoCloseCoroutine()
        {
            // yield return new WaitForSeconds(autoCloseDelay);
            // float countdown = autoCloseDelay;
            while (remainingTime > 0f)
            {
                // remainingTime = countdown; // 更新剩余时间
                remainingTime -= Time.deltaTime; // 减少剩余时间
                yield return null;
            }
            // remainingTime = autoCloseDelay;
            IsOn = false;
            autoCloseCoroutine = null;
        }

        [FoldoutGroup("Runtime"), ShowInInspector]
        public bool IsOn
        {
            get { return isOn; }
            set
            {
                bool previousState = isOn;
                isOn = value;

#if UNITY_EDITOR
                if (Application.isPlaying == false)
                {
                    EditorUtility.SetDirty(this);
                    EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }
#endif

                if (target) target.IsOpened = isOn;

                if (Application.isPlaying)
                {
                    Animator.SetBool("IsOn", isOn);

                    // 当开关从关闭变为开启时，启动自动关闭协程
                    if (!previousState && isOn && target != null)
                    {
                        // 停止之前的自动关闭协程（如果存在）
                        if (autoCloseCoroutine != null)
                        {
                            StopCoroutine(autoCloseCoroutine);
                        }

                        // 启动新的自动关闭协程
                        remainingTime = autoCloseDelay; // 重置剩余时间
                        autoCloseCoroutine = StartCoroutine(AutoCloseCoroutine());
                    }
                    // 当开关关闭时，停止自动关闭协程
                    else if (previousState && !isOn)
                    {
                        remainingTime = 0f; // 重置剩余时间
                        if (autoCloseCoroutine != null)
                        {
                            StopCoroutine(autoCloseCoroutine);
                            autoCloseCoroutine = null;
                        }
                    }
                }
                else
                {
                    if (spriteRenderer) spriteRenderer.sprite = isOn ? spriteOn : spriteOff;
                }
            }
        }
        [SerializeField, HideInInspector]
        private bool isOn;

        [FoldoutGroup("Runtime"), HorizontalGroup("Runtime/Button"), Button("Turn On")]
        public void TurnOn()
        {
            IsOn = true;
        }

        [FoldoutGroup("Runtime"), HorizontalGroup("Runtime/Button"), Button("Turn Off")]
        public void TurnOff()
        {
            IsOn = false;
        }
        public float GetRemainingTime()
        {
            return remainingTime;
        }
        public void SetRemainingTime(float time)
        {
            remainingTime = time;
        }
        public float GetAutoCloseDelay()
        {
            return autoCloseDelay;
        }
    }
}
