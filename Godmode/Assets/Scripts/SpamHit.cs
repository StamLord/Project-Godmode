using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpamHit : MonoBehaviour
{
    VirtualInput vi;
    public float timer;

    // Start is called before the first frame update
    void Start()
    {
        vi = GetComponent<VirtualInput>();
    }

    // Update is called once per frame
    void Update()
    {
        if(vi.lmbUp == true)
            vi.lmbUp = false;

        if (vi.lmb == false)
        {
            vi.lmbDown = true;
            vi.lmb = true;
        }
        else
            vi.lmbDown = false;

        timer += Time.deltaTime;

        if (timer >= 0.7f)
        {
            vi.lmbUp = true;
            vi.lmb = false;
            timer = 0;
        }
    }
}
