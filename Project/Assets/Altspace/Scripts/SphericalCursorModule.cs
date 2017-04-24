using UnityEngine;

public class SphericalCursorModule : MonoBehaviour {
	// This is a sensitivity parameter that should adjust how sensitive the mouse control is.
	public float Sensitivity;

	// This is a scale factor that determines how much to scale down the cursor based on its collision distance.
	public float DistanceScaleFactor;
	
	// This is the layer mask to use when performing the ray cast for the objects.
	// The furniture in the room is in layer 8, everything else is not.
	private const int FurnitureColliderMask = (1 << 8);

	private const int FloorColliderMask = (1 << 10);

	// This is the Cursor game object. Your job is to update its transform on each frame.
	private GameObject Cursor;

	// This is the Cursor mesh. (The sphere.)
	private MeshRenderer CursorMeshRenderer;

	// This is the scale to set the cursor to if no ray hit is found.
	private Vector3 DefaultCursorScale = new Vector3(7.0f, 7.0f, 7.0f);

	// Maximum distance to ray cast.
	private const float MaxDistance = 100.0f;

	// Sphere radius to project cursor onto if no raycast hit.
	private const float SphereRadius = 12.0f;

	// floor sprite
	public GameObject FloorSprite;



    void Awake() {
		Cursor = transform.Find("Cursor").gameObject;
		CursorMeshRenderer = Cursor.transform.GetComponentInChildren<MeshRenderer>();
        CursorMeshRenderer.GetComponent<Renderer>().material.color = new Color(0.0f, 0.8f, 1.0f);

		Vector3 mousePosition = Cursor.transform.position;
		mousePosition.z = SphereRadius;
		Cursor.transform.position = mousePosition;

    }	

	void Update()
	{
		// TODO: Handle mouse movement to update cursor position.
		FloorSprite.GetComponent<Renderer> ().enabled = false;

		Cursor.transform.RotateAround (Camera.main.transform.position, Vector3.up, Input.GetAxis ("Mouse X") * Sensitivity);
		Cursor.transform.RotateAround (Camera.main.transform.position, Vector3.left, Input.GetAxis ("Mouse Y") * Sensitivity);

		// TODO: Perform ray cast to find object cursor is pointing at.
		Vector3 origin = Camera.main.transform.position;
		Vector3 direction = (Cursor.transform.position - Camera.main.transform.position).normalized;

		Ray ray = new Ray (origin, direction);

		
		// TODO: Update cursor transform.
		var cursorHit = new RaycastHit();/* Your cursor hit code should set this properly. */;
		Physics.Raycast (ray, out cursorHit, MaxDistance, FurnitureColliderMask);
	

		// Update highlighted object based upon the raycast.
		if (cursorHit.collider != null)
		{
			Selectable.CurrentSelection = cursorHit.collider.gameObject;
			float geoScale = (cursorHit.distance * DistanceScaleFactor + 1.0f)/2.0f;
			Cursor.transform.localScale = new Vector3(geoScale, geoScale, geoScale);

		}
		else
		{
			Selectable.CurrentSelection = null;
			Cursor.transform.localScale = DefaultCursorScale;
			if (Physics.Raycast (ray, out cursorHit, MaxDistance, FloorColliderMask)) {
				FloorSprite.GetComponent<Renderer> ().enabled = true;
				Vector3 floorSpritePosition = cursorHit.point;
				FloorSprite.transform.position = floorSpritePosition;
			} 

		}
	}
}
