using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class AudioManager : ManagerBase<AudioManager>
{
    [SerializeField]
    private AudioSource BGAudioSource;

    #region 音量、播放控制
    [SerializeField][Range(0, 1)][OnValueChanged("UpdateAllAudioPlay")]
    private float globalVolume;
    public float GlobalVolume 
    { 
        get => globalVolume; 
        set 
        {
            if (globalVolume == value) return;  // 先判断发生变化再修改
            globalVolume = value; 
            UpdateAllAudioPlay(); 
        } 
    } // 封装字段并显示属性

    [SerializeField][Range(0, 1)][OnValueChanged("UpdateBGAudioPlay")]
    private float bgVolume;
    public float BGVolume 
    { 
        get => bgVolume; 
        set 
        {
            if (bgVolume == value) return;  // 先判断发生变化再修改
            bgVolume = value; 
            UpdateBGAudioPlay(); 
        } 
    }

    [SerializeField][Range(0, 1)][OnValueChanged("UpdateEffectAudioPlay")]
    private float effectVolume;
    public float EffectVolume 
    { 
        get => effectVolume; 
        set 
        {
            if (effectVolume == value) return;
            effectVolume = value; 
            UpdateEffectAudioPlay(); 
        } 
    }

    /// <summary>
    /// 静音部分逻辑
    /// </summary>
    [SerializeField][OnValueChanged("UpdateMute")]
    private bool isMute = false;
    public bool IsMute
    {
        get => isMute;
        set
        {
            if (isMute == value) return;
            isMute = value;
            UpdateMute();
        }
    }
    /// <summary>
    /// 循环部分逻辑
    /// </summary>
    [SerializeField][OnValueChanged("UpdateLoop")]
    private bool isLoop = true;
    public bool IsLoop
    {
        get => isLoop;
        set
        {
            if (isLoop == value) return;
            isLoop = value;
            UpdateLoop();
        }
    }

    private bool isPause = false;
    public bool IsPause
    {
        get => isPause;
        set
        {
            if (isPause == value) return;
            isPause = value;
            if (IsPause)
            {
                BGAudioSource.Pause();
            }
            else
            {
                BGAudioSource.UnPause();
            }
            UpdateEffectAudioPlay();
        }
    }

    /// <summary>
    /// 更新全部播放器类型
    /// </summary>
    private void UpdateAllAudioPlay()
    {
        UpdateBGAudioPlay();
        UpdateEffectAudioPlay();
    }
    /// <summary>
    /// 更新背景音乐
    /// </summary>
    private void UpdateBGAudioPlay()
    {
        BGAudioSource.volume = bgVolume * globalVolume;
    }
    /// <summary>
    /// 更新特效音乐
    /// </summary>
    private void UpdateEffectAudioPlay()
    {
        Debug.Log("更新特效音乐");
    }
    /// <summary>
    /// 更新背景音乐静音情况
    /// </summary>
    private void UpdateMute()
    {
        BGAudioSource.mute = isMute;
        UpdateEffectAudioPlay();
    }
    /// <summary>
    /// 更新背景音乐循环
    /// </summary>
    private void UpdateLoop()
    {
        BGAudioSource.loop = isLoop;
    }

    #endregion
    public override void Init()
    {
        base.Init();
        UpdateAllAudioPlay();
    }
    #region 背景音乐
    public void PlayBGAudio(AudioClip clip, bool loop = true, float volume = -1)
    {
        BGAudioSource.clip = clip;
        IsLoop = loop;
        if (volume!=-1)
        {
            BGVolume = volume;
        }
        BGAudioSource.Play();
    }

    public void PlayBGAudio(string clipPath, bool loop = true, float volume = -1)
    {
        AudioClip clip = ResManager.Instance.LoadAsset<AudioClip>(clipPath);
        PlayBGAudio(clip, loop, volume);
    }
    #endregion
}
