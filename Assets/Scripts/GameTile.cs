﻿public class GameTile
{
	public TileType type;
	public Player owner;
	public int armies;
	public int expendedArmies;

	public int AvailableArmies => armies - expendedArmies;
}
