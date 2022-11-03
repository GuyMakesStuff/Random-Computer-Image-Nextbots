using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    Camera cam;
    public bool Inverse;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 CamDir = cam.transform.forward;
        if(Inverse) { CamDir *= -1f; }
        transform.LookAt(transform.position + CamDir);
    }
}
