using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualInput : MonoBehaviour
{
    public bool localPlayer;

    public float horizontal = 0f;
    public float vertical = 0f;

    public bool space, spaceDown, spaceUp, lShift, lShiftDown, lShiftUp, rmb, rmbDown, rmbUp, lmb, lmbDown, lmbUp, e, eDown, eUp, q, qDown, qUp, f, fDown, fUp,
        w, wDown, a, aDown, s, sDown, d, dDown;

    void Update()
    {
        if(localPlayer)
        {
            //Axis
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            //Direction Buttons
            w = Input.GetKey(KeyCode.W);
            a = Input.GetKey(KeyCode.A);
            s = Input.GetKey(KeyCode.S);
            d = Input.GetKey(KeyCode.D);

            wDown = Input.GetKeyDown(KeyCode.W);
            aDown = Input.GetKeyDown(KeyCode.A);
            sDown = Input.GetKeyDown(KeyCode.S);
            dDown = Input.GetKeyDown(KeyCode.D);
            /*
            wUp = Input.GetKeyDown(KeyCode.W);
            aUp = Input.GetKeyDown(KeyCode.A);
            sUp = Input.GetKeyDown(KeyCode.S);
            dUp = Input.GetKeyDown(KeyCode.D);*/

            //Other Buttons
            space = Input.GetKey(KeyCode.Space);
            spaceDown = Input.GetKeyDown(KeyCode.Space);
            spaceUp = Input.GetKeyUp(KeyCode.Space);

            lShift = Input.GetKey(KeyCode.LeftShift);
            lShiftDown = Input.GetKeyDown(KeyCode.LeftShift);
            lShiftUp = Input.GetKeyUp(KeyCode.LeftShift);

            e = Input.GetKey(KeyCode.E);
            eDown = Input.GetKeyDown(KeyCode.E);
            eUp = Input.GetKeyUp(KeyCode.E);

            q = Input.GetKey(KeyCode.Q);
            qDown = Input.GetKeyDown(KeyCode.Q);
            qUp = Input.GetKeyUp(KeyCode.Q);

            f = Input.GetKey(KeyCode.F);
            fDown = Input.GetKeyDown(KeyCode.F);
            fUp = Input.GetKeyUp(KeyCode.F);


            //Mouse
            lmb = Input.GetMouseButton(0);
            lmbDown = Input.GetMouseButtonDown(0);
            lmbUp = Input.GetMouseButtonUp(0);
            rmb = Input.GetMouseButton(1);
            rmbDown = Input.GetMouseButtonDown(1);
            rmbUp = Input.GetMouseButtonUp(1);
        }
    }
}
