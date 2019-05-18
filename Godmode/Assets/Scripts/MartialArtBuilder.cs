using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MartialArtBuilder : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform customParent;
    public Transform moveParent;

    public GameObject customButtonPrefab;
    public GameObject moveButtonPrefab;

    protected Dictionary<GameObject, Move> moveButtons;
    protected Dictionary<GameObject, MartialArt> customButtons;

    [Header("Database")]
    public List<Move> allMoves = new List<Move>();
    public List<MartialArt> customs = new List<MartialArt>();
    public int maxCustoms = 3;

    protected MartialArt editing;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        foreach (MartialArt c in customs)
        {
            GameObject go = Instantiate(customButtonPrefab, customParent);
            customButtons.Add(go, c);
        }

        foreach (Move m in allMoves)
        {
            GameObject go = Instantiate(moveButtonPrefab, moveParent);
            moveButtons.Add(go, m);
        }

        UpdateUI();
    }

    public void CreateNewCustom()
    {
        if (customs.Count >= maxCustoms)
            return;

        customs.Add(new MartialArt());
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
        editing = customs[i];
        UpdateUI();
    }

    public void UpdateUI()
    {

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

    void Animate()
    {

    }
}
