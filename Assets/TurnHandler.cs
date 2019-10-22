using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnHandler : MonoBehaviour
{
	public int currentTurn;
	int currentPlayerIndex;
	static TurnHandler instance;

    // Start is called before the first frame update
    void Start()
    {
		instance = this;
		currentTurn = 0;
		currentPlayerIndex = 0;
    }

	public static Player GetCurrentPlayer ()
	{
		return GameController.instance.players[instance.currentPlayerIndex];
	}
	public static void ProgressTurn()
	{
		GameController.instance.ResetAllExpendedArmies();
		GameController.instance.EvaluateRemainingPlayers();
		GameController.instance.CollectArmies(GetCurrentPlayer());

		if (GameController.instance.gameEnded)
			return;

		instance.currentPlayerIndex++;
		if (instance.currentPlayerIndex >= GameController.instance.players.Count)
			instance.currentPlayerIndex = 0;
		instance.currentTurn++;

		if (GetCurrentPlayer().armiesToPlace > 0)
		{
			HUDManager.ShowPlaceArmyText(GetCurrentPlayer().armiesToPlace);
			InputHandler.instance.placingArmies = true;
		}
		HUDManager.SetPlayerIndicator(GetCurrentPlayer());
	}
}
