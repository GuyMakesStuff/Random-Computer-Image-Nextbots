using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
public class TextPopUp : MonoBehaviour
{
    public bool UsePhysics;
    public float UpDirection;
    public float SidewaysDirection;
    public float LastTime;
    float T;
    Rigidbody Body;
    TMP_Text Txt;
    Vector3 StartPos;
    Vector3 EndPos;

    // Start is called before the first frame update
    void Start()
    {
        Txt = GetComponent<TextMeshPro>();

        T = LastTime;

        if(UsePhysics)
        {
            Body = gameObject.AddComponent<Rigidbody>();
            Body.constraints = RigidbodyConstraints.FreezeRotation;

            Vector2 SideForce = Random.insideUnitCircle * SidewaysDirection;
            Body.AddForce(SideForce.x * 100f, UpDirection * 100f, SideForce.y * 100f);
        }
        else
        {
            StartPos = transform.position;
            EndPos = StartPos + (Vector3.up * UpDirection);
        }
    }

    // Update is called once per frame
    void Update()
    {
        T -= Time.deltaTime;
        Txt.alpha = T / LastTime;

        if(!UsePhysics)
        {
            transform.position = Vector3.Lerp(StartPos, EndPos, 1f - (T / LastTime));
        }

        if(T <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
