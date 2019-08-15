﻿using CellexalVR.AnalysisLogic;
using UnityEngine;

namespace CellexalVR.Filters
{

    public class FilterCreatorFacsBlock : FilterCreatorBlock
    {
        public GameObject block;
        public GameObject facsName;
        public GameObject operatorSign; // can't name it operator :(
        public GameObject value;
        public FilterCreatorBlockPort output;


        private string facsString;
        private string operatorString;
        private string valueString;
        private MeshRenderer meshRenderer;

        public override int HighlightedSection
        {
            get => base.HighlightedSection;
            set
            {
                base.HighlightedSection = value;
                meshRenderer.material.mainTextureOffset = new Vector2(value * 0.25f, 0f);
            }
        }

        protected override void Start()
        {
            base.Start();
            meshRenderer = block.GetComponent<MeshRenderer>();
            UpdateStrings();
        }

        private void UpdateStrings()
        {
            facsString = facsName.GetComponent<TMPro.TextMeshPro>().text;
            operatorString = operatorSign.GetComponent<TMPro.TextMeshPro>().text;
            valueString = value.GetComponent<TMPro.TextMeshPro>().text;
        }

        public override bool IsValid()
        {
            UpdateStrings();
            return facsString != "" && operatorString != "" && valueString != "";
        }

        public override string ToString()
        {
            UpdateStrings();
            return facsString + " " + operatorString + " " + valueString;
        }

        public override BooleanExpression.Expr ToExpr()
        {
            UpdateStrings();
            bool isPercent = valueString[valueString.Length - 1] == '%';
            float valueFloat;
            if (isPercent)
            {
                valueFloat = float.Parse(valueString.Substring(0, valueString.Length - 1));
            }
            else
            {
                valueFloat = float.Parse(valueString);
            }
            return new BooleanExpression.FacsExpr(facsString, BooleanExpression.ParseToken(operatorString, 0), valueFloat, isPercent);
        }
    }
}
