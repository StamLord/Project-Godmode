using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Debugger : MonoBehaviour
{
    public GameObject debugCanvas;
    public TMP_InputField timeScale;

    // Start is called before the first frame update
    void Start()
    {
        timeScale.onEndEdit.AddListener(delegate { SetTimeScale(timeScale); });
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F12))
        {
            debugCanvas.SetActive(!debugCanvas.activeSelf);
            if (debugCanvas.activeSelf)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void SetTimeScale(TMP_InputField input)
    {
        Time.timeScale = float.Parse(input.text);
    }

    
}
