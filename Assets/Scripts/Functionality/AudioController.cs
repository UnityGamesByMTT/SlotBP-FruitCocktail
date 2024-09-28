using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    [SerializeField] internal AudioListener m_Player_Listener;
    [SerializeField] internal AudioSource m_BG_Audio;
    [SerializeField] internal AudioSource m_Bonus_BG_Audio;
    [SerializeField] internal AudioSource m_Click_Audio;
    [SerializeField] internal AudioSource m_Win_Audio;
    [SerializeField] internal AudioSource m_LooseAudio;
    [SerializeField] internal AudioSource m_Bonus_Audio;
    [SerializeField] internal AudioSource m_FreeSpin_Audio;
    [SerializeField] internal AudioSource m_Spin_Audio;
    [SerializeField] internal AudioSource m_Bonus_Spin_Audio;
    [SerializeField] internal AudioSource m_Spin_Button_Clicked;

    [SerializeField] internal Slider m_SoundSlider;
    [SerializeField] internal Slider m_MusicSlider;

    private event Action m_On_Application_Focus;
    private event Action m_On_Application_Out_Of_Focus;

    private void Start()
    {
        UpdateSoundVolume(m_SoundSlider.value);
        UpdateMusicVolume(m_MusicSlider.value);

        m_SoundSlider.onValueChanged.AddListener(delegate
        {
            UpdateSoundVolume(m_SoundSlider.value);
        });
        m_MusicSlider.onValueChanged.AddListener(delegate
        {
            UpdateMusicVolume(m_MusicSlider.value);
        });
    }

    private void UpdateSoundVolume(float value)
    {
        m_Click_Audio.volume = value;
        m_Win_Audio.volume = value;
        m_LooseAudio.volume = value;
        m_Bonus_Audio.volume = value;
        m_FreeSpin_Audio.volume = value;
        m_Spin_Audio.volume = value;
        m_Spin_Button_Clicked.volume = value;
    }

    private void UpdateMusicVolume(float value)
    {
        m_BG_Audio.volume = value;
        m_Bonus_BG_Audio.volume = value;
    }

    internal void InitialAudioSetup()
    {
        if (m_BG_Audio) m_BG_Audio.Play();

        m_On_Application_Focus += delegate
        {
            m_Player_Listener.enabled = true;
            if (m_BG_Audio) m_BG_Audio.UnPause();
            if (m_Click_Audio) m_Click_Audio.UnPause();
            if (m_Win_Audio) m_Win_Audio.UnPause();
            if (m_LooseAudio) m_LooseAudio.UnPause();
            if (m_Bonus_Audio) m_Bonus_Audio.UnPause();
            if (m_FreeSpin_Audio) m_FreeSpin_Audio.UnPause();
            if (m_Spin_Audio) m_Spin_Audio.UnPause();
        };

        m_On_Application_Out_Of_Focus += delegate
        {
            m_Player_Listener.enabled = false;
            if (m_BG_Audio) m_BG_Audio.Pause();
            if (m_Click_Audio) m_Click_Audio.Pause();
            if (m_Win_Audio) m_Win_Audio.Pause();
            if (m_LooseAudio) m_LooseAudio.Pause();
            if (m_Bonus_Audio) m_Bonus_Audio.Pause();
            if (m_FreeSpin_Audio) m_FreeSpin_Audio.Pause();
            if (m_Spin_Audio) m_Spin_Audio.Pause();
        };
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            m_Player_Listener.enabled = false;
        }
        else
        {
            m_Player_Listener.enabled = true;

        }
    }

    internal void ToggleMute(bool toggle, string type = "all")
    {
        switch (type)
        {
            case "bg":
                m_BG_Audio.mute = toggle;
                break;
            case "button":
                m_Click_Audio.mute = toggle;
                m_Spin_Audio.mute = toggle;
                break;
            case "wl":
                m_Win_Audio.mute = toggle;
                m_Bonus_Audio.mute = toggle;
                m_FreeSpin_Audio.mute = toggle;
                break;
            case "all":
                m_BG_Audio.mute = toggle;
                m_Click_Audio.mute = toggle;
                m_Win_Audio.mute = toggle;
                m_Bonus_Audio.mute = toggle;
                m_FreeSpin_Audio.mute = toggle;
                m_Spin_Audio.mute = toggle;
                break;
        }
    }

}