using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Console : MonoBehaviour
{
    public Image console;
    bool consoleOn;

    private List<GameObject> listeners = new List<GameObject>();

    public void AddListener(GameObject listener)
    {
        if(!listeners.Contains(listener))
            listeners.Add(listener);
    }
    // Start is called before the first frame update
    void Start()
    {
        AddListener(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tilde))
        {
            consoleOn = !consoleOn;
            console.enabled = consoleOn;
        }

    }

    public void Command(string input)
    {
        string[] parts = input.Split(new char[] { '.', '(', ')' }, 3);
        GameObject go = listeners.Where(obj => obj.name == parts[0]).SingleOrDefault();
        if(go)
        {
            go.SendMessage(parts[1], parts[2]);
        }
    }
}
