using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaCreator : MonoBehaviour
{
    public float ArenaRadius;
    public GameObject FencePrefab;
    public Material ArenaFloorMaterial;
    public float ArenaTileScale;
    public float FenceGaps;
    public float FenceMargin;

    // Start is called before the first frame update
    void Start()
    {
        // Arena Floor.
        GameObject ArenaFloor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ArenaFloor.name = "Arena Floor";
        ArenaFloor.tag = "Floor";
        ArenaFloor.transform.SetParent(transform);
        ArenaFloor.transform.position = Vector3.zero;
        ArenaFloor.transform.localScale = new Vector3(ArenaRadius * 2f, 0.5f, ArenaRadius * 2f);
        ArenaFloor.GetComponent<CapsuleCollider>().enabled = false;
        ArenaFloor.AddComponent<MeshCollider>().sharedMesh = ArenaFloor.GetComponent<MeshFilter>().sharedMesh;
        ArenaFloor.layer = 3;
        Vector2 TextureScale = Vector2.one * (ArenaRadius / ArenaTileScale);
        ArenaFloorMaterial.SetTextureScale("_MainTex", TextureScale);
        ArenaFloorMaterial.SetTextureScale("_DetailAlbedoMap", TextureScale);
        ArenaFloor.GetComponent<MeshRenderer>().sharedMaterial = ArenaFloorMaterial;

        // Arena Fences Container.
        Transform FencesContainer = new GameObject("Fences").transform;
        FencesContainer.SetParent(transform);
        FencesContainer.position = Vector3.zero;

        // Arena Fences.
        float ArenaScope = (ArenaRadius * 2f) * Mathf.PI;
        int FenceCount = (int)(ArenaScope / FenceGaps);
        float AngleGaps = (360f / FenceCount);
        for (int F = 0; F < FenceCount; F++)
        {
            float Angle = AngleGaps * F;
            Vector2 OnUnitCircle = UnitCircle(Angle);
            Instantiate(FencePrefab, new Vector3(OnUnitCircle.x * (ArenaRadius - FenceMargin), 1.5f, OnUnitCircle.y * (ArenaRadius - FenceMargin)), Quaternion.Euler(-90f, 0f, Angle), FencesContainer).name = "Fence_" + (F + 1).ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static Vector2 UnitCircle(float Angle)
    {
        return new Vector2(Mathf.Sin(Angle * Mathf.Deg2Rad), Mathf.Cos(Angle * Mathf.Deg2Rad));
    }
}
