﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using VRTK;

/// <summary>
/// This class represents the selection tool that can be used to select multiple GraphPoints.
/// </summary>
public class SelectionToolHandler : MonoBehaviour
{
    public ReferenceManager referenceManager;

    public ushort hapticIntensity = 2000;
    public RadialMenu radialMenu;
    public Sprite[] buttonIcons;
    public GroupInfoDisplay groupInfoDisplay;
    public GroupInfoDisplay HUDGroupInfoDisplay;
    public GroupInfoDisplay FarGroupInfoDisplay;
    [HideInInspector]
    public bool selectionConfirmed = false;
    [HideInInspector]
    public bool heatmapGrabbed = false;
    [HideInInspector]
    public int fileCreationCtr = 0;
    public Material graphpointNormal;
    public Material graphpointHighlight;
    public Color[] Colors;

    private SelectionToolMenu selectionToolMenu;
    private CreateSelectionFromPreviousSelectionMenu previousSelectionMenu;
    private ControllerModelSwitcher controllerModelSwitcher;
    private SteamVR_TrackedObject rightController;
    private List<GraphPoint> selectedCells = new List<GraphPoint>();
    private List<GraphPoint> lastSelectedCells = new List<GraphPoint>();
    private Color selectedColor;
    private PlanePicker planePicker;
    private bool selectionMade = false;
    private GameObject grabbedObject;
    private bool heatmapCreated = true;

    [HideInInspector]
    public int[] groups = new int[10];
    private int currentColorIndex = 0;
    public string DataDir { get; set; }
    private List<HistoryListInfo> selectionHistory = new List<HistoryListInfo>();
    // the number of steps we have taken back in the history.
    private int historyIndexOffset;
    private GameManager gameManager;

    /// <summary>
    /// Helper struct for remembering history when selecting graphpoints.
    /// </summary>
    private struct HistoryListInfo
    {
        // the graphpoint this affects
        public GraphPoint graphPoint;
        // the color it was given
        public int toGroup;
        // the color it had before
        public int fromGroup;
        // true if this graphpoint was previously not in the list of selected graphpoints
        public bool newNode;

        public HistoryListInfo(GraphPoint graphPoint, int toGroup, int fromGroup, bool newNode)
        {
            this.graphPoint = graphPoint;
            this.toGroup = toGroup;
            this.fromGroup = fromGroup;
            this.newNode = newNode;
        }
    }

    void Awake()
    {
        // TODO CELLEXAL: create more colors.
        //Colors = new Color[10];
        //Colors[0] = new Color(1, 0, 0, .5f);     // red
        //Colors[1] = new Color(0, 0, 1, .5f);     // blue
        //Colors[2] = new Color(0, 1, 1, .5f);     // cyan
        //Colors[3] = new Color(1, 0, 1, .5f);     // magenta
        //Colors[4] = new Color(1f, 153f / 255f, 204f / 255f, 0.5f);     // pink
        //Colors[5] = new Color(1, 1, 0, .5f);     // yellow
        //Colors[6] = new Color(0, 1, 0, .5f);     // green
        //Colors[7] = new Color(.5f, 0, .5f, .5f);     // purple
        //Colors[8] = new Color(102f / 255f, 51f / 255f, 1, .5f);     // brown
        //Colors[9] = new Color(1, 153f / 255f, 51f / 255f, .5f);     // orange

        //selectorMaterial.color = colors[0];
        radialMenu.buttons[1].ButtonIcon = buttonIcons[buttonIcons.Length - 1];
        radialMenu.buttons[3].ButtonIcon = buttonIcons[1];
        radialMenu.RegenerateButtons();
        previousSelectionMenu = referenceManager.createSelectionFromPreviousSelectionMenu;
        //UpdateButtonIcons();
        SetSelectionToolEnabled(false);
        CellExAlEvents.SelectionToolColorsChanged.AddListener(UpdateColors);
    }

    private void Start()
    {
        selectionToolMenu = referenceManager.selectionToolMenu;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
        rightController = referenceManager.rightController;
        gameManager = referenceManager.gameManager;
    }

    /// <summary>
    /// Updates <see cref="Colors"/> to <see cref="CellExAlConfig.SelectionToolColors"/>.
    /// </summary>
    public void UpdateColors()
    {
        Colors = CellExAlConfig.SelectionToolColors;
        selectedColor = Colors[currentColorIndex];
        groupInfoDisplay.SetColors(Colors);
        HUDGroupInfoDisplay.SetColors(Colors);
        FarGroupInfoDisplay.SetColors(Colors);
    }

    /// <summary>
    /// Adds a graphpoint to the current selection, and changes its color to the current color of the selection tool.
    /// This method is called by a child object that holds the collider.
    /// </summary>
    public void AddGraphpointToSelection(GraphPoint graphPoint)
    {
        AddGraphpointToSelection(graphPoint, currentColorIndex, true);
    }

    /// <summary>
    /// Adds a graphpoint to the current selection, and changes its color.
    /// </summary>
    public void AddGraphpointToSelection(GraphPoint graphPoint, int newGroup, bool hapticFeedback)
    {
        // print(other.gameObject.name);
        if (graphPoint == null)
        {
            return;
        }
        Renderer renderer = graphPoint.gameObject.GetComponent<Renderer>();

        int oldGroup = graphPoint.CurrentGroup;

        Color oldColor = oldGroup == -1 ? Color.white : Colors[oldGroup];
        Color newColor = Colors[newGroup];
        graphPoint.Outline(Colors[newGroup]);
        graphPoint.CurrentGroup = newGroup;
        renderer.material.color = Colors[newGroup];
        gameManager.InformGraphPointChangedColor(graphPoint.GraphName, graphPoint.Label, Colors[newGroup]);

        bool newNode = !selectedCells.Contains(graphPoint);
        if (historyIndexOffset != 0)
        {
            // if we have undone some selected graphpoints, then they should be removed from the history
            selectionHistory.RemoveRange(selectionHistory.Count - historyIndexOffset, historyIndexOffset);
            historyIndexOffset = 0;
            // turn off the redo buttons
            CellExAlEvents.EndOfHistoryReached.Invoke();
        }
        if (!selectionMade)
        {
            // if this is a new selection we should reset some stuff
            selectionMade = true;
            //selectionToolMenu.SelectionStarted();
            groupInfoDisplay.ResetGroupsInfo();
            HUDGroupInfoDisplay.ResetGroupsInfo();
            FarGroupInfoDisplay.ResetGroupsInfo();
            // turn on the undo buttons
            CellExAlEvents.BeginningOfHistoryLeft.Invoke();
        }
        // The user might select cells that already have that color
        if (!Equals(newGroup, oldGroup))
        {
            selectionHistory.Add(new HistoryListInfo(graphPoint, newGroup, oldGroup, newNode));

            if (hapticFeedback)
                SteamVR_Controller.Input((int)rightController.index).TriggerHapticPulse(hapticIntensity);

            groupInfoDisplay.ChangeGroupsInfo(newGroup, 1);
            HUDGroupInfoDisplay.ChangeGroupsInfo(newGroup, 1);
            FarGroupInfoDisplay.ChangeGroupsInfo(newGroup, 1);
            if (newNode)
            {
                gameManager.InformSelectedAdd(graphPoint.GraphName, graphPoint.Label);
                if (selectedCells.Count == 0)
                {
                    CellExAlEvents.SelectionStarted.Invoke();
                }
                selectedCells.Add(graphPoint);
            }
            else
            {
                groupInfoDisplay.ChangeGroupsInfo(oldGroup, -1);
                HUDGroupInfoDisplay.ChangeGroupsInfo(oldGroup, -1);
                FarGroupInfoDisplay.ChangeGroupsInfo(oldGroup, -1);
            }
        }
    }

    public void DoClientSelectAdd(string graphName, string label)
    {
        GraphPoint gp = referenceManager.graphManager.FindGraphPoint(graphName, label);
        selectedCells.Add(gp);
    }

    /// <summary>
    /// Helper method to see if two colors are equal.
    /// </summary>
    /// <param name="c1"> The first color. </param>
    /// <param name="c2"> The second color. </param>
    /// <returns> True if the two colors have the same rgb values, false otherwise. </returns>
    private bool Equals(Color c1, Color c2)
    {
        return c1.r == c2.r && c1.g == c2.g && c1.b == c2.b;
    }

    /// <summary>
    /// Goes back one step in the history of selecting cells.
    /// </summary>
    public void GoBackOneStepInHistory()
    {
        if (historyIndexOffset == 0)
        {
            CellExAlEvents.EndOfHistoryLeft.Invoke();
        }

        int indexToMoveTo = selectionHistory.Count - historyIndexOffset - 1;
        if (indexToMoveTo == 0)
        {
            // beginning of history reached
            CellExAlEvents.BeginningOfHistoryReached.Invoke();
            //selectionToolMenu.UndoSelection();
        }
        else if (indexToMoveTo < 0)
        {
            // no more history
            return;
        }
        HistoryListInfo info = selectionHistory[indexToMoveTo];
        info.graphPoint.CurrentGroup = info.fromGroup;
        info.graphPoint.Outline(Colors[info.fromGroup]);
        groupInfoDisplay.ChangeGroupsInfo(info.toGroup, -1);
        HUDGroupInfoDisplay.ChangeGroupsInfo(info.toGroup, -1);
        FarGroupInfoDisplay.ChangeGroupsInfo(info.toGroup, -1);
        if (info.newNode)
        {
            selectedCells.Remove(info.graphPoint);
            info.graphPoint.Outline(Color.clear);
        }
        else
        {
            groupInfoDisplay.ChangeGroupsInfo(info.fromGroup, 1);
            HUDGroupInfoDisplay.ChangeGroupsInfo(info.fromGroup, 1);
            FarGroupInfoDisplay.ChangeGroupsInfo(info.fromGroup, 1);
            info.graphPoint.Outline(Colors[info.fromGroup]);
        }
        historyIndexOffset++;
        selectionMade = false;
    }

    /// <summary>
    /// Go forward one step in the history of selecting cells.
    /// </summary>
    public void GoForwardOneStepInHistory()
    {
        if (historyIndexOffset == selectionHistory.Count)
        {
            CellExAlEvents.BeginningOfHistoryLeft.Invoke();
            //selectionToolMenu.SelectionStarted();
        }

        int indexToMoveTo = selectionHistory.Count - historyIndexOffset;
        if (indexToMoveTo == selectionHistory.Count - 1)
        {
            // end of history reached
            CellExAlEvents.EndOfHistoryReached.Invoke();
        }
        else if (indexToMoveTo >= selectionHistory.Count)
        {
            // no more history
            return;
        }

        HistoryListInfo info = selectionHistory[indexToMoveTo];
        info.graphPoint.CurrentGroup = info.toGroup;
        info.graphPoint.Outline(Colors[info.toGroup]);
        groupInfoDisplay.ChangeGroupsInfo(info.toGroup, 1);
        HUDGroupInfoDisplay.ChangeGroupsInfo(info.toGroup, 1);
        FarGroupInfoDisplay.ChangeGroupsInfo(info.toGroup, 1);
        if (info.newNode)
        {
            selectedCells.Add(info.graphPoint);
        }
        else
        {
            groupInfoDisplay.ChangeGroupsInfo(info.fromGroup, -1);
            HUDGroupInfoDisplay.ChangeGroupsInfo(info.fromGroup, -1);
            FarGroupInfoDisplay.ChangeGroupsInfo(info.fromGroup, -1);
        }
        historyIndexOffset--;
        selectionMade = false;
    }

    /// <summary>
    /// Go back in history until the color changes. This unselects all the last cells that have the same color.
    /// </summary>
    /// <example>
    /// If the user selects 2 cells as red then 3 cells as blue and then 4 cells as red, in that order, the 4 last red cells would be unselected when calling this method. 
    /// </example>
    public void GoBackOneColorInHistory()
    {
        int indexToMoveTo = selectionHistory.Count - historyIndexOffset - 1;
        Color color = Colors[selectionHistory[indexToMoveTo].toGroup];
        Color nextColor;
        do
        {
            GoBackOneStepInHistory();
            indexToMoveTo--;
            if (indexToMoveTo >= 0)
            {
                nextColor = Colors[selectionHistory[indexToMoveTo].toGroup];
            }
            else
            {
                break;
            }
        } while (color.Equals(nextColor));
    }

    /// <summary>
    /// Go forward in history until the color changes. This re-selects all the last cells that have the same color.
    /// </summary>
    public void GoForwardOneColorInHistory()
    {
        int indexToMoveTo = selectionHistory.Count - historyIndexOffset;
        Color color = Colors[selectionHistory[indexToMoveTo].toGroup];
        Color nextColor;
        do
        {
            GoForwardOneStepInHistory();
            indexToMoveTo++;
            if (indexToMoveTo < selectionHistory.Count)
            {
                nextColor = Colors[selectionHistory[indexToMoveTo].toGroup];
            }
            else
            {
                break;
            }
        } while (color.Equals(nextColor));
    }

    public void SingleSelect(Collider other)
    {
        Color transparentColor = new Color(selectedColor.r, selectedColor.g, selectedColor.b);
        other.gameObject.GetComponent<Renderer>().material.color = transparentColor;
        GraphPoint gp = other.GetComponent<GraphPoint>();
        if (!selectedCells.Contains(gp))
        {
            selectedCells.Add(gp);
        }
        if (!selectionMade)
        {
            selectionMade = true;
            //UpdateButtonIcons();
        }
    }

    /// <summary>
    /// Adds rigidbody to all selected cells, making them fall to the ground.
    /// </summary>
    public void ConfirmRemove()
    {
        //GetComponent<AudioSource>().Play();
        foreach (GraphPoint other in selectedCells)
        {
            other.transform.parent = null;
            other.gameObject.AddComponent<Rigidbody>();
            other.GetComponent<Rigidbody>().useGravity = true;
            other.GetComponent<Rigidbody>().isKinematic = false;
            other.GetComponent<Collider>().isTrigger = false;
            other.Outline(Color.clear);
        }
        selectionHistory.Clear();
        CellExAlEvents.SelectionCanceled.Invoke();
        selectedCells.Clear();
        selectionMade = false;
        //selectionToolMenu.RemoveSelection();
    }

    /// <summary>
    /// Confirms a selection and dumps the relevant data to a .txt file.
    /// </summary>
    public void ConfirmSelection()
    {
        // create .txt file with latest selection
        DumpData();
        lastSelectedCells.Clear();
        StartCoroutine(UpdateRObjectGrouping());
        foreach (GraphPoint c in selectedCells)
        {
            c.Outline(Color.clear);
            lastSelectedCells.Add(c.gameObject.GetComponent<GraphPoint>());
        }
        // clear the list since we are done with it
        previousSelectionMenu.CreateButton(selectedCells);
        selectedCells.Clear();
        selectionHistory.Clear();
        CellExAlEvents.SelectionConfirmed.Invoke();
        heatmapCreated = false;
        selectionMade = false;
        selectionConfirmed = true;
        //selectionToolMenu.ConfirmSelection();
    }

    private IEnumerator UpdateRObjectGrouping()
    {
        string rScriptFilePath = Application.streamingAssetsPath + @"\R\update_grouping.R";
        string args = CellExAlUser.UserSpecificFolder + "\\selection" + (fileCreationCtr - 1) + ".txt " + CellExAlUser.UserSpecificFolder + " " + DataDir;
        CellExAlLog.Log("Updating R Object grouping at " + CellExAlUser.UserSpecificFolder);
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Thread t = new Thread(() => RScriptRunner.RunFromCmd(rScriptFilePath, args));
        t.Start();
        while (t.IsAlive)
        {
            yield return null;
        }
        stopwatch.Stop();
        CellExAlLog.Log("Updating R Object finished in " + stopwatch.Elapsed.ToString());
    }

    /// <summary>
    /// Gets the last selection that was confirmed.
    /// </summary>
    /// <returns> A List of all graphpoints that were selected. </returns>
    public List<GraphPoint> GetLastSelection()
    {
        return lastSelectedCells;
    }

    /// <summary>
    /// Get the current (not yet confirmed) selection.
    /// </summary>
    /// <returns> A List of all graphpoints currently selected. </returns>
    public List<GraphPoint> GetCurrentSelection()
    {
        return selectedCells;
    }

    /// <summary>
    /// Unselects anything selected.
    /// </summary>
    public void CancelSelection()
    {
        foreach (GraphPoint other in selectedCells)
        {
            other.ResetColor();
        }
        CellExAlEvents.SelectionCanceled.Invoke();
        historyIndexOffset = selectionHistory.Count;
        selectedCells.Clear();
        selectionMade = false;
        //selectionToolMenu.UndoSelection();
    }

    /// <summary>
    /// Changes the color of the selection tool.
    /// </summary>
    /// <param name="dir"> The direction to move in the array of colors. true for increment, false for decrement </param>
    public void ChangeColor(bool dir)
    {
        if (currentColorIndex == Colors.Length - 1 && dir)
        {
            currentColorIndex = 0;
        }
        else if (currentColorIndex == 0 && !dir)
        {
            currentColorIndex = Colors.Length - 1;
        }
        else if (dir)
        {
            currentColorIndex++;
        }
        else
        {
            currentColorIndex--;
        }
        int buttonIndexLeft = currentColorIndex == 0 ? Colors.Length - 1 : currentColorIndex - 1;
        int buttonIndexRight = currentColorIndex == Colors.Length - 1 ? 0 : currentColorIndex + 1;
        radialMenu.buttons[1].ButtonIcon = buttonIcons[buttonIndexLeft];
        radialMenu.buttons[3].ButtonIcon = buttonIcons[buttonIndexRight];
        radialMenu.RegenerateButtons();
        selectedColor = Colors[currentColorIndex];
        controllerModelSwitcher.SwitchControllerModelColor(Colors[currentColorIndex]);
    }

    public void HeatmapCreated()
    {
        heatmapCreated = true;
    }

    public bool GetHeatmapCreated()
    {
        return heatmapCreated;
    }

    /// <summary>
    /// Dumps the current selection to a .txt file.
    /// </summary>
    private void DumpData()
    {
        // print(new System.Diagnostics.StackTrace());
        string filePath = CellExAlUser.UserSpecificFolder + "\\selection" + (fileCreationCtr++) + ".txt";
        using (StreamWriter file =
                   new StreamWriter(filePath))
        {
            CellExAlLog.Log("Dumping selection data to " + CellExAlLog.FixFilePath(filePath));
            CellExAlLog.Log("\tSelection consists of  " + selectedCells.Count + " points");
            if (selectionHistory != null)
                CellExAlLog.Log("\tThere are " + selectionHistory.Count + " entries in the history");
            foreach (GraphPoint gp in selectedCells)
            {
                file.Write(gp.Label);
                file.Write("\t");
                Color c = gp.Color;
                int r = (int)(c.r * 255);
                int g = (int)(c.g * 255);
                int b = (int)(c.b * 255);
                // writes the color as #RRGGBB where RR, GG and BB are hexadecimal values
                file.Write(string.Format("#{0:X2}{1:X2}{2:X2}\t", r, g, b));
                file.Write("\t");
                file.Write(gp.GraphName);
                file.Write("\t");
                file.Write(gp.CurrentGroup);
                file.WriteLine();
            }
            file.Flush();
            file.Close();
        }
    }

    /// <summary>
    /// Activates or deactivates all colliders on the selectiontool.
    /// </summary>
    /// <param name="enabled"> True if the selection tool should be activated, false if it should be deactivated. </param>
    public void SetSelectionToolEnabled(bool enabled)
    {
        if (enabled)
        {
            controllerModelSwitcher.SwitchControllerModelColor(Colors[currentColorIndex]);
        }
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = enabled;
        }
    }

    public bool IsSelectionToolEnabled()
    {
        return GetComponentInChildren<Collider>().enabled;
    }

    public Color GetColor(int index)
    {
        return Colors[index];
    }

}
