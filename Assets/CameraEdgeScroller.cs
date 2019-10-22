using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEdgeScroller : MonoBehaviour
{
	int mDelta = 3;
	float mSpeed = 4f;
	Vector3 mRightDirection = Vector3.right;
	Vector3 mUpDirection = Vector3.up;


    // Update is called once per frame
    void Update()
    {
		if (!Application.isFocused)
			return;

		Vector3 targetPos = transform.position;
		if (Input.mousePosition.x >= Screen.width - mDelta)
		{
			// Move the camera
			targetPos += mRightDirection * Time.deltaTime * mSpeed;
		}
		if (Input.mousePosition.x <= 0 + mDelta)
		{
			// Move the camera
			targetPos -= mRightDirection * Time.deltaTime * mSpeed;
		}
		if (Input.mousePosition.y >= Screen.height - mDelta)
		{
			// Move the camera
			targetPos += mUpDirection * Time.deltaTime * mSpeed;
		}
		if (Input.mousePosition.y <= 0 + mDelta)
		{
			// Move the camera
			targetPos -= mUpDirection * Time.deltaTime * mSpeed;
		}

		transform.position = targetPos;
	}
}
