﻿/// <summary>
/// This class represents the buttons that increase and decrease the number of frames between each gene expression when flashing genes.
/// </summary>
namespace Assets.Scripts.MenuButtonScripts
{
    class ChangeFlashGenesModeButton : StationaryButton
    {
        protected override string Description
        {
            get { return "Change the mode"; }
        }

        public CellManager.FlashGenesMode switchToMode;

        private CellManager cellManager;

        private void Start()
        {
            cellManager = referenceManager.cellManager;
        }

        void Update()
        {
            device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                cellManager.CurrentFlashGenesMode = switchToMode;
            }
        }
    }
}