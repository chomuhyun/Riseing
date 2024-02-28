using UnityEngine;
using Cinemachine;
//using Cinemachine;

// This class corresponds to the 3rd person camera features.
public class CamCtrl : MonoBehaviour
{
	private Vector3 lastMousePosition;
	public bool isFixed; // 알트 누르면 고정


	public Transform player;                                           // Player's reference.
	public Vector3 pivotOffset = new Vector3(0.0f, 1.7f, 0.0f);       // Offset to repoint the camera.
	public Vector3 camOffset = new Vector3(0.0f, 0.0f, -3.0f);       // Offset to relocate the camera related to the player position.
	public float smooth = 10f;                                         // Speed of camera responsiveness.
	public float horizontalAimingSpeed = 6f;                           // Horizontal turn speed.
	public float verticalAimingSpeed = 6f;                             // Vertical turn speed.
	public float maxVerticalAngle = 30f;                               // Camera max clamp angle. 
	public float minVerticalAngle = -60f;                              // Camera min clamp angle.
	public string XAxis = "Analog X";                                  // The default horizontal axis input name.
	public string YAxis = "Analog Y";                                  // The default vertical axis input name.

	private float angleH = 0;                                          // Float to store camera horizontal angle related to mouse movement.
	private float angleV = 0;                                          // Float to store camera vertical angle related to mouse movement.
	public Transform cam;                                             // This transform.
	public Vector3 smoothPivotOffset;                                 // Camera current pivot offset on interpolation.
	public Vector3 smoothCamOffset;                                   // Camera current offset on interpolation.
	public Vector3 targetPivotOffset;                                 // Camera pivot offset target to interpolate.
	public Vector3 targetCamOffset;                                   // Camera offset target to interpolate.
	private float defaultFOV;                                          // Default camera Field of View.
	private float targetFOV;                                           // Target camera Field of View.
	private float targetMaxVerticalAngle;                              // Custom camera max vertical clamp angle.
	private bool isCustomOffset;                                       // Boolean to determine whether or not a custom camera offset is being used.

	// Get the camera horizontal angle.
	public float GetH => angleH;
	//public CinemachineVirtualCamera thiscam;
	public float zoomSpeed = 20f;
	public float scrollSpeed = 1.2f;

	private void Start()
	{

	}
	public void Starts()
	{
		if (player != null)
		{
			cam = transform;

			cam.position = player.position + Quaternion.identity * pivotOffset + Quaternion.identity * camOffset;
			cam.rotation = Quaternion.identity;

			smoothPivotOffset = pivotOffset;
			smoothCamOffset = camOffset;
			angleH = player.eulerAngles.y;

			ResetTargetOffsets();
			ResetFOV();
			ResetMaxVerticalAngle();

			if (camOffset.y > 0)
				Debug.LogWarning("Vertical Cam Offset (Y) will be ignored during collisions!\n" +
					"It is recommended to set all vertical offset in Pivot Offset.");
		}
	}

	void Update()
	{

		float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

		if (Input.GetKey(KeyCode.LeftAlt) || isFixed)
		{
			Cursor.visible = true;
			Vector3 deltaMouse = Input.mousePosition - lastMousePosition;
			lastMousePosition = Input.mousePosition;
			return;
		}
		angleH += Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1) * horizontalAimingSpeed;
		angleV += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1) * verticalAimingSpeed;
	
		angleH += Mathf.Clamp(Input.GetAxis(XAxis), -1, 10) * 60 * horizontalAimingSpeed * Time.deltaTime;
		angleV += Mathf.Clamp(Input.GetAxis(YAxis), -1, 10) * 60 * verticalAimingSpeed * Time.deltaTime;

		angleV = Mathf.Clamp(angleV, minVerticalAngle, targetMaxVerticalAngle);

		Quaternion camYRotation = Quaternion.Euler(15, angleH, 0);
		Quaternion aimRotation = Quaternion.Euler(-angleV, angleH, 0);
		cam.rotation = aimRotation;

		if (player != null)
		{

			Vector3 baseTempPosition = player.position + camYRotation * targetPivotOffset;
			Vector3 noCollisionOffset = targetCamOffset;
			while (noCollisionOffset.magnitude >= 0.2f)
			{
				if (DoubleViewingPosCheck(baseTempPosition + aimRotation * noCollisionOffset))
					break;
				noCollisionOffset -= noCollisionOffset.normalized * 0.2f;
			}
			if (noCollisionOffset.magnitude < 0.2f)
				noCollisionOffset = Vector3.zero;

			bool customOffsetCollision = isCustomOffset && noCollisionOffset.sqrMagnitude < targetCamOffset.sqrMagnitude;

			// Reposition the camera.
			smoothPivotOffset = Vector3.Lerp(smoothPivotOffset, customOffsetCollision ? pivotOffset : targetPivotOffset, smooth * Time.deltaTime);
			smoothCamOffset = Vector3.Lerp(smoothCamOffset, customOffsetCollision ? Vector3.zero : noCollisionOffset, smooth * Time.deltaTime);

			cam.position = player.position + camYRotation * smoothPivotOffset + aimRotation * smoothCamOffset;
		}
	}

	public void SetTargetOffsets(Vector3 newPivotOffset, Vector3 newCamOffset)
	{
		targetPivotOffset = newPivotOffset;
		targetCamOffset = newCamOffset;
		isCustomOffset = true;
	}


	public void ResetTargetOffsets()
	{
		targetPivotOffset = pivotOffset;
		targetCamOffset = camOffset;
		isCustomOffset = false;
	}


	public void ResetYCamOffset()
	{
		targetCamOffset.y = camOffset.y;
	}

	public void SetYCamOffset(float y)
	{
		targetCamOffset.y = y;
	}

	public void SetXCamOffset(float x)
	{
		targetCamOffset.x = x;
	}

	public void SetFOV(float customFOV)
	{
		this.targetFOV = customFOV;
	}

	public void ResetFOV()
	{
		this.targetFOV = defaultFOV;
	}

	public void SetMaxVerticalAngle(float angle)
	{
		this.targetMaxVerticalAngle = angle;
	}

	public void ResetMaxVerticalAngle()
	{
		this.targetMaxVerticalAngle = maxVerticalAngle;
	}

	bool DoubleViewingPosCheck(Vector3 checkPos)
	{
		return ViewingPosCheck(checkPos) && ReverseViewingPosCheck(checkPos);
	}

	bool ViewingPosCheck(Vector3 checkPos)
	{
		Vector3 target = player.position + pivotOffset;
		Vector3 direction = target - checkPos;
		if (Physics.SphereCast(checkPos, 0.2f, direction, out RaycastHit hit, direction.magnitude))
		{
			if (hit.transform != player && !hit.transform.GetComponent<Collider>().isTrigger)
			{
				return false;
			}
		}
		return true;
	}

	bool ReverseViewingPosCheck(Vector3 checkPos)
	{
		Vector3 origin = player.position + pivotOffset;
		Vector3 direction = checkPos - origin;
		if (Physics.SphereCast(origin, 0.2f, direction, out RaycastHit hit, direction.magnitude))
		{
			if (hit.transform != player && hit.transform != transform && !hit.transform.GetComponent<Collider>().isTrigger)
			{
				return false;
			}
		}
		return true;
	}
	public float GetCurrentPivotMagnitude(Vector3 finalPivotOffset)
	{
		return Mathf.Abs((finalPivotOffset - smoothPivotOffset).magnitude);
	}
}
