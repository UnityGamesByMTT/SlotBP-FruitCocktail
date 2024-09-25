using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using Newtonsoft.Json.Linq;


public class UIManager : MonoBehaviour
{

    [Header("Menu UI")]
    [SerializeField]
    private GameObject m_MenuButtonHolder;
    [SerializeField]
    private Button m_OpenMenu;
    [SerializeField]
    private Button m_CloseMenu;

    [Header("Popus UI")]
    [SerializeField]
    internal GameObject MainPopup_Object;

    [Header("About Popup")]
    [SerializeField]
    private GameObject AboutPopup_Object;
    [SerializeField]
    private Button AboutExit_Button;

    [Header("Paytable Popup")]
    [SerializeField] private GameObject PaytablePopup_Object;
    [SerializeField] private Button Left_Arrow;
    [SerializeField] private Button Right_Arrow;
    [SerializeField] private GameObject[] paytableList;
    [SerializeField] private int CurrentIndex = 0;

    [Header("Settings Popup")]
    [SerializeField]
    private GameObject SettingsPopup_Object;
    [SerializeField]
    private Button SettingsExit_Button;
    [SerializeField]
    private Button Sound_Button;
    [SerializeField]
    private Button Music_Button;
    [SerializeField]
    private AudioSource BG_Sounds;
    [SerializeField]
    private AudioSource Button_Sounds;
    [SerializeField]
    private AudioSource Spin_Sounds;
    [SerializeField]
    private GameObject MusicOn_Object;
    [SerializeField]
    private GameObject MusicOff_Object;
    [SerializeField]
    private GameObject SoundOn_Object;
    [SerializeField]
    private GameObject SoundOff_Object;
    [SerializeField] private TMP_Text[] SymbolsText;

    [SerializeField] private TMP_Text megaWInText;
    [SerializeField] private GameObject megaWIn;

    private bool isMusic = true;
    private bool isSound = true;

    [SerializeField] private AudioController audioController;
    [SerializeField] private SlotBehaviour slotManager;
    private int FreeSpins;
    public int currentSpin;
    [SerializeField] private TMP_Text freeSpintext;
    [SerializeField] private Image freeSpinSlider;
    [Header("Game Manager Reference")]
    [SerializeField] private GameManager m_GameManager;

    private void Start()
    {
        if (SettingsExit_Button) SettingsExit_Button.onClick.RemoveAllListeners();
        if (SettingsExit_Button) SettingsExit_Button.onClick.AddListener(delegate { ClosePopup(SettingsPopup_Object); });

        if (MusicOn_Object) MusicOn_Object.SetActive(true);
        if (MusicOff_Object) MusicOff_Object.SetActive(false);

        if (SoundOn_Object) SoundOn_Object.SetActive(true);
        if (SoundOff_Object) SoundOff_Object.SetActive(false);

        if (m_OpenMenu) m_OpenMenu.onClick.RemoveAllListeners();
        if (m_OpenMenu) m_OpenMenu.onClick.AddListener(delegate { OpenCloseMenu(true); });

        if (m_CloseMenu) m_CloseMenu.onClick.RemoveAllListeners();
        if (m_CloseMenu) m_CloseMenu.onClick.AddListener(delegate { OpenCloseMenu(false); });

        //if (FreeSpin_Button) FreeSpin_Button.onClick.RemoveAllListeners();
        //if (FreeSpin_Button) FreeSpin_Button.onClick.AddListener(delegate { StartFreeSpins(FreeSpins); });

        if (audioController) audioController.ToggleMute(false);

        isMusic = true;
        isSound = true;

        if (Sound_Button) Sound_Button.onClick.RemoveAllListeners();
        if (Sound_Button) Sound_Button.onClick.AddListener(ToggleSound);

        if (Music_Button) Music_Button.onClick.RemoveAllListeners();
        if (Music_Button) Music_Button.onClick.AddListener(ToggleMusic);

    }

    internal void PopulateWin(int value, double amount)
    {
        //if (Win_Image) Win_Image.sprite = MegaWin_Sprite;
        OpenPopup(megaWIn);
        StartPopupAnim(amount);
    }

    internal void StartFreeSpins(int spins)
    {

        FreeSpins = spins;
        currentSpin = FreeSpins;
        print("spin"+currentSpin);
        freeSpinSlider.fillAmount = 1f;
        freeSpintext.text = FreeSpins.ToString();
        Invoke("FreeSpinProcess",1.5f);


    }

    private void FreeSpinProcess()
    {

        slotManager.FreeSpin(FreeSpins);
    }

    internal void updateFreespinInfo() {
        currentSpin--;
        if (currentSpin > 0)
            freeSpinSlider.fillAmount = (float)currentSpin / (float)FreeSpins;
        else
            freeSpinSlider.fillAmount = 0;
        freeSpintext.text = currentSpin.ToString();
    }

    private void StartPopupAnim(double amount)
    {
        int initAmount = 0;
        //if (WinPopup_Object) WinPopup_Object.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);

        DOTween.To(() => initAmount, (val) => initAmount = val, (int)amount, 2f).OnUpdate(() =>
        {
            if (megaWInText) megaWInText.text = initAmount.ToString();
        });

        DOVirtual.DelayedCall(6f, () =>
        {
            if (megaWIn) megaWIn.SetActive(false);
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
            slotManager.CheckBonusGame();
        });
    }

    internal void InitialiseUIData(string SupportUrl, string AbtImgUrl, string TermsUrl, string PrivacyUrl, Paylines symbolsText, List<string> Specialsymbols)
    {
        //if (Support_Button) Support_Button.onClick.RemoveAllListeners();
        //if (Support_Button) Support_Button.onClick.AddListener(delegate { UrlButtons(SupportUrl); });

        //if (Terms_Button) Terms_Button.onClick.RemoveAllListeners();
        //if (Terms_Button) Terms_Button.onClick.AddListener(delegate { UrlButtons(TermsUrl); });

        //if (Privacy_Button) Privacy_Button.onClick.RemoveAllListeners();
        //if (Privacy_Button) Privacy_Button.onClick.AddListener(delegate { UrlButtons(PrivacyUrl); });

        PopulateSymbolsPayout(symbolsText);
        PopulateSpecialSymbols(Specialsymbols);
    }

    private void PopulateSpecialSymbols(List<string> Specialtext)
    {
        //for (int i = 0; i < SpecialSymbolsText.Length; i++)
        //{
        //    if (SpecialSymbolsText[i]) SpecialSymbolsText[i].text = Specialtext[i];
        //}
    }

    private void PopulateSymbolsPayout(Paylines paylines)
    {
        Debug.Log(string.Concat("<color=brown><b>", "Length: " + paylines.symbols.Count, "</b></color>"));
        for (int i = 0; i < paylines.symbols.Count; i++)
        {
            string text = null;

            try
            {
                if (paylines.symbols[i].Multiplier[0][0] != 0)
                {
                    text += string.Concat("<color=#F8D229>", "5x - " + paylines.symbols[i].Multiplier[0][0], "</color>");
                }
                if (paylines.symbols[i].Multiplier[1][0] != 0)
                {
                    text += string.Concat("<color=#F8D229>", "\n4x - " + paylines.symbols[i].Multiplier[1][0], "</color>");
                }
                if (paylines.symbols[i].Multiplier[2][0] != 0)
                {
                    text += string.Concat("<color=#F8D229>", "\n3x - " + paylines.symbols[i].Multiplier[2][0], "</color>");
                }
                if (SymbolsText[i]) SymbolsText[i].text = text;
            }
            catch
            {
                switch(paylines.symbols[i].Name)
                {
                    case "FREESPIN":
                        break;
                    case "SCATTER":
                        break;
                    case "JACKPOT":
                        break;
                    case "WILD":
                        break;
                    case "BONUS":
                        break;
                }
            }
        }
    }

    internal void CallOnExitFunction()
    {
        slotManager.CallCloseSocket();
        Application.ExternalCall("window.parent.postMessage", "onExit", "*");
    }

    private void OpenPopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
    }

    private void ClosePopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(false);
        if (MainPopup_Object) MainPopup_Object.SetActive(false);
    }

    private void ToggleMusic()
    {
        isMusic = !isMusic;
        if (isMusic)
        {
            if (MusicOn_Object) MusicOn_Object.SetActive(true);
            if (MusicOff_Object) MusicOff_Object.SetActive(false);
            audioController.ToggleMute(false, "bg");
        }
        else
        {
            if (MusicOn_Object) MusicOn_Object.SetActive(false);
            if (MusicOff_Object) MusicOff_Object.SetActive(true);
            audioController.ToggleMute(true, "bg");
        }
    }

    private void UrlButtons(string url)
    {
        Application.OpenURL(url);
    }

    private void ToggleSound()
    {
        isSound = !isSound;
        if (isSound)
        {
            if (SoundOn_Object) SoundOn_Object.SetActive(true);
            if (SoundOff_Object) SoundOff_Object.SetActive(false);
            if (audioController) audioController.ToggleMute(false, "button");
            if (audioController) audioController.ToggleMute(false, "wl");
        }
        else
        {
            if (SoundOn_Object) SoundOn_Object.SetActive(false);
            if (SoundOff_Object) SoundOff_Object.SetActive(true);
            if (audioController) audioController.ToggleMute(true, "button");
            if (audioController) audioController.ToggleMute(true, "wl");
        }
    }

    private void OpenCloseMenu(bool m_toggle)
    {
        if (m_toggle)
        {
            for(int i = 0; i < m_MenuButtonHolder.transform.childCount; i++)
            {
                m_MenuButtonHolder.transform.GetChild(i).DOLocalMoveY(195 -  (i * 120), 0.3f);
            }
            m_OpenMenu.gameObject.SetActive(false);
        }
        else
        {
            for (int i = 0; i < m_MenuButtonHolder.transform.childCount; i++)
            {
                m_MenuButtonHolder.transform.GetChild(i).DOLocalMoveY(195, 0.3f);
            }
            m_OpenMenu.gameObject.SetActive(true);
        }
    }
}
