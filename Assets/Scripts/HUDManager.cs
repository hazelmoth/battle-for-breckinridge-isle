using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI currentTurnText;
	[SerializeField] private TextMeshProUGUI placeArmyText;
	[SerializeField] private GameObject endTurnButton;
	private static HUDManager _instance;

	// Start is called before the first frame update
	private void Start()
	{
		_instance = this;
		HidePlaceArmyText();
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	public static void ShowHudForPlayer(Player player)
	{
		_instance.currentTurnText.text = player.NationName + "'s turn";
		_instance.currentTurnText.color = player.Color;
		_instance.endTurnButton.SetActive(player.AllowHumanInput);
	}

	public static void ShowPlaceArmyText (int armiesToPlace)
	{
		_instance.placeArmyText.gameObject.SetActive(true);
		_instance.placeArmyText.text = armiesToPlace + " armies remaining.\nChoose a territory to place an army.";
	}

	public static void HidePlaceArmyText ()
	{
		_instance.placeArmyText.gameObject.SetActive(false);
	}
}
