using UnityEngine;

///<summary>
/// Represents a button used for resetting the input data folders.
///</summary>
public class ResetFolderButton : CellexalButton
{
    //private GraphManager graphManager;
    //private InputFolderGenerator inputFolderGenerator;
    //private LoaderController loader;
    //private PreviousSearchesList previousSearchesList;
    //private GameObject inputFolderList;
    //private HeatmapGenerator heatmapGenerator;

    protected override string Description
    {
        get
        {
            return "Go back to loading a folder";
        }
    }

    private void Start()
    {
        //graphManager = referenceManager.graphManager;
        //inputFolderGenerator = referenceManager.inputFolderGenerator;
        //loader = referenceManager.loaderController;
        //previousSearchesList = referenceManager.previousSearchesList;
        //inputFolderList = referenceManager.inputFolderGenerator.gameObject;
        //heatmapGenerator = referenceManager.heatmapGenerator;
    }

    // Reset everything without clicking the button.
    public void Reset()
    {
        Click();
    }

    protected override void Click()
    {
        //var sceneLoader = GameObject.Find ("Load").GetComponent<Loading> ();
        //sceneLoader.doLoad = false;
        referenceManager.loaderController.ResetFolders();
        referenceManager.gameManager.InformLoadingMenu();
        //loader.ResetFolders();
        //graphManager.DeleteGraphsAndNetworks();
        //heatmapGenerator.DeleteHeatmaps();
        //previousSearchesList.ClearList();
        //// must reset loader before generating new folders
        //loader.ResetLoaderBooleans();
        //inputFolderGenerator.GenerateFolders();
        //inputFolderList.gameObject.SetActive(true);
        //CellexalEvents.GraphsUnloaded.Invoke();
        //if (loader.loaderMovedDown)
        //{
        //    loader.loaderMovedDown = false;
        //    loader.MoveLoader(new Vector3(0f, 2f, 0f), 2f);
        //}
    }
}
