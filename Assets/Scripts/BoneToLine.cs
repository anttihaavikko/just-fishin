using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BoneToLine : MonoBehaviour {

	public bool bezier = false;
	public Anima2D.Bone2D[] bones;
	private LineRenderer line;
	
	private void Start () {
		line = GetComponent<LineRenderer> ();
	}
	
	private void Update () {

		line.SetPosition (0, transform.position);

		if (bezier) {
			for (var i = 1; i < line.positionCount; i++) {
				// B(t) = (1-t)^2P0 + 2(1-t)tP1 + t2P2 , 0 < t < 1

				var t = (float)i / (float)line.positionCount;
				var p = Mathf.Pow (1 - t, 2) * transform.position + 2 * (1 - t) * t * bones [0].endPosition + Mathf.Pow (t, 2) * bones [1].endPosition;
				line.SetPosition (i, p);
			}
		} else {
			for (var i = 0; i < bones.Length; i++) {
				if (bones.Length >= i)
				{
					line.SetPosition (i + 1, bones[i].endPosition);	
				}
			}
		}
	}
}
