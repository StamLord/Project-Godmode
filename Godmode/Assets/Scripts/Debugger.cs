using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Debugger : MonoBehaviour
{
    public GameObject hud;
    public GameObject debugCanvas;
    public GameObject info;
    public TMP_InputField timeScale;

    void Start()
    {
        timeScale.onEndEdit.AddListener(delegate { SetTimeScale(timeScale); });
    }

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

        if(Input.GetKeyDown(KeyCode.F1))
        {
            info.SetActive(!info.activeSelf);
            hud.SetActive(!info.activeSelf);
        }
    }

    void SetTimeScale(TMP_InputField input)
    {
        Time.timeScale = float.Parse(input.text);
    }

    
}
