﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class MazeGenerator : MonoBehaviour
{
	[SerializeField] private int _width, _height;
	[SerializeField] private Cell _cellPrefab;
	public Dictionary<Vector2Int, CellData> UnvisitedCells, VisitedCells;

	private Random rand;

	public void StartGeneration(int height, int width, int finishCellPosX)
	{
		rand = new Random();

		_height = height;
		_width = width;

		UnvisitedCells = new Dictionary<Vector2Int, CellData>();
		VisitedCells = new Dictionary<Vector2Int, CellData>();

		Vector2Int position = Vector2Int.zero;

		for (int i = 0; i < _width * _height; i++)
		{
			UnvisitedCells.Add(position, new CellData(position));
			UnvisitedCells[position].RefreshNeighbours(VisitedCells, _width, _height);

			GameObject obj = Instantiate(_cellPrefab.gameObject, new Vector3(position.x, 0, position.y), Quaternion.identity, transform);
			obj.GetComponent<Cell>().SetData(UnvisitedCells[position]);


			if (position.x == finishCellPosX && position.y == _height - 1)
				obj.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
			else
				obj.GetComponent<BoxCollider>().enabled = false;

			if (position.x == _width - 1)
			{
				position.x = 0;
				position.y++;
			}
			else position.x++;
		}

		GenerateMaze(UnvisitedCells.Values.First());
	}

	private void GenerateMaze(CellData start)
	{
		var curr = start;
		var prev = new CellData(new Vector2Int(-1, -1));
		curr.RefreshNeighbours(VisitedCells, _width, _height);

		while (UnvisitedCells.Count > 0)
		{
			UnvisitedCells.Remove(curr.Position);
			VisitedCells.Add(curr.Position, curr);

			curr.RefreshNeighbours(VisitedCells, _width, _height);

			if (prev.Position != new Vector2Int(-1, -1))
			{
				curr.RemoveWall(prev.Position);
				curr.MarkDirty(true);
			}

			if (curr.HasUnvisitedNeighbour)
			{
				var next = UnvisitedCells[curr.GetRandomNeighbour(rand)];
				next.RefreshNeighbours(VisitedCells, _width, _height);
				curr.RemoveWall(next.Position);
				curr.MarkDirty(true);
				prev = curr;
				curr = next;
			}
			else if (UnvisitedCells.Count > 0)
			{
				var unvisitedCell = UnvisitedCells.Values.First();
				unvisitedCell.RefreshNeighbours(VisitedCells, _width, _height);
				prev = VisitedCells[unvisitedCell.GetRandomVisitedNeighbour(rand)];
				prev.RemoveWall(unvisitedCell.Position);
				prev.MarkDirty(true);
				curr = unvisitedCell;
			}
		}
	}

	// Delete all cells
	public void ClearMaze()
	{
		for (int i = 0; i < transform.childCount; i++)
			Destroy(transform.GetChild(i).gameObject);
	}
}