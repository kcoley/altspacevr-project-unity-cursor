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
	private float rotationAngle = 35.0f;
	//The max rotation angle difference from the cursor angle
	private float maxRotAngleDiff = 2.0f;

	//Reference to the GlobalTransformModeObject

    //private reference to a selected gameobject
    private GameObject selectedGameObject;
    private Vector3 selectedGameObjectOffset;
	private Vector3 selectedGameObjectFloorOffset;

	private TransformModeHandler transformModeHandler;

	// smallest size for scaled game object
	public Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);


	// Rotation speed for rotating game object
	public float rotationSpeed = 10.0f;


    void Awake() {
		transformModeHandler = transform.Find ("TransformHandler").gameObject.GetComponent<TransformModeHandler>();
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

		// Rotate camera based on cursor
		if (transformModeHandler.transformMode == TransformModeHandler.TransformModes.Rotate && selectedGameObject != null) {	
			capCursorWidth (ray);
		} else {
			rotateCamera (ray);
		}
		capCursorHeight (ray);

        
		// Updated the selected game object
        if (selectedGameObject != null)
        {
            if (Input.GetMouseButtonUp(0))
            {
				// Release the selected object
				releaseSelectedObject ();
            }
            else // The selected object is still being held.  Handle translation, rotation and scale modes
            {
				switch (transformModeHandler.transformMode) 
				{
					case TransformModeHandler.TransformModes.Translate:
						bool didHitFloor = false;
						Vector3 floorIntersectionPoint = getFloorIntersectionPoint (ray, out didHitFloor);
						if (didHitFloor) {
							translateSelectedObject (floorIntersectionPoint);
						}
						break;
					case TransformModeHandler.TransformModes.Rotate:
						rotateSelectedObject (ray);
						break;
					case TransformModeHandler.TransformModes.Scale:
						scaleSelectedObject ();
						break;
				}
            }
        }
		// Update highlighted object based upon the raycast.
		else if (cursorHit.collider != null)
		{
			if (Input.GetMouseButtonUp (0)) {
				float geoScale = (cursorHit.distance * DistanceScaleFactor + 1.0f)/2.0f;
				// Scale the cursor size
				Cursor.transform.localScale = new Vector3(geoScale, geoScale, geoScale);
			}

			Selectable.CurrentSelection = cursorHit.collider.gameObject;



			// The user has selected an object.  Calculate the ground plane vector offset from the 
			// floor intersection point and the selected object.
            if (selectedGameObject == null && Input.GetMouseButtonDown(0))
            {
				selectObject(cursorHit.collider.gameObject, cursorHit.point);
				bool didHitFloor;
				Vector3 floorIntersectionPoint = getFloorIntersectionPoint (ray, out didHitFloor);
				if (didHitFloor) {

					Vector3 cursorHitGroundPoint = cursorHit.point;
					cursorHitGroundPoint.y = floorIntersectionPoint.y;
					selectedGameObjectFloorOffset = floorIntersectionPoint - cursorHitGroundPoint;
				} else {
					// TODO: Need to handle the case where the object is selected above the floor plane better.
				}
            }
		}
		else
		{
			Cursor.transform.localScale = DefaultCursorScale;
			Selectable.CurrentSelection = null;

			// Display Floor Sprite if ray hits the floor
			bool didHitFloor;
			Vector3 floorSpritePosition = getFloorIntersectionPoint(ray, out didHitFloor);
			if (didHitFloor) {
				FloorSprite.transform.position = floorSpritePosition;
			}
		}
	}

	void rotateCamera(Ray ray) {
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

	Vector3 getFloorIntersectionPoint(Ray ray, out bool didHitFloor) {
		/*
		 * Intersects with the ground plane and returns a bool indicating whether it hit the floor or not
		 */
		Vector3 floorIntersectionPoint = Vector3.zero;
		didHitFloor = false;
		var cursorHit = new RaycastHit ();

		if (Physics.Raycast (ray, out cursorHit, MaxDistance, FloorColliderMask)) {
			FloorSprite.GetComponent<Renderer> ().enabled = true;
			floorIntersectionPoint = cursorHit.point;
			didHitFloor = true;
		} 

		return floorIntersectionPoint;

	}

	void capCursorHeight(Ray ray) {
		/*
		 * This function caps the height of the cursor in screen space
		 * We will calculate a new ray direction within screen space
		 */

		Vector3 cursorPosition = Camera.main.WorldToViewportPoint(Cursor.transform.position);


		cursorPosition.y = Mathf.Clamp01 (cursorPosition.y);
		cursorPosition.x = Mathf.Clamp01 (cursorPosition.x);
		cursorPosition = Camera.main.ViewportToWorldPoint (cursorPosition);
		ray.direction = (cursorPosition - ray.origin).normalized;
		ray.GetPoint (SphereRadius);
		Cursor.transform.position = ray.GetPoint (SphereRadius);

	}
	void capCursorWidth(Ray ray) {
		/*
		 * This function caps the height of the cursor in screen space
		 */
		Vector3 cursorPosition = Camera.main.WorldToScreenPoint(Cursor.transform.position);

		cursorPosition.x = Mathf.Clamp (cursorPosition.x, 0, Screen.width);
		cursorPosition = Camera.main.ScreenToWorldPoint (cursorPosition);
		Cursor.transform.position = cursorPosition;

	}

	void releaseSelectedObject() {
		/*
		 * releases the selected game object
		 */
		selectedGameObject.transform.GetComponent<Rigidbody> ().freezeRotation = false;
		selectedGameObject = null;
	}

	void selectObject(GameObject obj, Vector3 hitPoint) {
		/*
		 * Grabs the selected object
		 */

		//Calculate the scalar project to get the horizontal distance to the object
		selectedGameObject = obj;
		selectedGameObjectOffset = hitPoint - selectedGameObject.transform.position;
		selectedGameObject.transform.GetComponent<Rigidbody> ().freezeRotation = true;

	}
		
	void translateSelectedObject(Vector3 floorIntersectionPoint) {
		/*
		 * Translates the game object based on a floor intersection point and an vector offset, plus project upwards from the ground plane
		 */
		selectedGameObjectOffset.y = selectedGameObject.GetComponent<BoxCollider> ().bounds.size.y;
		Vector3 selectedGameObjectHeight = new Vector3 (0, selectedGameObject.GetComponent<BoxCollider> ().bounds.size.y, 0);
		selectedGameObject.transform.position = floorIntersectionPoint - selectedGameObjectFloorOffset + selectedGameObjectHeight;
		FloorSprite.transform.position = floorIntersectionPoint - selectedGameObjectFloorOffset;

	}

	void rotateSelectedObject(Ray ray) {
		/*
		 * Rotates the game object based on the mouse x and mouse y world space positions
		 */
		capCursorWidth (ray);
		float xRotation = Input.GetAxis ("Mouse X") * rotationSpeed * Mathf.Deg2Rad;
		float yRotation = Input.GetAxis ("Mouse Y") * rotationSpeed * Mathf.Deg2Rad;

		selectedGameObject.transform.Rotate(Vector3.up, -xRotation, Space.World);
		selectedGameObject.transform.Rotate(Vector3.right, yRotation, Space.World);

	}

	void scaleSelectedObject() {
		/*
		 * Scale Game object based on the mouse x axis value
		 * The minimum size is set with the public minScale Vector
		 */
		Vector3 s = selectedGameObject.transform.localScale;
		float scaleValue = -Input.GetAxis ("Mouse X");

		Vector3 scaleDifference = new Vector3 (scaleValue, scaleValue, scaleValue);
		Vector3 newScale = s - scaleDifference;
		if (newScale.x >= minScale.x && newScale.y >= minScale.y && newScale.z >= minScale.z) {
			selectedGameObject.transform.localScale = newScale;
		}


	}
}
