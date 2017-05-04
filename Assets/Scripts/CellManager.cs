﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellManager : MonoBehaviour {

	private Dictionary<string, Cell> cells;
	public Cell cell;
	public List<Material> materialList;


	void Awake(){
		cells = new Dictionary<string, Cell>();
	}

	public Cell addCell(string label) {
		if(!cells.ContainsKey(label)) {
			cells [label] = new Cell (label, materialList);
		}
		return cells [label];
	}

	public void setGeneExpression(string cellName, string geneName, int slot){
		Cell cell;
		cells.TryGetValue (cellName, out cell);
		cell.setExpressionData (geneName, slot);
	}

}
