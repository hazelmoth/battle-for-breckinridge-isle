using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreenManager : MonoBehaviour
{
	[SerializeField] private GameObject victoryCanvas;
	[SerializeField] private TextMeshProUGUI victoryText;
	private static VictoryScreenManager _instance;

	// Start is called before the first frame update
	private void Start()
	{
		_instance = this;
		victoryCanvas.SetActive(false);
	}
	private void OnDestroy()
	{
		_instance = null;
	}
	// Update is called once per frame
	private void Update()
	{
        
	}

	public static void LaunchVictoryScreen (Player winner)
	{
		_instance.victoryCanvas.SetActive(true);
		_instance.victoryText.text = winner.NationName + " is victorious!";
	}
	public void OnNewGameButton ()
	{
		SceneManager.LoadScene(1);
	}
}