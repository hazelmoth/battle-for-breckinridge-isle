using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI currentTurnText;
	[SerializeField] TextMeshProUGUI placeArmyText;
	static HUDManager instance;

    // Start is called before the first frame update
    void Start()
    {
		instance = this;
		HidePlaceArmyText();
    }
	private void OnDestroy()
	{
		instance = null;
	}
	// Update is called once per frame
	void Update()
    {
        
    }
	public static void SetPlayerIndicator(Player player)
	{
		instance.currentTurnText.text = player.nationName + "'s turn";
		instance.currentTurnText.color = player.color;
	}
	public static void ShowPlaceArmyText (int armiesToPlace)
	{
		instance.placeArmyText.gameObject.SetActive(true);
		instance.placeArmyText.text = armiesToPlace + " armies remaining.\nChoose a territory to place an army.";
	}
	public static void HidePlaceArmyText ()
	{
		instance.placeArmyText.gameObject.SetActive(false);
	}
}
