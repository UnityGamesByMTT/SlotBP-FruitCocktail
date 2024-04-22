using System.Collections;
using System.Collections.Generic;
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

    [Header("Animated Sprites")]
    //[SerializeField]
    //private Sprite[] Bonus_Sprite;
    [SerializeField]
    private Sprite[] Apple_sprite;
    [SerializeField]
    private Sprite[] Cherry_Sprite;
    [SerializeField]
    private Sprite[] Coconut_Sprite;
    [SerializeField]
    private Sprite[] Jelly_Sprite;
    [SerializeField]
    private Sprite[] Juice_Sprite;
    [SerializeField]
    private Sprite[] Lemon_Sprite;
    [SerializeField]
    private Sprite[] Orange_Sprite;
    [SerializeField]
    private Sprite[] Pear_Sprite;
    [SerializeField]
    private Sprite[] Pineapple_Sprite;
    [SerializeField]
    private Sprite[] Strawberry_Sprite;
    [SerializeField]
    private Sprite[] Watermelon_Sprite;




    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private GameObject Image_Prefab;    //icons prefab

    [SerializeField]
    private PayoutCalculation PayCalculator;

    private List<Tweener> alltweens = new List<Tweener>();


    [SerializeField]
    private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField]
    private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing

    private int numberOfSlots = 3;          //number of columns

    [SerializeField]
    int verticalVisibility = 3;

    [SerializeField]
    private SocketIOManager SocketManager;

    [SerializeField] private List<int> InitalizeList = new List<int>();
    [SerializeField] private List<int> Stoppositions = new List<int>();

    [SerializeField] private Transform Border;
    [SerializeField] private float X_distance;
    [SerializeField] private float Y_distance;
    private int currentIndex;
    [SerializeField] private int stopIndex;
    [SerializeField] private List<Sprite> OuterReelSpriteList;
    [SerializeField] private List<Transform> OuterReelSlots;
    [SerializeField] private GameObject OuterSlotItemPrefab;
    [SerializeField] private List<int> leftList;
    [SerializeField] private List<int> upList;
    [SerializeField] private List<GameObject> OuterReeAllItem;
    [SerializeField] private GameObject lighting;
    private bool ison = true;

    private void Start()
    {

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        PopulateInitalSlots(0, InitalizeList);
        PopulateInitalSlots(1, InitalizeList);
        PopulateInitalSlots(2, InitalizeList);
        PopulateOuterReel();

        InvokeRepeating("ToggleOnOff", 0.1f, 0.2f);
    }




    //just for testing purposes delete on production
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartSlots();
            //MoveBorder();
            StartCoroutine(MoveSelector());

        }
    }
    //populate the slots with the values recieved from backend
    internal void PopulateInitalSlots(int number, List<int> myvalues)
    {
        PopulateSlot(myvalues, number);
    }

    //reset the layout after populating the slots
    internal void LayoutReset(int number)
    {
        if (Slot_Elements[number]) Slot_Elements[number].ignoreLayout = true;
        if (SlotStart_Button) SlotStart_Button.interactable = true;
    }

    private void PopulateSlot(List<int> values, int number)
    {
        if (Slot_Objects[number]) Slot_Objects[number].SetActive(true);
        for (int i = 0; i < values.Count; i++)
        {
            GameObject myImg = Instantiate(Image_Prefab, Slot_Transform[number]);
            images[number].slotImages.Add(myImg.GetComponent<Image>());
            images[number].slotImages[i].sprite = myImages[values[i]];
            PopulateAnimationSprites(images[number].slotImages[i].gameObject.GetComponent<ImageAnimation>(), values[i]);
        }
        for (int k = 0; k < 2; k++)
        {
            GameObject mylastImg = Instantiate(Image_Prefab, Slot_Transform[number]);
            images[number].slotImages.Add(mylastImg.GetComponent<Image>());
            images[number].slotImages[images[number].slotImages.Count - 1].sprite = myImages[values[k]];
            PopulateAnimationSprites(images[number].slotImages[images[number].slotImages.Count - 1].gameObject.GetComponent<ImageAnimation>(), values[k]);
        }
        if (mainContainer_RT) LayoutRebuilder.ForceRebuildLayoutImmediate(mainContainer_RT);
        tweenHeight = (values.Count * IconSizeFactor) - 280;
    }

    //function to populate animation sprites accordingly
    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
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
                break;
            case 3:
                for (int i = 0; i < Jelly_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Jelly_Sprite[i]);
                }
                break;
            case 4:
                for (int i = 0; i < Juice_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Juice_Sprite[i]);
                }
                break;
            case 5:
                for (int i = 0; i < Lemon_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Lemon_Sprite[i]);
                }
                break;
            case 6:
                for (int i = 0; i < Orange_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Orange_Sprite[i]);
                }
                break;
            case 7:
                for (int i = 0; i < Pear_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Pear_Sprite[i]);
                }
                break;
            case 8:
                for (int i = 0; i < Pineapple_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Pineapple_Sprite[i]);
                }
                break;
            case 9:
                for (int i = 0; i < Strawberry_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Strawberry_Sprite[i]);
                }
                break;
            case 10:
                for (int i = 0; i < Watermelon_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Watermelon_Sprite[i]);
                }
                break;

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
        if (SlotStart_Button) SlotStart_Button.interactable = false;
        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }

        SocketManager.AccumulateResult();
        yield return new WaitForSeconds(0.5f);
        List<int> resultnum = SocketManager.tempresult.StopList?.Split(',')?.Select(Int32.Parse)?.ToList();

        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(resultnum[i], Slot_Transform[i], i);
        }

        yield return new WaitForSeconds(0.3f);
        GenerateMatrix(SocketManager.tempresult.StopList);
        CheckPayoutLineBackend(SocketManager.tempresult.resultLine, SocketManager.tempresult.x_animResult, SocketManager.tempresult.y_animResult);
        KillAllTweens();
        if (SlotStart_Button) SlotStart_Button.interactable = true;

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
    private void CheckPayoutLineBackend(List<int> LineId, List<string> x_AnimString, List<string> y_AnimString)
    {
        List<int> x_points = null;
        List<int> y_points = null;
        List<int> x_anim = null;
        List<int> y_anim = null;

        //for (int i = 0; i < LineId.Count; i++)
        //{
        //    x_points = x_string[LineId[i]]?.Split(',')?.Select(Int32.Parse)?.ToList();
        //    y_points = y_string[LineId[i]]?.Split(',')?.Select(Int32.Parse)?.ToList();
        //    PayCalculator.GeneratePayoutLinesBackend(x_points, y_points, x_points.Count);
        //}

        for (int i = 0; i < 3; i++)
        {

            StartGameAnimation(Tempimages[i].slotImages[0].gameObject);
        }

    }

    //generate the result matrix
    private void GenerateMatrix(string stopList)
    {
        List<int> numbers = stopList?.Split(',')?.Select(Int32.Parse)?.ToList();

        for (int i = 0; i < numbers.Count; i++)
        {
            //for (int s = 0; s < verticalVisibility; s++)
            //{
            if (i < 3)
                Tempimages[i].slotImages.Add(images[i].slotImages[(images[i].slotImages.Count - (numbers[i]))]);
            //}
        }
    }

    void PopulateOuterReel()
    {

        for (int i = 0; i < OuterReelSlots.Count; i++)
        {
            if (i % 2 == 0)
            {
                for (int j = 0; j < leftList.Count; j++)
                {
                    GameObject slotItem = Instantiate(OuterSlotItemPrefab, OuterReelSlots[i]);
                    Image slotItemImage = slotItem.transform.GetChild(0).GetComponent<Image>();
                    slotItemImage.sprite = OuterReelSpriteList[leftList[j]];

                }
                for (int l = 0; l < 2; l++)
                {
                    GameObject slotItem = Instantiate(OuterSlotItemPrefab, OuterReelSlots[i]);
                    Image slotItemImage = slotItem.transform.GetChild(0).GetComponent<Image>();
                    slotItemImage.sprite = OuterReelSpriteList[8];
                    if (l == 0)
                        slotItem.transform.SetAsFirstSibling();
                }

            }
            else
            {

                for (int k = 0; k < upList.Count; k++)
                {
                    GameObject slotItem = Instantiate(OuterSlotItemPrefab, OuterReelSlots[i]);
                    slotItem.transform.GetChild(0).GetComponent<Image>().sprite = OuterReelSpriteList[upList[k]];
                }
            }
        }

        for (int i = 0; i < OuterReelSlots[0].childCount; i++)
        {
            OuterReeAllItem.Add(OuterReelSlots[0].GetChild(i).gameObject);
        }

        for (int i = 0; i < OuterReelSlots[3].childCount; i++)
        {
            OuterReeAllItem.Add(OuterReelSlots[3].GetChild(i).gameObject);
        }

        for (int i = OuterReelSlots[2].childCount - 1; i >= 0; i--)
        {
            OuterReeAllItem.Add(OuterReelSlots[2].GetChild(i).gameObject);
        }

        for (int i = OuterReelSlots[1].childCount - 1; i >= 0; i--)
        {
            OuterReeAllItem.Add(OuterReelSlots[1].GetChild(i).gameObject);
        }

    }

    void ToggleOnOff() {
        ison=!ison;
        lighting.SetActive(ison);

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
        int tweenpos = 230 + ((reqpos - 1) * IconSizeFactor);
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 115, 0.5f).SetEase(Ease.OutElastic);
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



    //void MoveBorder()
    //{


    //    StartCoroutine(MoveSelectorLoop());


    //}

    IEnumerator MoveSelector()
    {

        for (int i = 0; i < 4; i++)
        {
            if (i == 3)
            {

                yield return StartCoroutine(ToggleSelectorAnimation(0.1f, 9));
                yield break;
            }
            yield return StartCoroutine(ToggleSelectorAnimation(0.1f));
        }
    }

    IEnumerator ToggleSelectorAnimation(float delay, int stop_index = -1)
    {

        for (int i = 0; i < OuterReeAllItem.Count; i++)
        {
            if (i == stop_index)
            {

                OuterReeAllItem[i].transform.GetChild(1).gameObject.SetActive(true);
                yield break;
            }
            OuterReeAllItem[i].transform.GetChild(1).gameObject.SetActive(true);


            yield return new WaitForSeconds(delay);

            OuterReeAllItem[i].transform.GetChild(1).gameObject.SetActive(false);

        }

    }


    //IEnumerator MoveSelectorLoop(int loop = 2)
    //{
    //    int x_direction = 1;
    //    int y_direction = -1;

    //    for (int i = 0; i < loop * 2; i++)
    //    {

    //        if (currentIndex > 25)
    //            currentIndex = 0;

    //        if (i >= (loop - 1) * 2)
    //        {

    //            yield return StartCoroutine(MoveSelectorXY(x_direction, y_direction, stopIndex));
    //            x_direction = x_direction * -1;
    //            y_direction = y_direction * -1;
    //        }
    //        else
    //        {
    //            yield return StartCoroutine(MoveSelectorXY(x_direction, y_direction));
    //            x_direction = x_direction * -1;
    //            y_direction = y_direction * -1;

    //        }


    //    }
    //}

    //IEnumerator MoveSelectorXY(int x_direction, int y_direction, int stop_index = -1)
    //{

    //    bool move_x = false;
    //    bool move_y = false;
    //    int max_x = 7;
    //    int max_y = 6;

    //    if (currentIndex >= 0 && currentIndex < 7)
    //    {
    //        move_x = true;
    //        max_x = 7 - currentIndex;

    //    }
    //    if (currentIndex >= 13 && currentIndex < 20)
    //    {
    //        move_x = true;
    //        max_x = 20 - currentIndex;
    //    }


    //    if ((currentIndex >= 7 && currentIndex < 13))
    //    {
    //        move_y = true;
    //        max_y = 13 - currentIndex;
    //    }

    //    if (currentIndex >= 20 && currentIndex < 25)
    //    {

    //        move_y = true;
    //        max_y = 25 - currentIndex;

    //    }

    //    if (currentIndex == 25)
    //        max_y = 1;
    //    //if (currentIndex == 0)
    //    //    max_y = 1;

    //    if (move_x)
    //    {

    //        for (int i = 0; i < max_x; i++)
    //        {
    //            if (currentIndex >= stop_index && stop_index >= 0)
    //            {
    //                Tweener tweener = Border.DOLocalMoveX(Border.localPosition.x + X_distance * x_direction, 0.1f);
    //                tweener.Kill();
    //                yield break;
    //            }
    //            Border.DOLocalMoveX(Border.localPosition.x + X_distance * x_direction, 0.1f);
    //            yield return new WaitForSeconds(0.1f);

    //            currentIndex++;

    //        }
    //        move_y = true;
    //        move_x = false;

    //    }

    //    if (move_y)
    //    {
    //        for (int i = 0; i < max_y; i++)
    //        {
    //            if (currentIndex >= stop_index && stop_index >= 0)
    //            {

    //                Tweener tweener = Border.DOLocalMoveY(Border.localPosition.y + Y_distance * y_direction, 0.1f);
    //                tweener.Kill();
    //                yield break;
    //            }
    //            Border.DOLocalMoveY(Border.localPosition.y + Y_distance * y_direction, 0.1f);
    //            yield return new WaitForSeconds(0.1f);
    //            currentIndex++;

    //        }
    //        move_x = true;
    //        move_y = false;
    //    }


    //}


    #endregion
}

