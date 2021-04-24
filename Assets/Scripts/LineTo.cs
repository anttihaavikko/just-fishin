using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineTo : MonoBehaviour
{
    public LineRenderer line;
    public Transform target;

    private void Update()
    {
        var pos = transform.position;
        var tPos = target.position;
        var mid = (pos + tPos) * 0.5f + Vector3.down * 0.3f;
        line.SetPosition(0, pos);
        line.SetPosition(1, mid);
        line.SetPosition(2, tPos);
    }
}
