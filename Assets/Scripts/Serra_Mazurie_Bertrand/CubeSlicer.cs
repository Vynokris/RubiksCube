using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSlicer : MonoBehaviour
{
    private CubeCreator cubeCreator;
    
    public int MaskSubCubePos(Vector3 pos, Vector3 mask)
    {
        float result;

        if      (mask.x != 0) result = pos.x / cubeCreator.transform.localScale.x;
        else if (mask.y != 0) result = pos.y / cubeCreator.transform.localScale.y;
        else                  result = pos.z / cubeCreator.transform.localScale.z;

        result += cubeCreator.size / 2;
        if (cubeCreator.size % 2 == 0)
            result += 0.5f;

        return Mathf.RoundToInt(result);
    }

    public List<GameObject> GetSlice(Vector3 normal, int index)
    {
        Vector3 indexAlongNormal = normal * (index - cubeCreator.size / 2) * cubeCreator.transform.localScale.x;

        List<GameObject> subCubes = new List<GameObject>();
        foreach (Transform child in transform.GetChild(0))
            if (MaskSubCubePos(child.position, normal) == index)
                subCubes.Add(child.gameObject);
        return subCubes;
    }

    void Awake()
    {
        cubeCreator = gameObject.GetComponent<CubeCreator>();
    }
}
