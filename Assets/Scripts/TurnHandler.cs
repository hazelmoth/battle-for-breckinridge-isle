using UnityEngine;

public class TurnHandler : MonoBehaviour
{
	public int currentTurn;
	private int _currentPlayerIndex;
	private static TurnHandler _instance;

	// Start is called before the first frame update
	private void Start()
	{
		_instance = this;
		currentTurn = 1;
		_currentPlayerIndex = 0;
	}

	public static int CurrentTurn => _instance.currentTurn;

	public static Player GetCurrentPlayer ()
	{
		return GameController.instance.remainingPlayers[_instance._currentPlayerIndex];
	}

	public static void ProgressTurn()
	{
		GameController.instance.ResetAllExpendedArmies();
		GameController.instance.EvaluateRemainingPlayers();

		if (GameController.instance.gameEnded)
			return;

		_instance._currentPlayerIndex++;
		if (_instance._currentPlayerIndex >= GameController.instance.remainingPlayers.Count)
			_instance._currentPlayerIndex = 0;
		_instance.currentTurn++;

		if (_instance.currentTurn > 2)
			GameController.instance.CollectArmies(GetCurrentPlayer());

		if (GetCurrentPlayer().ArmiesToPlace > 0)
		{
			HUDManager.ShowPlaceArmyText(GetCurrentPlayer().ArmiesToPlace);
			InputHandler.instance.placingArmies = true;
		}
		HUDManager.ShowHudForPlayer(GetCurrentPlayer());
		GetCurrentPlayer().BeginTurn(ProgressTurn);
	}

	public static void StartGame()
	{
		GetCurrentPlayer().BeginTurn(ProgressTurn);
	}
}
