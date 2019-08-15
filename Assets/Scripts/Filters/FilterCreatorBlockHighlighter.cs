﻿using UnityEngine;
using System.Collections;
using CellexalVR.General;

namespace CellexalVR.Filters
{
    /// <summary>
    /// Represents a zone on a <see cref="FilterCreatorBlock"/> that can be highlighted. Highlighting is done by offsetting the texture to a part that has a zone highlighted is shown.
    /// </summary>
    public class FilterCreatorBlockHighlighter : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public FilterCreatorBlock parent;
        public int section;
        public bool isAttributeInclude = false;
        public CellexalVR.Interaction.KeyboardHandler keyboardToUse;
        public TMPro.TextMeshPro textmeshpro;

        private SteamVR_TrackedObject rightController;
        private bool controllerInside;

        private void Start()
        {
            rightController = referenceManager.rightController;
        }

        private void OnValidate()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            parent = transform.parent?.GetComponent<FilterCreatorBlock>();
            textmeshpro = gameObject.GetComponent<TMPro.TextMeshPro>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Controller"))
            {
                parent.HighlightedSection = section;
                controllerInside = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Controller"))
            {
                controllerInside = false;
                if (parent.HighlightedSection == section)
                {
                    parent.HighlightedSection = 0;
                }
            }
        }

        private void Update()
        {
            var device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                if (!isAttributeInclude)
                {
                    keyboardToUse.gameObject.SetActive(true);
                    keyboardToUse.output = textmeshpro;
                }
                else
                {
                    textmeshpro.text = textmeshpro.text == "included" ? "not included" : "included";
                }
            }
        }
    }
}

