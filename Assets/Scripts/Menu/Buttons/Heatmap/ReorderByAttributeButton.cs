namespace CellexalVR.Menu.Buttons.Heatmap
{
    ///<summary>
    /// Represents a button that is used to reorder the attribute bar on the heatmap. 
    /// Click once to order by attribute and again to go back to original order.
    ///</summary>
    public class ReorderByAttributeButton : CellexalButton
    {
        // private bool toggle;
        private CellexalVR.AnalysisObjects.Heatmap heatmap;
        private CellexalVR.AnalysisLogic.HeatmapGenerator heatmapGenerator;

        protected override string Description => "";
            //!heatmap.orderedByAttribute ? "Reorder heatmap so attribute bar is sorted in each group"
            //    : "Switch back to original ordering";

        protected override void Awake()
        {
            base.Awake();
            heatmap = GetComponentInParent<CellexalVR.AnalysisObjects.Heatmap>();
            heatmapGenerator = referenceManager.heatmapGenerator;
        }


        public override void Click()
        {
            referenceManager.multiuserMessageSender.SendMessageReorderByAttribute(heatmap.gameObject.name, !heatmap.orderedByAttribute);
            if (!heatmap.orderedByAttribute)
            {
                heatmap.ReorderByAttribute();
            }
            else
            {
                heatmapGenerator.BuildTexture(heatmap.selection, "", heatmap, false);
            }
        }
    }
}