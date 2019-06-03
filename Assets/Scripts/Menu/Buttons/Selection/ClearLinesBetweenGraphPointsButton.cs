﻿using CellexalVR.AnalysisLogic;
using CellexalVR.General;

namespace CellexalVR.Menu.Buttons.Selection
{
    /// <summary>
    /// Represents the button that clears lines drawn between graphs.
    /// </summary>
    public class ClearLinesBetweenGraphPointsButton : CellexalButton
    {

        private CellManager cellManager;

        protected override string Description
        {
            get { return "Clear lines between all cells with the same label in other graphs"; }
        }

        private void Start()
        {
            cellManager = referenceManager.cellManager;
            SetButtonActivated(false);
            CellexalEvents.LinesBetweenGraphsDrawn.AddListener(TurnOn);
            CellexalEvents.LinesBetweenGraphsCleared.AddListener(TurnOff);
        }

        public override void Click()
        {
            cellManager.ClearLinesBetweenGraphPoints();
            SetButtonActivated(false);
            referenceManager.gameManager.InformClearLinesBetweenGps();
        }

        private void TurnOn()
        {
            SetButtonActivated(true);
        }

        private void TurnOff()
        {
            SetButtonActivated(false);
        }
    }
}