using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSizeFitter : MonoBehaviour
{
    Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
		float camX = GameController.WORLD_X  / 2;
		float camY = GameController.WORLD_Y  / 2;
		cam.transform.position = new Vector3(camX, camY, cam.transform.position.z);

		cam.orthographicSize = 3.5f;
	}

    // Update is called once per frame
    void Update()
    {

	}
}
