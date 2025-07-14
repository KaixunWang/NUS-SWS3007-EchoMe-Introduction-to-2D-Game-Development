using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

public class GlobalClickSound : MonoBehaviour
{
    [Header("点击音效")]
    public AudioClip clickSound;
    public float volume = 0.5f;
    
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;  // 拖入你的Audio Mixer
    
    private AudioSource audioSource;
    
    void Awake()
    {
        // 确保只有一个实例
        if (FindObjectsOfType<GlobalClickSound>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        // 创建AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
        
        // 设置输出到SFX组
        if (audioMixer != null)
        {
            // 查找SFX组并分配给AudioSource
            var sfxGroup = audioMixer.FindMatchingGroups("SFX");
            if (sfxGroup.Length > 0)
            {
                audioSource.outputAudioMixerGroup = sfxGroup[0];
            }
        }
    }
    
    void Update()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            // 检查是否点击在UI上
            if (IsPointerOverUI())
            {
                PlayClickSound();
            }
        }
    }
    
    // 检查鼠标是否在UI上
    bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
    
    void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}