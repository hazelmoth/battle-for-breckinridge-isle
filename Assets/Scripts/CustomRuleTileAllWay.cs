﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class CustomRuleTileAllWay : TileBase
{
	[SerializeField] private List<Sprite> sprites;

	public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		Sprite[] surroundingSprites = null;
		GetNeighboringTileSprites(tilemap, position, ref surroundingSprites);
		var iden = Matrix4x4.identity;

		tileData.sprite = sprites[0];
		tileData.colliderType = Tile.ColliderType.None;
		tileData.flags = TileFlags.LockTransform;
		tileData.transform = iden;

		Matrix4x4 transform = iden;

		// Checks if another sprite is one from this same tile
		bool IsOneOfThese(Sprite spriteToCheck, bool includeNullTile)
		{
			if (includeNullTile && spriteToCheck == null)
			{
				return true;
			}
			foreach (Sprite sprite in sprites)
			{
				if (sprite == spriteToCheck)
					return true;
			}
			return false;
		}

		bool top = (IsOneOfThese(surroundingSprites[1],         true));
		bool left = (IsOneOfThese(surroundingSprites[3],        true));
		bool right = (IsOneOfThese(surroundingSprites[4],       true));
		bool bottom = (IsOneOfThese(surroundingSprites[6],      true));
		bool topLeft = (IsOneOfThese(surroundingSprites[0],     true));
		bool topRight = (IsOneOfThese(surroundingSprites[2],    true));
		bool bottomLeft = (IsOneOfThese(surroundingSprites[5],  true));
		bool bottomRight = (IsOneOfThese(surroundingSprites[7], true));

		List<bool> scenarios = new List<bool>
		{
			top && left && right && bottom && topLeft && topRight && bottomLeft && bottomRight,
			top && left && right && bottom && !topLeft && topRight && bottomLeft && bottomRight,
			top && left && right && bottom && topLeft && !topRight && bottomLeft && bottomRight,
			top && left && right && bottom && topLeft && topRight && bottomLeft && !bottomRight,
			top && left && right && bottom && topLeft && topRight && !bottomLeft && bottomRight,
			top && left && right && bottom && !topLeft && topRight && !bottomLeft && bottomRight,
			top && left && right && bottom && !topLeft && !topRight && bottomLeft && bottomRight,
			top && left && right && bottom && topLeft && !topRight && bottomLeft && !bottomRight,
			top && left && right && bottom && topLeft && topRight && !bottomLeft && !bottomRight,
			top && left && right && bottom && !topLeft && topRight && bottomLeft && !bottomRight,
			top && left && right && bottom && topLeft && !topRight && !bottomLeft && bottomRight,
			top && left && right && bottom && topLeft && !topRight && !bottomLeft && !bottomRight,
			top && left && right && bottom && !topLeft && topRight && !bottomLeft && !bottomRight,
			top && left && right && bottom && !topLeft && !topRight && !bottomLeft && bottomRight,
			top && left && right && bottom && !topLeft && !topRight && bottomLeft && !bottomRight,
			top && left && right && bottom && !topLeft && !topRight && !bottomLeft && !bottomRight,
			top && !left && right && bottom && topRight && bottomRight,
			top && !left && right && bottom && !topRight && bottomRight,
			top && !left && right && bottom && topRight && !bottomRight,
			top && !left && right && bottom && !topRight && !bottomRight,
			!top && left && right && bottom && bottomLeft && bottomRight,
			!top && left && right && bottom && bottomLeft && !bottomRight,
			!top && left && right && bottom && !bottomLeft && bottomRight,
			!top && left && right && bottom && !bottomLeft && !bottomRight,
			top && left && !right && bottom && topLeft && bottomLeft,
			top && left && !right && bottom && !topLeft && bottomLeft,
			top && left && !right && bottom && topLeft && !bottomLeft,
			top && left && !right && bottom && !topLeft && !bottomLeft,
			top && left && right && !bottom && topLeft && topRight,
			top && left && right && !bottom && !topLeft && topRight,
			top && left && right && !bottom && topLeft && !topRight,
			top && left && right && !bottom && !topLeft && !topRight,
			top && !left && !right && bottom,
			!top && left && right && !bottom,
			!top && !left && right && bottom && bottomRight,
			!top && !left && right && bottom && !bottomRight,
			!top && left && !right && bottom && bottomLeft,
			!top && left && !right && bottom && !bottomLeft,
			top && left && !right && !bottom && topLeft,
			top && left && !right && !bottom && !topLeft,
			top && !left && right && !bottom && topRight,
			top && !left && right && !bottom && !topRight,
			!top && left && !right && !bottom,
			top && !left && !right && !bottom,
			!top && !left && right && !bottom,
			!top && !left && !right && bottom,
			!top && !left && !right && !bottom
		};

		for (int i = 0; i < scenarios.Count; i++)
		{
			if (scenarios[i])
			{
				tileData.sprite = sprites[i];
			}
		}
	}

	public override void RefreshTile(Vector3Int location, ITilemap tileMap)
	{
		for (int y = -1; y <= 1; y++)
		{
			for (int x = -1; x <= 1; x++)
			{
				base.RefreshTile(location + new Vector3Int(x, y, 0), tileMap);
			}
		}
	}

	private void GetNeighboringTileSprites(ITilemap tilemap, Vector3Int position, ref Sprite[] neighboringTileSprites)
	{
		if (neighboringTileSprites != null)
			return;

		Sprite[] mCachedNeighboringTiles = new Sprite[8];

		int index = 0;
		for (int y = 1; y >= -1; y--)
		{
			for (int x = -1; x <= 1; x++)
			{
				if (x != 0 || y != 0)
				{
					Vector3Int tilePosition = new Vector3Int(position.x + x, position.y + y, position.z);
					mCachedNeighboringTiles[index++] = tilemap.GetSprite(tilePosition);
				}
			}
		}
		neighboringTileSprites = mCachedNeighboringTiles;
	}

}
