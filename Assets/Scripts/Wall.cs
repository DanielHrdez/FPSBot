using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    BoxCollider boxCollider;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    public float[] BoxColliderBounds2D()
    {
        // Get the bounds of a box collider
        Vector3 center = transform.TransformPoint(boxCollider.center);
        Vector3 size = transform.TransformVector(boxCollider.size);
        Vector3 rotation = transform.eulerAngles;
        float xMin = center.x - size.x / 2;
        float xMax = center.x + size.x / 2;
        float yMin = center.z - size.z / 2;
        float yMax = center.z + size.z / 2;
        // return new float[] { xMin, xMax, yMin, yMax };
        // rotate
        float angle = rotation.y * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        float xMinRot = xMin * cos - yMin * sin;
        float xMaxRot = xMax * cos - yMax * sin;
        float yMinRot = xMin * sin + yMin * cos;
        float yMaxRot = xMax * sin + yMax * cos;
        return new float[] { xMinRot, xMaxRot, yMinRot, yMaxRot };
    }
}
