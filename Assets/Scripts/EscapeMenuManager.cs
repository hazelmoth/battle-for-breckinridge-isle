using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeMenuManager : MonoBehaviour
{
	[SerializeField] private GameObject escapeMenuCanvas;

	private void Start()
	{
		escapeMenuCanvas.SetActive(false);
	}
	// Update is called once per frame
	private void Update()
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