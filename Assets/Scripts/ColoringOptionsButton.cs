﻿using UnityEngine;


class ColoringOptionsButton : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public GraphManager.GeneExpressionColoringMethods modeToSwitchTo;

    private GraphManager graphManager;
    private new Renderer renderer;

    private void Start()
    {
        graphManager = referenceManager.graphManager;
        renderer = GetComponent<Renderer>();
    }

    public void PressButton()
    {
        graphManager.GeneExpressionColoringMethod = modeToSwitchTo;
        // set all other texts to white and ours to green
        foreach (TextMesh textMesh in transform.parent.gameObject.GetComponentsInChildren<TextMesh>())
        {
            textMesh.color = Color.white;
        }
        GetComponentInChildren<TextMesh>().color = Color.green;
    }

    /// <summary>
    /// Sets the list node's material to a new material.
    /// </summary>
    /// <param name="newMaterial"> The new material. </param>
    public void SetMaterial(Material newMaterial)
    {
        renderer.material = newMaterial;
    }

}
