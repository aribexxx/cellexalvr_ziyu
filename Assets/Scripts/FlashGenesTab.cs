﻿using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the individual tabs that make up the menu for cnotroling the flashing genes.
/// </summary>
public class FlashGenesTab : Tab
{
    public FlashGenesCategoryButton buttonPrefab;
    public string GeneFilePath { get; set; }

    private CellManager cellManager;
    private InputReader inputReader;
    private MenuToggler menuToggler;
    private List<FlashGenesCategoryButton> buttons = new List<FlashGenesCategoryButton>();
    private Vector3 buttonPosStart = new Vector3(0, 0, 0);
    private Vector3 buttonPosInc = new Vector3(2.5f, 0, 0);
    private Vector3 buttonPosNewRowInc = new Vector3(-7.5f, -2.5f, 0);

    private void Awake()
    {
        cellManager = referenceManager.cellManager;
        inputReader = referenceManager.inputReader;
        menuToggler = referenceManager.menuToggler;
    }

    /// <summary>
    /// Creates the category buttons that is displayed on this tab.
    /// </summary>
    /// <param name="categories"> An array with the names of the categories. </param>
    public void CreateCategoryButtons(string[] categories)
    {
        Vector3 nextButtonPos = buttonPosStart;
        foreach (string category in categories)
        {
            FlashGenesCategoryButton newButton = Instantiate(buttonPrefab, transform);
            newButton.gameObject.SetActive(true);
            menuToggler.AddGameObjectToActivate(newButton.gameObject, gameObject);
            menuToggler.AddGameObjectToActivate(newButton.transform.GetChild(0).gameObject, gameObject);
            newButton.transform.localPosition = nextButtonPos;
            newButton.transform.localRotation = Quaternion.identity;
            newButton.transform.localScale = Vector3.one;
            newButton.Category = category;
            newButton.textRenderer.text = category;
            buttons.Add(newButton);

            if (buttons.Count % 4 == 0)
            {
                nextButtonPos += buttonPosNewRowInc;
            }
            else
            {
                nextButtonPos += buttonPosInc;
            }
        }
    }

    /// <summary>
    /// Turns this tab on or off.
    /// </summary>
    /// <param name="active"> True for turning this tab on, false for turning it off. </param>
    public override void SetTabActive(bool active)
    {
        base.SetTabActive(active);
        if (active)
        {
            if (!cellManager)
                cellManager = referenceManager.cellManager;
            if (!inputReader)
                inputReader = referenceManager.inputReader;
            cellManager.SaveFlashGenesData(inputReader.ReadFlashingGenesFiles(GeneFilePath));
        }
    }
}