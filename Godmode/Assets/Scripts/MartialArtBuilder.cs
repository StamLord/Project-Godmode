using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MartialArtBuilder : MonoBehaviour
{
    [Header("References")]
    
    public Animator animator;
    public Transform customParent;
    public Transform moveParent;
    public GameObject addCustomButton;
    public Button[] editableMoves;

    [Header("Prefabs")]
    public GameObject customButtonPrefab;
    public GameObject moveButtonPrefab;

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> parent of dc1c306... test
    [Header("Design")]
=======
>>>>>>> parent of e540f79... 25.05
=======
>>>>>>> parent of e540f79... 25.05
=======
>>>>>>> parent of e540f79... 25.05
=======
>>>>>>> parent of e540f79... 25.05
=======
>>>>>>> parent of e540f79... 25.05
=======
>>>>>>> parent of e540f79... 25.05
    public Sprite regularCustom;
    public Sprite selectedCustom;

    protected List<MoveButton> moveButtons = new List<MoveButton>();
    protected Dictionary<GameObject, MartialArt> customButtons = new Dictionary<GameObject, MartialArt>();
<<<<<<< HEAD
=======
    protected Dictionary<GameObject, Move> moveButtons;
    protected Dictionary<GameObject, MartialArt> customButtons;
>>>>>>> parent of c4c115a... 21.05
=======
    protected Dictionary<GameObject, Move> moveButtons;
    protected Dictionary<GameObject, MartialArt> customButtons;
>>>>>>> parent of c4c115a... 21.05
=======
>>>>>>> parent of dc1c306... test

    [Header("Database")]
    public List<Move> allMoves = new List<Move>();
    public List<MartialArt> customs = new List<MartialArt>();
    public int maxCustoms = 3;

    [SerializeField]
    protected List<GameObject> instantiatedCustomSlots = new List<GameObject>();
    [SerializeField]
    protected int indexEditing;
    [SerializeField]
    protected MartialArt editing;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        int i = 0;
        foreach (MartialArt c in customs)
        {
            GameObject go = Instantiate(customButtonPrefab, customParent);
            Button b = go.GetComponent<Button>();
            b.onClick.AddListener(delegate { SetEditing(i); });
            customButtons.Add(go, c);
            i++;
        }

        foreach (Move m in allMoves)
        {
            GameObject go = Instantiate(moveButtonPrefab, moveParent);
            go.GetComponentInChildren<TextMeshProUGUI>().text = m.name;
            MoveButton b = go.GetComponent<MoveButton>();
            b.move = m;
            b.manager = this;
            moveButtons.Add(b);
        }

        UpdateUI();
    }

    public void CreateNewCustom()
    {
        if (customs.Count >= maxCustoms)
            return;

        MartialArt m = Instantiate(ScriptableObject.CreateInstance<MartialArt>());
        customs.Add(m);
        SetEditing(customs.Count-1);
        UpdateUI();
    }

    public void DeleteCurrentCustom()
    {
        if (editing == null)
            return;

        customs.Remove(editing);
        SetEditing(customs.Count - 1);
        UpdateUI();
    }

    public void SetEditing(int i)
    {
        indexEditing = i;
        editing = customs[i];
        UpdateUI();
    }

    public void UpdateUI()
    {
        UpdateArtSlots();
        UpdateCurrentArt();
        UpdateMoveList();
    }

    public void RemoveMove(int i)
    {
        if (editing == null)
            return;

        i = Mathf.Clamp(i, 0, 4);

        editing.moveArray[i] = null;
        UpdateUI();
    }

    public void AddMove(Move m)
    {
        if (editing == null)
            return;

        for (int i = 0; i < editing.moveArray.Length; i++)
        {
            if (editing.moveArray[i] == null)
            {
                editing.moveArray[i] = m;
                UpdateCurrentArt();
                UpdateMoveList();
                return;
            }
        }
    }

    bool MoveAvailable(Move move)
    {
        if (editing == null)
            return true;

        for (int i = 0; i < editing.moveArray.Length; i++)
        {
            if (editing.moveArray[i] == move)
                return false;
        }

        return true;
    }

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> parent of dc1c306... test
    public void AnimateMove(Move m)
    {

    }

    void UpdateArtSlots()
    {
        int difference = customs.Count - instantiatedCustomSlots.Count;

        for (int i = 0; i < difference; i++)
        {
            GameObject go = Instantiate(customButtonPrefab, customParent);
            Button b = go.GetComponent<Button>();
            int f = instantiatedCustomSlots.Count + i;
            b.onClick.AddListener(delegate { SetEditing(f); });
            customButtons.Add(go, customs[f]);
            instantiatedCustomSlots.Add(go);
        }

        for (int j = 0; j < instantiatedCustomSlots.Count; j++)
        {
            instantiatedCustomSlots[j].GetComponentInChildren<TextMeshProUGUI>().text = (j+1).ToString();
            if (j == indexEditing)
                instantiatedCustomSlots[j].GetComponent<Image>().sprite = selectedCustom;
            else
                instantiatedCustomSlots[j].GetComponent<Image>().sprite = regularCustom;

        }

        addCustomButton.SetActive(instantiatedCustomSlots.Count < maxCustoms);
        addCustomButton.transform.SetAsLastSibling();
    }

    void UpdateCurrentArt()
    {
        for (int i = 0; i < editableMoves.Length; i++)
        {
            if (editing == null || editing.moveArray[i] == null)
            {
                editableMoves[i].GetComponentInChildren<TextMeshProUGUI>().text = "Empty";
            }
            else
            {
                editableMoves[i].GetComponentInChildren<TextMeshProUGUI>().text = editing.moveArray[i].name;
            }
        }

        //UpdateAnimations();
    }

    void UpdateAnimations()
<<<<<<< HEAD
=======
    void Animate()
>>>>>>> parent of c4c115a... 21.05
=======
    void Animate()
>>>>>>> parent of c4c115a... 21.05
=======
>>>>>>> parent of dc1c306... test
    {
        if (editing == null)
            return;

        AnimatorOverrideController overrideAnim = animator.runtimeAnimatorController as AnimatorOverrideController;
        RuntimeAnimatorController originalController = animator.runtimeAnimatorController;
        overrideAnim.runtimeAnimatorController = null;

        AnimatorOverrideController newOverride = new AnimatorOverrideController();
        newOverride.runtimeAnimatorController = originalController;

        newOverride["Move 1"] = (editing.moveArray[0] != null) ? editing.moveArray[0].animation : null;
        //overrideAnim["Move 2"] = (editing.moveArray[1] != null) ? editing.moveArray[1].animation : null;
        //overrideAnim["Move 3"] = (editing.moveArray[2] != null) ? editing.moveArray[2].animation : null;
        // overrideAnim["Move 4"] = (editing.moveArray[3] != null) ? editing.moveArray[3].animation : null;
        //overrideAnim["Move 5"] = (editing.moveArray[4] != null) ? editing.moveArray[4].animation : null;

        animator.runtimeAnimatorController = newOverride;
    }

    void UpdateMoveList()
    {
        foreach(MoveButton b in moveButtons)
        {
            b.GetComponent<Button>().interactable = MoveAvailable(b.move);
        }
    }
}
