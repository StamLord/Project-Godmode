using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if false

public class AnimationScript : MonoBehaviour
{
    public Animator anim;
    public CharController charCon;

    private void Start()
    {
        charCon = GetComponent<CharController>(); 
    }

    public void FreezeAnimation()
    {
        anim.speed = 0f;
    }

    public void ReleaseAnimation()
    {
        anim.speed = 1f;
    }

    public void CheckCombo()
    {
        int cc = charCon.punchNum;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Punch1") && cc == 1)
        {
            //Debug.Log("Reset");
            anim.SetInteger("Combo", 0);
            charCon.punchNum = 0;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Punch1") && cc >= 2)
        {
            //Debug.Log("1 > 2");
            anim.SetInteger("Combo", 2);
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Punch2") && cc == 2)
        {
            //Debug.Log("2 > 0");
            anim.SetInteger("Combo", 0);
            charCon.punchNum = 0;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Punch2") && cc >= 3)
        {
            //Debug.Log("2 > 3");
            anim.SetInteger("Combo", 3);
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Punch3") && cc == 3)
        {
            //Debug.Log("3 > 0");
            anim.SetInteger("Combo", 0);
            charCon.punchNum = 0;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Punch3") && cc >= 4)
        {
            //Debug.Log("3 > 4");
            anim.SetInteger("Combo", 4);
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Punch4"))
        {
           // Debug.Log("4 > 0");
            anim.SetInteger("Combo", 0);
            charCon.punchNum = 0;
        }
    }

}

#endif