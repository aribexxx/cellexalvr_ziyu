﻿using CellexalVR.General;
using System.Linq;
using UnityEngine;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Keyboard for web browser. Enter key sends output to navigate function in web browser.
    /// </summary>
    public class WebManager : MonoBehaviour
    {

        public SimpleWebBrowser.WebBrowser webBrowser;
        public TMPro.TextMeshPro output;

        private ReferenceManager referenceManager;

        // Use this for initialization
        void Start()
        {
            SetVisible(false);
            referenceManager = webBrowser.referenceManager;
        }

        public void EnterKey()
        {
            print("Navigate to - " + output.text);
            // If url field does not contain '.' then may not be a url so google the output instead
            if (!output.text.Contains('.'))
            {
                output.text = "www.google.com/search?q=" + output.text;
            }
            webBrowser.OnNavigate(output.text);
            referenceManager.gameManager.InformBrowserEnter();
        }

        public void SetVisible(bool visible)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = visible;
            }
            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                c.enabled = visible;
            }
            //webBrowser.enabled = visible;
        }

    }
}