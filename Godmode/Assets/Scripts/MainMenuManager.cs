using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public Animator camAnimator;

    public void ChangeScreen(int screen)
    {
        camAnimator.SetInteger("Screen", screen);
    }
}
