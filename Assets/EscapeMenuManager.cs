using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeMenuManager : MonoBehaviour
{
	[SerializeField] GameObject escapeMenuCanvas;

	private void Start()
	{
		escapeMenuCanvas.SetActive(false);
	}
	// Update is called once per frame
	void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
		{
			escapeMenuCanvas.SetActive(!escapeMenuCanvas.activeInHierarchy);
		}
    }
	public void OnNewGameButton ()
	{
		SceneManager.LoadScene(1);
	}
	public void OnQuitButton ()
	{
		Application.Quit();
	}
}
