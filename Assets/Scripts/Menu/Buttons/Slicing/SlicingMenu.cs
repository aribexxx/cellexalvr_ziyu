﻿using System;
using System.Collections;
using CellexalVR.Spatial;
using Unity.Entities;
using UnityEngine;
using DG.Tweening;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class SlicingMenu : MonoBehaviour
    {
        public GameObject automaticModeMenu;
        public GameObject manualModeMenu;
        public GameObject freeHandModeMenu;
        public SlicingModeToggleButton manualModeButton;
        public SlicingModeToggleButton autoModeButton;
        public SlicingModeToggleButton fromSelectionModeButton;
        public ChangeNrOfSlicesButton addSliceButton;
        public ChangeNrOfSlicesButton subtractSliceButton;
        public int nrOfSlices;
        public GameObject movableContent_meshCulling;

        public enum SliceMode
        {
            None,
            Automatic,
            Manual,
            Freehand,
        }

        public enum SliceAxis
        {
            X = 0,
            Y = 1,
            Z = 2,
        }

        private SliceMode currentMode;
        public SliceAxis currentAxis;
        [HideInInspector] public SlicerBox slicerBox;
        private SliceGraphButton sliceGraphButton;

        private void Start()
        {
            slicerBox = GetComponentInParent<SlicerBox>();
            //ActivateMode(SliceMode.Automatic);
            sliceGraphButton = GetComponentInChildren<SliceGraphButton>();
            //sliceGraphSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SliceGraphSystem>();//GetComponentInParent<GraphSlicer>();
        }


        public void ToggleMode(SliceMode modeToActivate, bool toggle)
        {
            currentMode = toggle ? modeToActivate : currentMode;
            switch (modeToActivate)
            {
                case SliceMode.None:
                    automaticModeMenu.SetActive(false);
                    break;
                case SliceMode.Automatic:
                    if (toggle)
                    {
                        manualModeButton.CurrentState = !toggle;
                        fromSelectionModeButton.CurrentState = !toggle;
                    }
                    movableContent_meshCulling.transform.DOLocalMoveZ(toggle ? -5.3f : -1.3f, 0.5f).SetEase(Ease.InOutSine);
                    //StartCoroutine(MoveContent(toggle ? -5.3f : -1.3f));
                    automaticModeMenu.SetActive(toggle);
                    break;
                case SliceMode.Manual:
                    if (toggle)
                    {
                        autoModeButton.CurrentState = !toggle;
                        fromSelectionModeButton.CurrentState = !toggle;
                    }
                    slicerBox.sliceInteractable.SetActive(toggle);
                    break;
                case SliceMode.Freehand:
                    if (toggle)
                    {
                        autoModeButton.CurrentState = !toggle;
                        manualModeButton.CurrentState = !toggle;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(modeToActivate), modeToActivate, null);
            }
            if (!(manualModeButton.CurrentState || autoModeButton.CurrentState))
            {
                movableContent_meshCulling.transform.DOLocalMoveZ(-1.3f, 0.5f).SetEase(Ease.InOutSine);
                //StartCoroutine(MoveContent(-1.3f));
            }
        }

        //private IEnumerator MoveContent(float zCoord)
        //{
            //float animationTime = 0.5f;
            //float t = 0f;
            //Vector3 startPos = movableContent.transform.localPosition;
            //Vector3 targetPos = new Vector3(movableContent.transform.localPosition.x, movableContent.transform.localPosition.y, zCoord);
            //while (t < animationTime)
            //{
            //    //float progress = Mathf.SmoothStep(0, animationTime, t / animationTime);
            //    movableContent.transform.localPosition = Vector3.Lerp(startPos, targetPos, t / animationTime);
            //    t += (Time.deltaTime);
            //    yield return null;
            //}
            //yield return null;
        //}

        public void ChangeAxis(SliceAxis axis)
        {
            currentAxis = axis;
            foreach (ChangeSliceAxisButton b in GetComponentsInChildren<ChangeSliceAxisButton>())
            {
                b.SetButtonActivated(true);
            }
        }

        public void ChangeNrOfSlices(int dir)
        {
            nrOfSlices += dir;
            if (nrOfSlices + dir < 2)
            {
                subtractSliceButton.SetButtonActivated(false);
            }

            else
            {
                subtractSliceButton.SetButtonActivated(true);
            }
        }

        public void SliceGraph()
        {
            switch (currentMode)
            {
                case SliceMode.Automatic:
                    slicerBox.SliceGraphAutomatic((int)currentAxis, nrOfSlices);
                    break;
                case SliceMode.Manual:
                    slicerBox.SliceGraphManual();
                    break;
                case SliceMode.None:
                    break;
                case SliceMode.Freehand:
                    slicerBox.SliceGraphFromSelection();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ChangeMeshTransparancy(float val)
        {
            //MeshGenerator.instance.ChangeMeshTransparency(val);
        }

        public IEnumerator MinimizeMenu()
        {
            float time = 1f;
            float t = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = Vector3.zero;
            while (t < time)
            {
                float progress = Mathf.SmoothStep(0, time, t);
                transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
                t += (Time.deltaTime / time);
                yield return null;
            }
        }

        public void ToggleMenu(bool activate)
        {
            Vector3 targetScale = activate ? Vector3.one * 0.05f : Vector3.zero;
            transform.DOScale(targetScale, 0.5f).SetEase(Ease.InOutSine);
        }

        public IEnumerator MaximizeMenu()
        {
            float time = 1f;
            float t = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = Vector3.one * 0.05f;
            while (t < time)
            {
                float progress = Mathf.SmoothStep(0, time, t);
                transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
                t += (Time.deltaTime / time);
                yield return null;
            }
        }
    }

}
