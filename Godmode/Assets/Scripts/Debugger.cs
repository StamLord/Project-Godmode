using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Debugger : MonoBehaviour
{
    public GameObject debugCanvas;
    public InputField timeScale;

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

    void SetTimeScale(InputField input)
    {
        Time.timeScale = float.Parse(input.text);
    }

    
}
