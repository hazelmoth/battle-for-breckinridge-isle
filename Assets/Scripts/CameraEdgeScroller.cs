using UnityEngine;

public class CameraEdgeScroller : MonoBehaviour
{
	private int _mDelta = 3;
	private float _mSpeed = 4f;
	private Vector3 _mRightDirection = Vector3.right;
	private Vector3 _mUpDirection = Vector3.up;


	// Update is called once per frame
	private void Update()
	{
		if (!Application.isFocused)
			return;

		Vector3 targetPos = transform.position;
		if (Input.mousePosition.x >= Screen.width - _mDelta)
		{
			// Move the camera
			targetPos += _mRightDirection * Time.deltaTime * _mSpeed;
		}
		if (Input.mousePosition.x <= 0 + _mDelta)
		{
			// Move the camera
			targetPos -= _mRightDirection * Time.deltaTime * _mSpeed;
		}
		if (Input.mousePosition.y >= Screen.height - _mDelta)
		{
			// Move the camera
			targetPos += _mUpDirection * Time.deltaTime * _mSpeed;
		}
		if (Input.mousePosition.y <= 0 + _mDelta)
		{
			// Move the camera
			targetPos -= _mUpDirection * Time.deltaTime * _mSpeed;
		}

		transform.position = targetPos;
	}
}