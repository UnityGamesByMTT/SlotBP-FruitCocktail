using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;


public class BonusGame : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> Tempimages;

    [Header("Slots Objects")]
    [SerializeField]
    private GameObject[] Slot_Objects;
    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;


    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private GameObject Image_Prefab;    //icons prefab

    [SerializeField]
    private PayoutCalculation PayCalculator;

    private List<Tweener> alltweens = new List<Tweener>();


    [SerializeField]
    private List<ImageAnimation> TempList;

    [SerializeField]
    private int IconSizeFactor = 100;

    private int numberOfSlots = 3;

    [SerializeField]
    int verticalVisibility = 3;

    [SerializeField]
    private SocketIOManager SocketManager;
    [SerializeField] private SlotBehaviour slotBehaviour;

    [SerializeField] private List<int> InitalizeList = new List<int>();

    [SerializeField] private int stopIndex;
    [SerializeField] private Sprite exitSprite;
    [SerializeField] private GameObject bonusGame;
    [SerializeField] private List<Transform> OuterReelSlots;
    [SerializeField] private GameObject OuterSlotItemPrefab;
    [SerializeField] private List<int> verticalList;
    [SerializeField] private List<int> horizontalList;

    [SerializeField] private List<OuterReelItem> Outer_Reel_All_Item;
    [SerializeField] private GameObject lighting;

    [SerializeField] private TMP_Text currentBet;
    [SerializeField] private TMP_Text creditAmount;
    [SerializeField] private TMP_Text multiplier;
    [SerializeField] private List<int> animIndex = new List<int>();
    [SerializeField] private int m_SpaceFactor;

    [SerializeField] private List<IconPos> OuterIconsPosition;

    BonusResult m_DefaultStructure;
    BonusResult m_ReceivedStructure;

    private bool ison = true;
    public List<int> resultnum = new List<int>();
    private int resultmult = 0;
    private double bet = 0;
    private int N_SpinCount_Begin = 0;
    private int N_SpinCount = 0;
    private Coroutine m_Spinning;
    private Coroutine tweenroutine;
    private bool IsSpinning;

    private void Start()
    {
        InvokeRepeating("ToggleOnOff", 0.1f, 0.2f);
        tweenHeight = (myImages.Length * IconSizeFactor) - 280;

        m_DefaultStructure = new BonusResult
        {
            innerMatrix = new List<List<int>> { new List<int> { 5, 5, 6 }, new List<int> { 6, 6, 3 }, new List<int> { 2, 3, 4 } },
            outerRingSymbol = new List<int> { 5, 3, 7 },
            totalWinAmount = 2.5,
            winings = new List<string> { "1.0", "1.0", "0" }
        };
    }

    //just for testing purposes delete on production
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartBonus(m_DefaultStructure.winings.Count, m_DefaultStructure);
        }
    }

    internal void StartBonus(int m_count, BonusResult m_BonusData)
    {
        Debug.Log(m_count);
        bonusGame.SetActive(true);
        m_ReceivedStructure = m_BonusData;
        N_SpinCount = m_count;

        if (!IsSpinning)
        {
            IsSpinning = true;

            if (m_Spinning != null)
            {
                StopCoroutine(m_Spinning);
                m_Spinning = null;
            }
            m_Spinning = StartCoroutine(AutoSpinCoroutine());
        }
    }

    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }

        PayCalculator.ResetLines();

        tweenroutine = StartCoroutine(TweenRoutine());
    }

    private void StopAutoSpin()
    {
        //if (audioController) audioController.PlaySpinButtonAudio();

        if (IsSpinning)
        {
            IsSpinning = false;
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {
        for(int i = 0; i < N_SpinCount; i ++)
        {
            StartSlots(IsSpinning);
            yield return tweenroutine;
        }

        yield return new WaitForSeconds(0.2f);

        StopAutoSpin();
        Reset();
        slotBehaviour.ToggleButtonGrp(true);
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        if (m_Spinning != null || tweenroutine != null)
        {
            try
            {
                StopCoroutine(m_Spinning);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                m_Spinning = null;
                StopCoroutine(StopAutoSpinCoroutine());
            }
            catch (Exception e)
            {
                Debug.Log("Error Occured..." + string.Concat("<color=red><b>", e, "</b></color>"));
            }
        }
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {
        IsSpinning = true;
        currentBet.text = bet.ToString();
        Coroutine moveSelector = StartCoroutine(ToggleSelectorAnimation(0.05f, 2));
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);

        PopulateInnerMatrix();

        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(5, Slot_Transform[i], i);
        }

        yield return moveSelector;
        CheckPayoutLineBackend(resultnum);

        creditAmount.text = (resultmult * bet).ToString();
        multiplier.text = "x " + resultmult.ToString();

        yield return new WaitForSeconds(3f);
        KillAllTweens();
    }


    private void CheckPayoutLineBackend(List<int> numbers)
    {

        for (int i = 0; i < animIndex.Count; i++)
        {
            StartGameAnimation(Tempimages[animIndex[i]].slotImages[0].gameObject);
        }

    }

    private void StartGameAnimation(GameObject animObjects)
    {

        Tweener tweener = animObjects.transform.DOScale(Vector3.one * 1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        alltweens.Add(tweener);
    }

    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
        }
    }

    void ToggleOnOff()
    {
        ison = !ison;
        lighting.SetActive(ison);

    }

    private void Reset()
    {
        StopGameAnimation();

        foreach (Transform item in OuterReelSlots)
        {
            for (int i = item.childCount - 1; i >= 0; i--)
            {
                Destroy(item.GetChild(i).gameObject);
            }
        }

        resultnum.Clear();
        animIndex.Clear();
        Outer_Reel_All_Item.Clear();
        foreach (OuterReelItem item in Outer_Reel_All_Item)
        {
            item.selector.SetActive(false);

        }

        bonusGame.SetActive(false);
    }

    private void PopulateInnerMatrix()
    {
        for(int i = 0; i < Tempimages.Count; i++)
        {
            Tempimages[i].slotImages[0].transform.GetChild(0).GetComponent<Image>().sprite = myImages[m_ReceivedStructure.innerMatrix[N_SpinCount_Begin][i]];
        }
        if(N_SpinCount_Begin < N_SpinCount)
        {
            N_SpinCount_Begin++;
        }
    }

    private void PopulateOuterMatrix()
    {

    }

    #region [[===TWEENING CODE===]]

    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 1.2f).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear).SetDelay(0);
        tweener.Play();
        alltweens.Add(tweener);
    }

    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index)
    {
        bool IsRegister = false;
        yield return alltweens[index].OnStepComplete(delegate { IsRegister = true; });
        yield return new WaitUntil(() => IsRegister);
        alltweens[index].Kill();
        //alltweens[index].Pause();
        int tweenpos = (reqpos * (IconSizeFactor + m_SpaceFactor)) - (IconSizeFactor + (2 * m_SpaceFactor)) + 20;
        //alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos - 100 - 40, 0.5f).SetEase(Ease.OutElastic);
        alltweens[index] = slotTransform.DOLocalMoveY((-tweenpos + (100)) + (m_SpaceFactor > 0 ? m_SpaceFactor / 4 : 0), 1.1f).SetEase(Ease.OutQuad);
        //yield return new WaitForSeconds(0.2f);
        yield return alltweens[index].WaitForCompletion();
        alltweens[index].Kill();
    }

    //HACK: Killing all initialized tweens to stop tweens.
    private void KillAllTweens()
    {
        for (int i = 0; i < alltweens.Count; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }

    //HACK: This coroutine is used to run the border of the boxes one after another.
    IEnumerator ToggleSelectorAnimation(float delay, int noOfRotation = 1)
    {
        for (int j = 0; j < noOfRotation + 1; j++)
        {
            for(int i = Outer_Reel_All_Item.Count - 1; i > 0; i--)
            {
                Outer_Reel_All_Item[i].selector.SetActive(true);
                if (j == noOfRotation && Outer_Reel_All_Item[i].id == stopIndex)
                {

                    slotBehaviour.CheckPopups = false;
                    yield break;
                }
                yield return new WaitForSeconds(delay);
                Outer_Reel_All_Item[i].selector.SetActive(false);
            }
        }
    }
    #endregion

    private struct IconPos
    {
        public List<OuterReelItem> m_Icons;
    }
}