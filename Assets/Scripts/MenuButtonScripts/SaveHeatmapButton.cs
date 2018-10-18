﻿using UnityEngine;
/// <summary>
/// Represents the button that saves the heatmap it is attached to the disk.
/// </summary>
class SaveHeatmapButton : CellexalButton
{
    protected override string Description
    {
        get { return "Save heatmap\nimage to disk"; }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Click()
    {
        gameObject.GetComponentInParent<Heatmap>().SaveImage();
        device.TriggerHapticPulse(2000);
    }
}
