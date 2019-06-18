using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitBoxVisual : MonoBehaviour
{
    Collider toCopy;
    MeshRenderer renderer;
    public Toggle toggle;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<MeshRenderer>();
        toCopy = GetComponentInParent<Collider>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (toCopy == null)
            return;

        if (toggle == null)
            renderer.enabled = toCopy.enabled;
        else
            renderer.enabled = toggle.isOn && toCopy.enabled;


        if (toCopy is SphereCollider)
        {
            float scale = (toCopy as SphereCollider).radius * 2;
            Vector3 offset = (toCopy as SphereCollider).center;

            transform.localScale = new Vector3(scale, scale, scale);
            transform.localPosition = offset;
        }

        else if (toCopy is BoxCollider)
        {
            Vector3 size = (toCopy as BoxCollider).size;
            Vector3 offset = (toCopy as BoxCollider).center;

            transform.localScale = new Vector3(size.x, size.y, size.z);
            transform.localEulerAngles = offset;
        }

    }

}
