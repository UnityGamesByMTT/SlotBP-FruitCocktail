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

    [SerializeField]
    private RectTransform mainContainer_RT;

    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;
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

    [SerializeField] private List<OuterReelItem> OuterReeAllItem;
    [SerializeField] private GameObject lighting;

    [SerializeField] private TMP_Text currentBet;
    [SerializeField] private TMP_Text creditAmount;
    [SerializeField] private TMP_Text multiplier;
    [SerializeField] private List<int> animIndex = new List<int>();
    private bool ison = true;
    public List<int> resultnum = new List<int>();
    private int resultmult = 0;
    private double bet = 0;

    private void Start()
    {

        //PopulateOuterReel();
        InvokeRepeating("ToggleOnOff", 0.1f, 0.2f);
        //startGame(new List<int> { 2, 3, 4, 5, 6, 3, 4, 3, 2, 2, 3, 4, 5, 7 }, 0.1);
        tweenHeight = (myImages.Length * IconSizeFactor) - 280;
    }


    internal void startGame(List<int> result, double currentBet)
    {

        List<int> allItem = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        bet = currentBet;
       
        for (int i = 0; i < 3; i++)
        {
            resultnum.Add(result[i]);
        }

        stopIndex = result[3];

        for (int i = 0; i < 3; i++)
        {
            resultmult += (result[result.Count - 1 - i]);
        }

        for (int i = 0; i < resultnum.Count; i++)
        {
            if (resultnum[i] == stopIndex)
                animIndex.Add(i);
        }
        Shuffle(InitalizeList);
        PopulateOuterReel();
        PopulateInitalSlots(0, InitalizeList);
        PopulateInitalSlots(1, InitalizeList);
        PopulateInitalSlots(2, InitalizeList);
        bonusGame.SetActive(true);
        StartSlots();

    }


    //just for testing purposes delete on production
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartSlots();
            MoveSelector();

        }
    }
    //populate the slots with the values recieved from backend
    internal void PopulateInitalSlots(int number, List<int> myvalues)
    {
        PopulateSlot(myvalues, number);
    }

    //reset the layout after populating the slots
    //internal void LayoutReset(int number)
    //{
    //    if (Slot_Elements[number]) Slot_Elements[number].ignoreLayout = true;
    //    //if (SlotStart_Button) SlotStart_Button.interactable = true;
    //}

    private void PopulateSlot(List<int> values, int number)
    {
        Shuffle(values);
        if (Slot_Objects[number]) Slot_Objects[number].SetActive(true);
        for (int i = 0; i < values.Count; i++)
        {
            //GameObject myImg = Instantiate(Image_Prefab, Slot_Transform[number]);
            GameObject myImg = null;
            images[number].slotImages.Add(myImg.GetComponent<Image>());
            images[number].slotImages[i].sprite = myImages[values[i]];
        }
        int max_child = Slot_Objects[number].transform.childCount;


        Slot_Objects[number].transform.GetChild(max_child - resultnum[number] - 1).GetComponent<Image>().sprite = myImages[resultnum[number]];
        if (mainContainer_RT) LayoutRebuilder.ForceRebuildLayoutImmediate(mainContainer_RT);
    }


    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        PayCalculator.ResetLines();
        StartCoroutine(TweenRoutine());
        for (int i = 0; i < Tempimages.Count; i++)
        {
            Tempimages[i].slotImages.Clear();
            Tempimages[i].slotImages.TrimExcess();
        }
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {
        currentBet.text = bet.ToString();
        yield return new WaitForSeconds(0.5f);
        Coroutine moveSelector = StartCoroutine(ToggleSelectorAnimation(0.05f, 2));
        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(resultnum[i] + 1, Slot_Transform[i], i);
        }

        yield return moveSelector;
        CheckPayoutLineBackend(resultnum);

        creditAmount.text = (resultmult * bet).ToString();
        multiplier.text = "x " + resultmult.ToString();

        yield return new WaitForSeconds(3f);
        KillAllTweens();
        Reset();



    }


    private void CheckPayoutLineBackend(List<int> numbers)
    {

        for (int i = 0; i < animIndex.Count; i++)
        {
            Tempimages[animIndex[i]].slotImages.Add(images[animIndex[i]].slotImages[(images[animIndex[i]].slotImages.Count - (numbers[animIndex[i]] + 1))]);
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


    void PopulateOuterReel()
    {
        GameObject slotItem;
        OuterReelItem temp;

        for (int i = 0; i < OuterReelSlots.Count; i++)
        {

            if (i % 2 == 0)
            {
                setVerticalist();
                verticalList.RemoveAll(item => item == -1);
                Shuffle(verticalList);
                verticalList.Insert(0, -1);
                verticalList.Add(-1);
                for (int j = 0; j < verticalList.Count; j++)
                {
                    slotItem = OuterReeAllItem[i].gameObject;
                    slotItem = null;
                    temp = slotItem.GetComponent<OuterReelItem>();
                    if (verticalList[j] == -1)
                    {
                        temp.image.sprite = exitSprite;
                        temp.id = -1;
                    }
                    else
                    {
                        temp.image.sprite = myImages[verticalList[j]];
                        temp.id = verticalList[j];
                    }
                    if (i == 2) slotItem.transform.SetAsFirstSibling();
                }

            }
            else
            {
                setHorizontalist();
                Shuffle(horizontalList);
                for (int k = 0; k < horizontalList.Count; k++)
                {
                    slotItem = OuterReeAllItem[i].gameObject;
                    slotItem = null;
                    temp = slotItem.GetComponent<OuterReelItem>();

                    temp.image.sprite = myImages[horizontalList[k]];
                    temp.id = horizontalList[k];
                    if (i == 3) slotItem.transform.SetAsFirstSibling();
                }
            }
        }

        setOuterReel();
    }

    void setVerticalist()
    {
        verticalList.Clear();
        List<int> tempList = new List<int>();
        for (int i = 0; i < myImages.Length; i++)
        {
            tempList.Add(i);
        }
            verticalList.Clear();
        for (int i = 0; i < 5; i++)
        {
            int index = UnityEngine.Random.Range(0, tempList.Count);
            verticalList.Add(tempList[index]);
            tempList.Remove(tempList[index]);

        }
        tempList.Clear();

    }

    void setHorizontalist()
    {
        horizontalList.Clear();
        List<int> tempList = new List<int>();
        for (int i = 0; i < myImages.Length; i++)
        {
            tempList.Add(i);
        }
            horizontalList.Clear();
        for (int i = 0; i < 6; i++)
        {
            int index = UnityEngine.Random.Range(0, tempList.Count);
            horizontalList.Add(tempList[index]);
            tempList.Remove(tempList[index]);

        }
        tempList.Clear();


    }

    void setOuterReel()
    {

        //0,6,13,19
        List<int> outerReelIndex = new List<int>();
        for (int i = 0; i < OuterReeAllItem.Count; i++)
        {
            if (OuterReeAllItem[i].id != -1)
                outerReelIndex.Add(i);
        }

        int rndIndex = outerReelIndex[UnityEngine.Random.Range(0, outerReelIndex.Count)];
        print("randomindex " + rndIndex);
        OuterReeAllItem[rndIndex].image.sprite = myImages[stopIndex];
        OuterReeAllItem[rndIndex].id = stopIndex;

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
        OuterReeAllItem.Clear();
        foreach (var item in images)
        {
            item.slotImages.Clear();
        }
        foreach (OuterReelItem item in OuterReeAllItem)
        {
            item.selector.SetActive(false);

        }

        bonusGame.SetActive(false);
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
        int tweenpos = (reqpos * (IconSizeFactor + 40)) - (IconSizeFactor + (2 * 40));
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos - 100 - 40, 0.5f).SetEase(Ease.OutElastic);
        yield return new WaitForSeconds(0.2f);
    }


    private void KillAllTweens()
    {
        for (int i = 0; i < alltweens.Count; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }


    void MoveSelector()
    {
        StartCoroutine(ToggleSelectorAnimation(0.05f, 2));
    }

    IEnumerator ToggleSelectorAnimation(float delay, int noOfRotation = 1)
    {
        for (int j = 0; j < noOfRotation + 1; j++)
        {
            foreach (OuterReelItem item in OuterReeAllItem)
            {
                item.selector.SetActive(true);
                if (j == noOfRotation && item.id == stopIndex)
                {

                    slotBehaviour.CheckPopups = false;
                    yield break;
                }
                yield return new WaitForSeconds(delay);
                item.selector.SetActive(false);

            }
        }

    }

    void Shuffle(List<int> ts)
    {
        int count = ts.Count;
        int last = count - 1;
        for (int i = 0; i < last; ++i)
        {
            int r = UnityEngine.Random.Range(i, count);
            int tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
    #endregion
}

