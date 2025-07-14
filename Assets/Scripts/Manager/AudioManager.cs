using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    
    [Header("音量控制")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("BGM Source")]
    public AudioSource BGM;
    
    
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
            
            // 添加场景切换监听器
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 新增：场景加载完成时的回调
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"场景已加载: {scene.name}");
        PlayBGMForCurrentScene();
    }
    
    // 新增：销毁时移除监听器
    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    void Start()
    {
        ApplyVolumeSettings();
        PlayBGMForCurrentScene();
    }
    
    // 新增：为当前场景播放BGM的方法
    public void PlayBGMForCurrentScene()
    {
        // 检查BGM是否已赋值
        if (BGM == null)
        {
            Debug.LogError("AudioManager: BGM AudioSource 未赋值！请在Inspector中设置BGM变量。");
            return;
        }
        
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log("currentScene: " + currentScene);
        
        // 停止当前播放的音乐
        BGM.Stop();
        
        if(currentScene == "Menu"){
            BGM.clip = LoadAudioClip("Menu_BGM");
            BGM.loop = true;
            BGM.Play();
        }else if(currentScene == "LevelSelectScene"){
            BGM.clip = LoadAudioClip("LevelSel_BGM");
            BGM.loop = true;
            BGM.Play();
        }else if(currentScene == "BeatTheGameScene"){
            BGM.clip = LoadAudioClip("BeatTheGame_BGM");
            BGM.loop = true;
            BGM.Play();
        }else if(currentScene.StartsWith("Level_")){//是Level_x的格式
            BGM.clip = LoadAudioClip("Level_BGM");
            BGM.loop = true;
            BGM.Play();
        }
        
        Debug.Log($"切换到场景 {currentScene}，播放BGM: {BGM.clip?.name ?? "null"}");
    }
    
    // 新增：加载音频文件的方法
    private AudioClip LoadAudioClip(string clipName)
    {
        // 首先尝试从Resources加载
        AudioClip clip = Resources.Load<AudioClip>($"Audio/{clipName}");
        if (clip != null)
        {
            return clip;
        }
        
        // 如果Resources中没有，尝试从Assets/Audio加载（仅在编辑器中）
        #if UNITY_EDITOR
        string assetPath = $"Assets/Audio/{clipName}.mp3";
        clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        if (clip != null)
        {
            Debug.Log($"从 {assetPath} 加载音频文件成功");
            return clip;
        }
        
        // 尝试.wav格式
        assetPath = $"Assets/Audio/{clipName}.wav";
        clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        if (clip != null)
        {
            Debug.Log($"从 {assetPath} 加载音频文件成功");
            return clip;
        }
        #endif
        
        Debug.LogError($"无法找到音频文件: {clipName}");
        return null;
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
        // 检查是否获胜（只在关卡场景中检查）
        if(SceneManager.GetActiveScene().name.StartsWith("Level_") && FindObjectOfType<WinScript>() != null){
            // 避免重复播放获胜音乐
            if (BGM.clip == null || !BGM.clip.name.Contains("Win_BGM"))
            {
                BGM.Stop();
                BGM.clip = LoadAudioClip("Win_BGM");
                BGM.loop = false;
                BGM.Play();
                Debug.Log("播放获胜音乐");
            }
        }
    }
}