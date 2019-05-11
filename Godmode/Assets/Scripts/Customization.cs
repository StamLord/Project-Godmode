using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customization : MonoBehaviour
{
    //public SkinnedMeshRenderer head;
    //public SkinnedMeshRenderer[] heads;

    //public void ChangeHead(int index)
    //{
    //    head.sharedMesh = heads[index].sharedMesh;
    //}
    [Header("Camera")]
    public Transform cam;
    public float zoomFraction = 0.5f;
    public float minCamDist = -3f;
    public float maxCamDist = -0.5f;
    private Vector3 camOrigin;

    [Header("Player")]
    public Transform player;
    public float rotationY = -180;
    public float ySensitivity = 4f;

    public GameObject[] heads;
    private int currentHead;

    public SkinnedMeshRenderer body;
    public Material[] materials;
    private int currentMaterial;

    public GameObject cape;
    bool capeOn;

    public Material[] capeMaterials;
    private int currentCapeMaterial;

    public ParticleSystem charge;
    public Color[] chargeColors;
    public int currentCharge;

    public void SwitchHead()
    {
        currentHead++;
        currentHead %= heads.Length;

        for (int i = 0; i < heads.Length; i++)
        {
            if (i == currentHead && heads[i])
                heads[i].SetActive(true);
            else if (heads[i])
                heads[i].SetActive(false);
        }
    }

    public void SwitchMaterials()
    {
        currentMaterial++;
        currentMaterial %= materials.Length;

        body.material = materials[currentMaterial];
    }

    public void EnableCape()
    {
        capeOn = !capeOn;
        cape.SetActive(capeOn);
    }

    public void SwitchCapeMaterial()
    {
        currentCapeMaterial++;
        currentCapeMaterial %= capeMaterials.Length;

        cape.GetComponent<MeshRenderer>().material = capeMaterials[currentCapeMaterial];
    }

    public void SwitchCharge()
    {
        currentCharge++;
        currentCharge %= chargeColors.Length;

        charge.startColor = chargeColors[currentCharge];
        charge.Play();
    }

    private void Start()
    {
        camOrigin = cam.position;
    }

    private void Update()
    {
        if(Input.GetMouseButton(1))
        {
            rotationY -= Input.GetAxis("Mouse X") * ySensitivity;
        }

        cam.position += new Vector3(0, 0, Input.mouseScrollDelta.y) * zoomFraction;
        cam.position = new Vector3(cam.position.x, cam.position.y, Mathf.Clamp(cam.position.z, minCamDist, maxCamDist));

        Vector3 euler = player.rotation.eulerAngles;
        euler.y = rotationY;
        player.rotation = Quaternion.Euler(euler);
    }
}

