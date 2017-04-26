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

	// The threshold rotation angle
	private float rotationAngle = 45.0f;
	//The max rotation angle difference from the cursor angle
	private float maxRotAngleDiff = 2.0f;

    //private reference to a selected gameobject
    private GameObject selectedGameObject;
    private float selectedGameObjectRadius;
    private Vector3 selectedGameObjectOffset;




    void Awake() {
		Cursor = transform.Find("Cursor").gameObject;
		CursorMeshRenderer = Cursor.transform.GetComponentInChildren<MeshRenderer>();
        CursorMeshRenderer.GetComponent<Renderer>().material.color = new Color(0.0f, 0.8f, 1.0f);

		Vector3 mousePosition = Cursor.transform.position;
		mousePosition.z = SphereRadius;
		Cursor.transform.position = mousePosition;

        selectedGameObject = null;

    }	

	void Update()
	{
		// Handle mouse movement to update cursor position.
		FloorSprite.GetComponent<Renderer> ().enabled = false;

		Cursor.transform.RotateAround (Camera.main.transform.position, transform.up, Input.GetAxis ("Mouse X") * Sensitivity);
		Cursor.transform.RotateAround (Camera.main.transform.position, -transform.right, Input.GetAxis ("Mouse Y") * Sensitivity);

		// Perform ray cast to find object cursor is pointing at.
		Vector3 origin = Camera.main.transform.position;
		Vector3 direction = (Cursor.transform.position - Camera.main.transform.position).normalized;

		Ray ray = new Ray (origin, direction);

		var cursorHit = new RaycastHit();/* Your cursor hit code should set this properly. */;
		Physics.Raycast (ray, out cursorHit, MaxDistance, FurnitureColliderMask);

		// Rotate player based on cursor
		rotatePlayer(ray);
		capCursorHeight (ray);

        // Update highlighted object based upon the raycast.
        if (selectedGameObject != null)
        {
            if (Input.GetMouseButtonUp(0))
            {
                selectedGameObject = null;
                Debug.Log("Released game object");
            }
            else
            {
                selectedGameObject.transform.position = ray.GetPoint(selectedGameObjectRadius) - selectedGameObjectOffset;
            }
        }
        else if (cursorHit.collider != null)
		{
			Selectable.CurrentSelection = cursorHit.collider.gameObject;
			float geoScale = (cursorHit.distance * DistanceScaleFactor + 1.0f)/2.0f;
			Cursor.transform.localScale = new Vector3(geoScale, geoScale, geoScale);

            if (selectedGameObject == null && Input.GetMouseButtonDown(0))
            {
                //Calculate the scalar project to get the horizontal distance to the object
                selectedGameObjectRadius = Vector3.Project(cursorHit.point - ray.origin, transform.forward).magnitude;
                selectedGameObject = cursorHit.collider.gameObject;
                selectedGameObjectOffset = cursorHit.point - selectedGameObject.transform.position;
                Debug.Log("Selected game object");
         
            }
		}

		else
		{
			Selectable.CurrentSelection = null;
			Cursor.transform.localScale = DefaultCursorScale;

			// Display Floor Sprite if ray hits the floor
			if (Physics.Raycast (ray, out cursorHit, MaxDistance, FloorColliderMask)) {
				FloorSprite.GetComponent<Renderer> ().enabled = true;
				Vector3 floorSpritePosition = cursorHit.point;
				FloorSprite.transform.position = floorSpritePosition;
			} 
		}
	}

	void rotatePlayer(Ray ray) {
		/*
		 * Rotates the camera based on the cursor's distance to the edge of the screen.
		 * The rotation speed is limited by the maxRotAngleDiff, which is the maximum allowed angle from
		 * the transform forward direction and the cursor ray.  If too great, the cursor rotation
		 * will get corrected to the max angle.
		 *
		 */
		if (Vector3.Angle (ray.direction, transform.forward) > rotationAngle) {
			float angleDiff = Vector3.Angle (ray.direction, transform.forward) - rotationAngle;
			float sign = 1.0f;

			if (Vector3.Dot (ray.direction, transform.right) < 0) {
				sign = -1.0f;
			}
			if (angleDiff > maxRotAngleDiff) {
				Cursor.transform.RotateAround (Camera.main.transform.position, transform.up, -sign * (angleDiff - maxRotAngleDiff));
				angleDiff = maxRotAngleDiff;
			}
			Camera.main.transform.RotateAround(Camera.main.transform.position, Vector3.up, sign * angleDiff);
		}
	}

	void capCursorHeight(Ray ray) {
		/*
		 * This function caps the height of the cursor in screen space
		 */
		Vector3 cursorPosition = Camera.main.WorldToScreenPoint(Cursor.transform.position);

		cursorPosition.y = Mathf.Clamp (cursorPosition.y, 0, Screen.height);
		cursorPosition = Camera.main.ScreenToWorldPoint (cursorPosition);
		Cursor.transform.position = cursorPosition;

	}
}
