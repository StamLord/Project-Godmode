using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitBoxVisual : MonoBehaviour
{
    Collider toCopy;
    MeshRenderer renderer;
    Toggle toggle;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<MeshRenderer>();
        toCopy = GetComponentInParent<Collider>();
        toggle = GameObject.Find("HitBoxToggle").GetComponent<Toggle>();
        //GameObject.FindObjectOfType<MartialArtBuilder>().
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (toggle == null)
            return;

        renderer.enabled = toggle.isOn && toCopy.enabled;

        if (toCopy == null)
            return;

        if (toCopy is SphereCollider)
        {
            float scale = (toCopy as SphereCollider).radius * 2;
            Vector3 offset = (toCopy as SphereCollider).center;

            transform.localScale = new Vector3(scale, scale, scale);
            transform.localPosition = offset;
        }

    }

}
