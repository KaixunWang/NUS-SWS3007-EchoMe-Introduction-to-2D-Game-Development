using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionPanelVolumeController : MonoBehaviour
{
    [Header("音量滑动条")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    
    [Header("音量显示文本（可选）")]
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    
    void Start()
    {
        InitializeSliders();
    }
    
    void OnEnable()
    {
        // 每次打开OptionPanel时更新滑动条值
        UpdateSlidersFromAudioManager();
    }
    
    private void InitializeSliders()
    {
        // 检查AudioManager是否存在
        if (AudioManager.Instance == null)
        {
            Debug.LogError("找不到AudioManager！请确保场景中有AudioManager对象。");
            return;
        }
        
        // 设置滑动条事件监听
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
        
        // 初始化滑动条值
        UpdateSlidersFromAudioManager();
    }
    
    private void UpdateSlidersFromAudioManager()
    {
        if (AudioManager.Instance == null) return;
        
        // 更新滑动条值（不触发事件）
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.masterVolume);
            UpdateVolumeText(masterVolumeText, AudioManager.Instance.masterVolume);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.musicVolume);
            UpdateVolumeText(musicVolumeText, AudioManager.Instance.musicVolume);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.sfxVolume);
            UpdateVolumeText(sfxVolumeText, AudioManager.Instance.sfxVolume);
        }
    }
    
    // 滑动条事件处理方法
    private void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value);
            UpdateVolumeText(masterVolumeText, value);
        }
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
            UpdateVolumeText(musicVolumeText, value);
        }
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
            UpdateVolumeText(sfxVolumeText, value);
        }
    }
    
    // 更新音量显示文本
    private void UpdateVolumeText(TextMeshProUGUI volumeText, float volume)
    {
        if (volumeText != null)
        {
            volumeText.text = $"{Mathf.RoundToInt(volume * 100)}%";
        }
    }
    
  
    public void ResetToDefaultVolumes()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(1f);
            AudioManager.Instance.SetMusicVolume(1f);
            AudioManager.Instance.SetSFXVolume(1f);
            UpdateSlidersFromAudioManager();
        }
    }
    
    public void MuteAllAudio()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(0f);
            UpdateSlidersFromAudioManager();
        }
    }
    
    public void UnmuteAllAudio()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(1f);
            UpdateSlidersFromAudioManager();
        }
    }
    
   
}