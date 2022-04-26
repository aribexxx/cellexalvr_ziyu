using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CellexalVR.Spatial
{

    public class GeoMXImageRaycaster : MonoBehaviour
    {
        private GeoMXImageHandler imageHandler;
        private bool block = true;
        private GeoMXSlide currentSlideHit;

        private void Start()
        {
            imageHandler = GetComponent<GeoMXImageHandler>();
            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
            CellexalEvents.LoadingImages.AddListener(() => block = true);
            CellexalEvents.ImagesLoaded.AddListener(() => block = false);
        }

        private void OnTriggerClick()
        {
            if (block)
                return;
            Transform rLaser = ReferenceManager.instance.rightLaser.transform;
            Physics.Raycast(rLaser.position, rLaser.forward, out RaycastHit hit);
            if (hit.collider != null)
            {
                GeoMXSlide slide = hit.collider.GetComponent<GeoMXSlide>();
                if (slide != null && ReferenceManager.instance.rightLaser.enabled)
                {
                    slide.Select();
                }
            }
        }

        private void Update()
        {
            Raycast();
        }

        private void Raycast()
        {
            if (block)
                return;
            Transform rLaser = ReferenceManager.instance.rightLaser.transform;
            Physics.Raycast(rLaser.position, rLaser.forward, out RaycastHit hit, 10, 1 << LayerMask.NameToLayer("EnvironmentButtonLayer"));
            if (hit.collider)
            {
                GeoMXSlide slide = hit.collider.GetComponent<GeoMXSlide>();
                if (slide != null)
                {
                    slide.ShowName();
                    if (currentSlideHit == slide)
                        return;
                    slide.OnRaycastHit();
                    currentSlideHit = slide;
                }
                else
                {
                    imageHandler.ResetDisplayName();
                    //imageHandler.ResetHighlightedCells();
                    currentSlideHit = null;
                }
            }
            else
            {
                //imageHandler.ResetHighlightedCells();
                imageHandler.ResetDisplayName();
                currentSlideHit = null;
            }
        }
    }
}
