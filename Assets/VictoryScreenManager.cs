using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class VictoryScreenManager : MonoBehaviour
{
	[SerializeField] GameObject victoryCanvas;
	[SerializeField] TextMeshProUGUI victoryText;
	static VictoryScreenManager instance;

    // Start is called before the first frame update
    void Start()
    {
		instance = this;
		victoryCanvas.SetActive(false);
    }
	private void OnDestroy()
	{
		instance = null;
	}
	// Update is called once per frame
	void Update()
    {
        
    }

	public static void LaunchVictoryScreen (Player winner)
	{
		instance.victoryCanvas.SetActive(true);
		instance.victoryText.text = winner.nationName + " is victorious!";
	}
	public void OnNewGameButton ()
	{
		SceneManager.LoadScene(1);
	}
}
