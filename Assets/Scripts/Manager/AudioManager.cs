using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    
    [Header("音量控制")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    
    // Audio Mixer参数名称（需要在Mixer中暴露这些参数）
    private const string MASTER_VOLUME = "MasterVolume";
    private const string MUSIC_VOLUME = "MusicVolume";
    private const string SFX_VOLUME = "SFXVolume";
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolumeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        ApplyVolumeSettings();
    }
    
    // =============================================================================
    // 音量控制方法
    // =============================================================================
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        float db = VolumeToDecibel(masterVolume);
        audioMixer.SetFloat(MASTER_VOLUME, db);
        SaveVolumeSettings();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        float db = VolumeToDecibel(musicVolume);
        audioMixer.SetFloat(MUSIC_VOLUME, db);
        SaveVolumeSettings();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        float db = VolumeToDecibel(sfxVolume);
        audioMixer.SetFloat(SFX_VOLUME, db);
        SaveVolumeSettings();
    }
    
    // 将0-1的音量值转换为分贝值
    private float VolumeToDecibel(float volume)
    {
        return volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
    }
    
    // =============================================================================
    // 设置保存和加载
    // =============================================================================
    
    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }
    
    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }
    
    private void ApplyVolumeSettings()
    {
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }
    
    // Inspector中实时调整音量
    void OnValidate()
    {
        if (Application.isPlaying && audioMixer != null)
        {
            ApplyVolumeSettings();
        }
    }
    
    // =============================================================================
    // 快捷测试方法
    // =============================================================================
    
    void Update()
    {
        // 快捷键测试（可以删除）
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetMasterVolume(masterVolume - 0.1f);
            Debug.Log($"Master Volume: {masterVolume:F1}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetMasterVolume(masterVolume + 0.1f);
            Debug.Log($"Master Volume: {masterVolume:F1}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetMusicVolume(musicVolume - 0.1f);
            Debug.Log($"Music Volume: {musicVolume:F1}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetMusicVolume(musicVolume + 0.1f);
            Debug.Log($"Music Volume: {musicVolume:F1}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SetSFXVolume(sfxVolume - 0.1f);
            Debug.Log($"SFX Volume: {sfxVolume:F1}");
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SetSFXVolume(sfxVolume + 0.1f);
            Debug.Log($"SFX Volume: {sfxVolume:F1}");
        }
    }
}