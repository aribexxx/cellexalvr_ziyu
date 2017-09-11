﻿using System;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;

/// <summary>
/// This class represents the button that calculates the correlated genes.
/// </summary>
public class CorrelatedGenesButton : MonoBehaviour
{
    public PreviousSearchesListNode listNode;
    public CorrelatedGenesList correlatedGenesList;
    public SelectionToolHandler selectionToolHandler;
    public StatusDisplay statusDisplay;
    private bool calculatingGenes = false;
    private new Renderer renderer;
    private string outputFile = Directory.GetCurrentDirectory() + @"\Assets\Resources\correlated_genes.txt";

    private void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    /// <summary>
    /// Runs the R script that calculates the correlated and anti correlated genes and populates the lists with those genes.
    /// </summary>
    public void CalculateCorrelatedGenes()
    {
        if (listNode.GeneName == "")
            return;
        StartCoroutine(CalculateCorrelatedGenesCoroutine());
    }
    /// <summary>
    /// Sets the texture of this button.
    /// </summary>
    /// <param name="newTexture"> The new texture. </param>
    public void SetTexture(Texture newTexture)
    {
        if (!calculatingGenes)
        {
            renderer.material.mainTexture = newTexture;
        }
    }

    IEnumerator CalculateCorrelatedGenesCoroutine()
    {
        calculatingGenes = true;
        var geneName = listNode.GeneName;
        string args = selectionToolHandler.DataDir + " " + geneName + " " + outputFile;
        Thread t = new Thread(() => RScriptRunner.RunFromCmd(@"\Assets\Scripts\R\get_correlated_genes.R", args));
        var statusId = statusDisplay.AddStatus("Calculating genes correlated to " + geneName);
        t.Start();
        while (t.IsAlive)
        {
            yield return null;
        }
        // r script is done, read the results.
        string[] lines = File.ReadAllLines(outputFile);
        // if the file is not 2 lines, something probably went wrong
        if (lines.Length != 2)
        {
            Debug.LogWarning("Correlated genes file at " + outputFile + " was not 2 lines long. Actual length: " + lines.Length);
            yield break;
        }

        string[] correlatedGenes = lines[0].Split(null);
        string[] anticorrelatedGenes = lines[1].Split(null);
        correlatedGenesList.SetVisible(true);
        correlatedGenesList.PopulateList(geneName, correlatedGenes, anticorrelatedGenes);
        // set the texture to a happy face :)
        calculatingGenes = false;
        SetTexture(GetComponentInParent<PreviousSearchesList>().correlatedGenesButtonHighlightedTexture);
        statusDisplay.RemoveStatus(statusId);
    }
}