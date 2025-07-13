using UnityEngine;
using Cainos.LucidEditor;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


namespace Cainos.PixelArtPlatformer_Dungeon
{
    public class Door : MonoBehaviour
    {
        [FoldoutGroup("Reference")] public SpriteRenderer spriteRenderer;
        [FoldoutGroup("Reference")] public Sprite spriteOpened;
        [FoldoutGroup("Reference")] public Sprite spriteClosed;

        public AudioSource openAudioSource;
        public AudioSource closeAudioSource;

        public bool Control = true;
        public void SetControl(bool state)
        {
            Control = state;
        }

        private Animator Animator
        {
            get
            {
                if (animator == null ) animator = GetComponent<Animator>();
                return animator;
            }
        }
        private Animator animator;


        [FoldoutGroup("Runtime"), ShowInInspector]
        public bool IsOpened
        {
            get { return isOpened; }
            set
            {
                isOpened = value;

                #if UNITY_EDITOR
                if (Application.isPlaying == false)
                {
                    EditorUtility.SetDirty(this);
                    EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }
                #endif


                if (Application.isPlaying)
                {
                    Animator.SetBool("IsOpened", isOpened);
                }
                else
                {
                    if(spriteRenderer) spriteRenderer.sprite = isOpened ? spriteOpened : spriteClosed;
                }
            }
        }
        [SerializeField,HideInInspector]
        private bool isOpened;

        public void SetDoor(bool state)
        {
            if (Control == false)
            {
                return;
            }

            IsOpened = state;
            if (state)
            {
                if (openAudioSource != null)
                {
                    openAudioSource.Play();
                }
            }
            else
            {
                if (closeAudioSource != null)
                {
                    closeAudioSource.Play();
                }
            }
        }

        public void SetGate(bool state)
        {
            if (Control == false)
            {
                return;
            }
            Collider2D targetCollider = GetComponent<Collider2D>();
            if (targetCollider != null)
                targetCollider.isTrigger = state; // 设置为触发器或非触发器
        }

        private void Start()
        {
            Animator.Play(isOpened ? "Opened" : "Closed");
            IsOpened = isOpened;
        }


        [FoldoutGroup("Runtime"), HorizontalGroup("Runtime/Button"), Button("Open")]
        public void Open()
        {
            IsOpened = true;
        }

        [FoldoutGroup("Runtime"), HorizontalGroup("Runtime/Button"), Button("Close")]
        public void Close()
        {
            IsOpened = false;
        }
    }
}
