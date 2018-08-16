﻿using System;

public class AddAttributeLogicToSelectionButton : CellexalButton
{

    protected override string Description
    {
        get { return "Add the currently shown attributes as a new group"; }
    }

    protected override void Click()
    {
        referenceManager.attributeSubMenu.AddCurrentExpressionAsGroup();
    }
}