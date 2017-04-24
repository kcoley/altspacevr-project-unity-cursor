using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportModule : MonoBehaviour {
	// This is the Cursor game object. 
	private GameObject Cursor;

	// Maximum distance to ray cast.
	private const float MaxDistance = 100.0f;

	// This is the layer mask to use when performing the ray cast for the objects.
	// The furniture in the room is in layer 8, everything else is not.
	private const int FurnitureColliderMask = (1 << 8);

	// This layer mask is for the floor
	private const int FloorColliderMask = (1 << 10);

	// Use this for initialization
	void Awake () {
		Cursor = transform.Find("Cursor").gameObject;
		
	}

	void Update () {
		Vector3 origin = Camera.main.transform.position;
		Vector3 direction = (Cursor.transform.position - Camera.main.transform.position).normalized;

		Ray ray = new Ray (origin, direction);
		var cursorHit = new RaycastHit();/* Your cursor hit code should set this properly. */;
		if (Input.GetMouseButtonDown(0)) {
			if (!Physics.Raycast (ray, out cursorHit, MaxDistance, FurnitureColliderMask)) {
				if (Physics.Raycast (ray, out cursorHit, MaxDistance, FloorColliderMask)) {
					Camera.main.transform.position = new Vector3 (cursorHit.point.x, origin.y, cursorHit.point.z);
				}
			}
		}
	}
}
