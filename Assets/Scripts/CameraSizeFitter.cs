using UnityEngine;

public class CameraSizeFitter : MonoBehaviour
{
	private Camera _cam;
	// Start is called before the first frame update
	private void Start()
	{
		_cam = GetComponent<Camera>();
		float camX = GameController.WorldX  / 2;
		float camY = GameController.WorldY  / 2;
		_cam.transform.position = new Vector3(camX, camY, _cam.transform.position.z);

		_cam.orthographicSize = 3.5f;
	}

	// Update is called once per frame
	private void Update()
	{

	}
}