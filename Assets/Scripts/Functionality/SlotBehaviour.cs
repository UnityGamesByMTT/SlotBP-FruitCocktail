using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotBehaviour : MonoBehaviour
{
    [SerializeField]
    private RectTransform mainContainer_RT;

    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;  //images taken initially

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;     //class to store total images
    [SerializeField]
    private List<SlotImage> Tempimages;     //class to store the result matrix

    [Header("Slots Objects")]
    [SerializeField]
    private GameObject[] Slot_Objects;
    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;

    private Dictionary<int, string> x_string = new Dictionary<int, string>();
    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;
    [SerializeField]
    private Button AutoSpinStop_Button;
    [SerializeField]
    private Button AutoSpin_Button;
    [SerializeField]
    private Button MaxBet_Button;
    [SerializeField]
    private Button BetPlus_Button;
    [SerializeField]
    private Button BetMinus_Button;
    [SerializeField]
    private Button LinePlus_Button;
    [SerializeField]
    private Button LineMinus_Button;

    [Header("Animated Sprites")]
    //[SerializeField]
    //private Sprite[] Bonus_Sprite;
    [SerializeField]
    internal Sprite[] Apple_sprite;
    [SerializeField]
    internal Sprite[] Cherry_Sprite;
    [SerializeField]
    internal Sprite[] Coconut_Sprite;
    [SerializeField]
    internal Sprite[] Jelly_Sprite;
    [SerializeField]
    internal Sprite[] Juice_Sprite;
    [SerializeField]
    internal Sprite[] Lemon_Sprite;
    [SerializeField]
    internal Sprite[] Orange_Sprite;
    [SerializeField]
    internal Sprite[] Pear_Sprite;
    [SerializeField]
    internal Sprite[] Pineapple_Sprite;
    [SerializeField]
    internal Sprite[] Strawberry_Sprite;
    [SerializeField]
    internal Sprite[] Watermelon_Sprite;
    [SerializeField]
    internal Sprite[] Wild_Sprite;


    [Header("Miscellaneous UI")]
    [SerializeField]
    private TMP_Text Balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text Lines_text;
    [SerializeField]
    private TMP_Text TotalWin_text;

    [Header("Audio Management")]
    [SerializeField] private AudioController audioController;

    [Header("paylines ")]
    [SerializeField] private List<TMP_Text> StaticLine_Texts;
    [SerializeField] private List<GameObject> StaticLine_Objects;

    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private GameObject Image_Prefab;    //icons prefab

    [SerializeField] private PayoutCalculation PayCalculator;
    [SerializeField] private BonusGame bonusManager;
    [SerializeField] private SocketIOManager SocketManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameManager m_GameManager;

    private List<Tweener> alltweens = new List<Tweener>();

    [SerializeField] private List<TMP_Text> m_BonusPayText = new List<TMP_Text>();

    [SerializeField]
    private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField]
    private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing

    private int numberOfSlots = 5;          //number of columns

    [SerializeField]
    int verticalVisibility = 3;

    [SerializeField] private GameObject charecter_happy;
    [SerializeField] private GameObject charecter_idle;

    Coroutine AutoSpinRoutine = null;
    Coroutine tweenroutine = null;
    Coroutine FreeSpinRoutine = null;
    bool IsAutoSpin = false;
    bool IsSpinning = false;
    bool IsFreeSpin = false;

    internal bool CheckPopups = false;
    private int BetCounter = 0;

    static private int Lines = 20;
    private double currentBalance = 0;
    private double currentTotalBet = 0;


    private void Start()
    {
        IsAutoSpin = false;
        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); m_GameManager.m_AudioController.m_Spin_Button_Clicked.Play(); });

        if (BetPlus_Button) BetPlus_Button.onClick.RemoveAllListeners();
        if (BetPlus_Button) BetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); m_GameManager.m_AudioController.m_Click_Audio.Play(); });
        if (BetMinus_Button) BetMinus_Button.onClick.RemoveAllListeners();
        if (BetMinus_Button) BetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); m_GameManager.m_AudioController.m_Click_Audio.Play(); });

        if (LinePlus_Button) LinePlus_Button.onClick.RemoveAllListeners();
        if (LinePlus_Button) LinePlus_Button.onClick.AddListener(delegate { ChangeBet(true); m_GameManager.m_AudioController.m_Click_Audio.Play(); });
        if (LineMinus_Button) LineMinus_Button.onClick.RemoveAllListeners();
        if (LineMinus_Button) LineMinus_Button.onClick.AddListener(delegate { ChangeBet(false); m_GameManager.m_AudioController.m_Click_Audio.Play(); });

        if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(delegate { AutoSpin(); m_GameManager.m_AudioController.m_Spin_Button_Clicked.Play(); });


        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(delegate { StopAutoSpin(); m_GameManager.m_AudioController.m_Click_Audio.Play(); });

        tweenHeight = (myImages.Length * IconSizeFactor) - 280;
    }

    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {

            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());

        }
    }

    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {

            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));

        }
    }

    private void StopAutoSpin()
    {
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }

    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
        }
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        while (i < spinchances)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            i++;
        }
        ToggleButtonGrp(true);
        IsFreeSpin = false;
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }

    //Fetch Lines from backend
    internal void FetchLines(string LineVal, int count)
    {
        y_string.Add(count + 1, LineVal);
        //StaticLine_Texts[count].text = (count + 1).ToString();
        //StaticLine_Objects[count].SetActive(true);
    }

    //Generate Static Lines from button hovers
    internal void GenerateStaticLine(int LineNo)
    {
        DestroyStaticLine();
        int LineID = 1;
        LineID = LineNo;
        //try
        //{
        //    LineID = int.Parse(LineID_Text.text);
        //}
        //catch (Exception e)
        //{
        //    Debug.Log("Exception while parsing " + e.Message);
        //}
        List<int> y_points = null;
        y_points = y_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();

        PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, true);
    }

    //Destroy Static Lines from button hovers
    internal void DestroyStaticLine()
    {
        PayCalculator.ResetStaticLine();
    }

    private void MaxBet()
    {
        if (audioController.m_Player_Listener.enabled) audioController.m_Click_Audio.Play();

        BetCounter = SocketManager.initialData.Bets.Count - 1;
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count;
        if (TotalBet_text) TotalBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        CompareBalance();
    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController.m_Player_Listener.enabled) audioController.m_Click_Audio.Play();
        if (IncDec)
        {
            if (BetCounter < SocketManager.initialData.Bets.Count - 1)
            {
                BetCounter++;
            }
        }
        else
        {
            if (BetCounter > 0)
            {
                BetCounter--;
            }
        }
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count;
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString("f3");
        if (Lines_text) Lines_text.text = (SocketManager.initialData.Bets[BetCounter]).ToString();
        CompareBalance();
    }


    //just for testing purposes delete on production
    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space) && SlotStart_Button.interactable)
    //    {
    //        StartSlots();
    //    }
    //}

    internal void SetInitialUI()
    {
        BetCounter = 0;
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString("f3");
        if (Lines_text) Lines_text.text = (SocketManager.initialData.Bets[BetCounter]).ToString();
        if (TotalWin_text) TotalWin_text.text = 0.ToString("f3");
        if (Balance_text) Balance_text.text = (SocketManager.playerdata.Balance).ToString();
        uiManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines, SocketManager.initUIData.spclSymbolTxt);
        //bonusManager.PopulateBonusPaytable(SocketManager.bonusdata);
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = double.Parse(TotalBet_text.text);
        for (int i = 0; i < m_BonusPayText.Count; i++)
        {
            m_BonusPayText[i].text = SocketManager.bonusdata[i].ToString() + " x";
        }
        CompareBalance();
    }

    //reset the layout after populating the slots
    internal void LayoutReset(int number)
    {
        if (Slot_Elements[number]) Slot_Elements[number].ignoreLayout = true;
        if (SlotStart_Button) SlotStart_Button.interactable = true;
    }

    //function to populate animation sprites accordingly
    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();

        switch (val)
        {
            case 0:
                for (int i = 0; i < Apple_sprite.Length; i++)
                {
                    animScript.textureArray.Add(Apple_sprite[i]);
                }
                animScript.AnimationSpeed = 30f;
                break;
            case 1:
                for (int i = 0; i < Cherry_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Cherry_Sprite[i]);
                }
                animScript.AnimationSpeed = 30f;
                break;
            case 2:
                for (int i = 0; i < Coconut_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Coconut_Sprite[i]);
                }
                animScript.AnimationSpeed = 15f;
                break;
            case 3:
                for (int i = 0; i < Lemon_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Lemon_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;
            case 4:
                for (int i = 0; i < Orange_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Orange_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;
            case 5:
                for (int i = 0; i < Pear_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Pear_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;
            case 6:
                for (int i = 0; i < Pineapple_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Pineapple_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;
            case 7:
                for (int i = 0; i < Strawberry_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Strawberry_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;
            case 8:
                for (int i = 0; i < Wild_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Wild_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;
            case 9:
                for (int i = 0; i < Watermelon_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Watermelon_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;
            case 10:
                for (int i = 0; i < Juice_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Juice_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;
            case 11:
                for (int i = 0; i < Jelly_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Jelly_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;
        }
    }

    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        if (audioController.m_Player_Listener.enabled) audioController.m_Spin_Audio.Play();

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }

        if (SlotStart_Button) SlotStart_Button.interactable = false;
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        PayCalculator.ResetLines();
        tweenroutine = StartCoroutine(TweenRoutine());
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {

        if (currentBalance < currentTotalBet && !IsFreeSpin)
        {
            CompareBalance();
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            ToggleButtonGrp(true);
            yield break;
        }
        IsSpinning = true;

        ToggleButtonGrp(false);

        if (IsFreeSpin)
        {

            uiManager.updateFreespinInfo();
            uiManager.currentSpin--;
        }
        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }

        double bet = 0;
        double balance = 0;
        try
        {
            bet = double.Parse(TotalBet_text.text);
        }

        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        double initAmount = balance;
        balance = balance - bet;

        SocketManager.AccumulateResult(BetCounter);
        yield return new WaitUntil(() => SocketManager.isResultdone);

        Debug.Log(string.Concat("<color=green><b>", string.Join(", ", SocketManager.resultData.ResultReel[0]), "</b></color>"));
        Debug.Log(string.Concat("<color=green><b>", string.Join(", ", SocketManager.resultData.ResultReel[1]), "</b></color>"));
        Debug.Log(string.Concat("<color=green><b>", string.Join(", ", SocketManager.resultData.ResultReel[2]), "</b></color>"));

        //HACK: Image Populate Loop For Testing
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < Tempimages[i].slotImages.Count; j++)
            {
                Tempimages[i].slotImages[j].sprite = myImages[int.Parse(SocketManager.resultData.ResultReel[j][i])];
                PopulateAnimationSprites(Tempimages[i].slotImages[j].gameObject.GetComponent<ImageAnimation>(), int.Parse(SocketManager.resultData.ResultReel[j][i]));
            }
        }

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(5, Slot_Transform[i], i);
        }

        yield return new WaitForSeconds(0.3f);

        m_GameManager.m_AudioController.m_Spin_Audio.Stop();

        //TODO: To Be Uncommented When Move To Publishing
        CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, SocketManager.resultData.jackpot);
        currentBalance = SocketManager.playerdata.Balance;
        KillAllTweens();
        if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("f3");
        if (Balance_text) Balance_text.text = ((double)SocketManager.playerdata.Balance).ToString("f3");


        CheckPopups = true;

        if (SocketManager.resultData.WinAmout >= currentTotalBet * 15)
        {
            uiManager.PopulateWin(3, (double)SocketManager.resultData.WinAmout);
        }
        else
        {
            yield return new WaitForSeconds(0.8f);
            CheckBonusGame();
        }

        yield return new WaitUntil(() => !CheckPopups);
        if (!IsAutoSpin && !IsFreeSpin)
        {
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            yield return new WaitForSeconds(0.8f);
            IsSpinning = false;
        }


        if (SocketManager.resultData.freeSpins.isNewAdded && !IsFreeSpin)
        {

            uiManager.StartFreeSpins((int)SocketManager.resultData.freeSpins.count);
            yield break;
        }
    }

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
            //if (AutoSpin_Button) AutoSpin_Button.interactable = false;
            //if (SlotStart_Button) SlotStart_Button.interactable = false;
        }
        //else
        //{
        //    if (AutoSpin_Button) AutoSpin_Button.interactable = true;
        //    if (SlotStart_Button) SlotStart_Button.interactable = true;
        //}
    }

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
    }

    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, myImages.Length);
                Tempimages[i].slotImages[j].sprite = myImages[randomIndex];
            }
        }
    }

    internal void CheckBonusGame()
    {
        if (SocketManager.resultData.isBonus)
        {
            m_GameManager.m_AudioController.m_Bonus_Audio.Play();
            m_GameManager.m_PushObject(m_GameManager.m_Bonus_Start_Object);
            bonusManager.StartBonus(SocketManager.resultData.BonusResult.winings.Count, SocketManager.resultData.BonusResult);
        }
        else
        {
            CheckPopups = false;
        }

        if (SocketManager.resultData.freeSpins.count > 0)
        {
            if (IsAutoSpin)
            {
                StopAutoSpin();
            }
        }
    }

    internal void ToggleButtonGrp(bool toggle)
    {

        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (MaxBet_Button) MaxBet_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if(!IsSpinning || !IsAutoSpin || !IsFreeSpin)
        {
            if (LinePlus_Button) LinePlus_Button.interactable = true;
            if (LineMinus_Button) LineMinus_Button.interactable = true;
            if (BetMinus_Button) BetMinus_Button.interactable = true;
            if (BetPlus_Button) BetPlus_Button.interactable = true;
        }
        else
        {
            if (LinePlus_Button) LinePlus_Button.interactable = toggle;
            if (LineMinus_Button) LineMinus_Button.interactable = toggle;
            if (BetMinus_Button) BetMinus_Button.interactable = toggle;
            if (BetPlus_Button) BetPlus_Button.interactable = toggle;
        }

    }

    //start the icons animation
    private void StartGameAnimation(GameObject animObjects)
    {
        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        temp.StartAnimation();
        TempList.Add(temp);
    }

    //stop the icons animation
    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
        }
    }

    //generate the payout lines generated 
    private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, double jackpot = 0)
    {
        List<int> y_points = null;
        List<int> points_anim = null;
        if (LineId.Count > 0)
        {
            if (audioController.m_Player_Listener.enabled) audioController.m_Win_Audio.Play();

            for (int i = 0; i < LineId.Count; i++)
            {
                y_points = y_string[LineId[i] + 1]?.Split(',')?.Select(Int32.Parse)?.ToList();
                PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count);
            }

            if (jackpot > 0)
            {
                for (int i = 0; i < Tempimages.Count; i++)
                {
                    for (int k = 0; k < Tempimages[i].slotImages.Count; k++)
                    {
                        StartGameAnimation(Tempimages[i].slotImages[k].gameObject);
                    }
                }
            }
            else
            {
                for (int i = 0; i < points_AnimString.Count; i++)
                {
                    points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                    for (int k = 0; k < points_anim.Count; k++)
                    {
                        if (points_anim[k] >= 10)
                        {
                            StartGameAnimation(Tempimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject);
                        }
                        else
                        {
                            StartGameAnimation(Tempimages[0].slotImages[points_anim[k]].gameObject);
                        }
                    }
                }
            }
        }
        else
        {

            if (audioController.m_Player_Listener.enabled) audioController.m_LooseAudio.Play();
        }
    }

    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetLoops(-1, LoopType.Restart).SetDelay(0);
        tweener.Play();
        alltweens.Add(tweener);
    }

    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index)
    {
        alltweens[index].Pause();
        int tweenpos = (reqpos * IconSizeFactor) - IconSizeFactor;
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100, 0.5f).SetEase(Ease.OutElastic);
        yield return new WaitForSeconds(0.2f);
    }

    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(13);
}

