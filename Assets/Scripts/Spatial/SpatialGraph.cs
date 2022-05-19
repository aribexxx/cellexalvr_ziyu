﻿using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using static CellexalVR.AnalysisObjects.Graph;
using CellexalVR.MarchingCubes;
using CellexalVR.General;
using System;
using CellexalVR.AnalysisObjects;
using CellexalVR.Interaction;
using Valve.VR;
using UnityEngine.XR.Interaction.Toolkit;
using CellexalVR.AnalysisLogic;
using DG.Tweening;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// Represents a spatial graph that in turn consists of many slices. The spatial graph is the parent of the graph objects.
    /// </summary>
    public class SpatialGraph : MonoBehaviour
    {
        private GameObject contour;
        private Vector3 startPosition;
        private Rigidbody _rigidBody;
        private bool dispersing;
        private Vector3 positionBeforeDispersing;
        private Quaternion rotationBeforeDispersing;

        public bool slicesActive;
        public List<Graph> slices = new List<Graph>();
        public Dictionary<string, GraphPoint> points = new Dictionary<string, GraphPoint>();
        public GameObject chunkManagerPrefab;
        public GameObject contourParent;
        public Material opaqueMat;
        public ReferenceManager referenceManager;
        public GameObject replacementPrefab;
        public GameObject wirePrefab;
        public GameObject brainModel;
        public GameObject cubePrefab;

        private void Start()
        {
            startPosition = transform.position;
            _rigidBody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (GetComponent<XRGrabInteractable>().isSelected)
            {
                referenceManager.multiuserMessageSender.SendMessageMoveGraph(gameObject.name, transform.position,
                    transform.rotation, transform.localScale);
            }


            if (_rigidBody != null && _rigidBody.velocity.magnitude > 2f && !dispersing)
            {
                positionBeforeDispersing = transform.localPosition;
                rotationBeforeDispersing = transform.localRotation;
                StartCoroutine(DisperseSlices());

                // ActivateSlices();
            }

        }

        public IEnumerator AddSlices()
        {
            foreach (Graph graph in GetComponentsInChildren<Graph>())
            {
                foreach (BoxCollider bc in graph.GetComponents<BoxCollider>())
                {
                    Vector3 size = bc.size;
                    size.z += 0.01f;
                    bc.size = size;
                    bc.enabled = false;
                }

                foreach (KeyValuePair<string, Graph.GraphPoint> gpPair in graph.points)
                {
                    points[gpPair.Key] = gpPair.Value;
                }

                slices.Add(graph);
            }

            yield return null;
        }

        /// <summary>
        /// Places the slices in a grid pattern to be able to look at them all individually.
        /// </summary>
        /// <returns></returns>
        private IEnumerator DisperseSlices()
        {
            dispersing = true;
            _rigidBody.drag = 1;
            _rigidBody.angularDrag = 1;

            float time = 0;

            while (time <= 1.0f)
            {
                time += Time.deltaTime;
                yield return null;
            }

            _rigidBody.velocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;

            transform.LookAt(referenceManager.inputFolderGenerator.transform);
            double angle = (Math.PI * 1.1d);
            Vector3 center = Vector3.zero; // referenceManager.headset.transform.position;
            int slicesPerRow = slices.Count / 4;
            float yDiff = transform.position.y;
            float xPos;
            float yPos = (yDiff > 0f) ? -0.5f : -yDiff;
            float zPos;
            float radius = 4.0f;
            List<Vector3> slicePositions = new List<Vector3>();
            for (int j = 0; j < slices.Count; j++)
            {
                if (j % slicesPerRow == 0 && j > 0)
                {
                    angle = (Math.PI * 1.1d);
                    radius += 0.1f;
                    yPos += 1.0f;
                }

                xPos = center.x + (float) Math.Cos(angle) * radius;
                zPos = center.z + (float) Math.Sin(angle) * radius / 2f;
                Vector3 pos = new Vector3(xPos, yPos, zPos);
                slicePositions.Add(pos);
                angle += (Math.PI * 0.9d) / (double) slicesPerRow;
            }

            float animationTime = 1f;
            GraphSlice gs;
            for (int i = 0; i < slices.Count; i++)
            {
                gs = slices[i].GetComponent<GraphSlice>();
                Vector3 pos = slicePositions[i];
                gs.transform.DOLocalMove(pos, 0.8f).SetEase(Ease.InOutQuad);
            }

            while (time < 1f + animationTime)
            {
                time += Time.deltaTime;
                yield return null;
            }

            ActivateSlices(false);
        }

        /// <summary>
        /// Move graph back to position before slices where dispersed.
        /// </summary>
        /// <returns></returns>
        private IEnumerator GatherSlices()
        {
            yield return new WaitForSeconds(1f);
            transform.localScale = Vector3.one;
            float animationTime = 1f;
            float t = 0;
            Vector3 startPosition = transform.localPosition;
            Quaternion startRotation = transform.localRotation;
            while (t < animationTime)
            {
                float progress = Mathf.SmoothStep(0, animationTime, t);
                transform.localPosition = Vector3.Lerp(startPosition, positionBeforeDispersing, progress);
                transform.localRotation =
                    Quaternion.Lerp(startRotation, rotationBeforeDispersing, progress);
                t += (Time.deltaTime / animationTime);
                yield return null;
            }

            dispersing = false;
        }

        private IEnumerator FlipSlices()
        {
            foreach (Graph graph in slices)
            {
                GraphSlice slice = graph.GetComponent<GraphSlice>();
                StartCoroutine(slice.FlipSlice(1f));
                yield return new WaitForSeconds(0.01f);
            }
        }

        /// <summary>
        /// Activate/Deactive slicemode. Activating means making each slice of the graph interactable independently of the others.
        /// Deactivating will reorganise them back to their original orientation and they will be moved as one object.
        /// </summary>
        public void ActivateSlices(bool move = true)
        {
            foreach (GraphSlice gs in GetComponentsInChildren<GraphSlice>())
            {
                if (!slicesActive)
                {
                    Destroy(_rigidBody);
                    Destroy(GetComponent<Collider>());
                    gs.ActivateSlice(true, move);
                }
                else
                {
                    Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
                    if (rigidbody == null)
                    {
                        rigidbody = gameObject.AddComponent<Rigidbody>();
                    }

                    _rigidBody = rigidbody;
                    _rigidBody.useGravity = false;
                    _rigidBody.isKinematic = false;
                    _rigidBody.drag = 10;
                    _rigidBody.angularDrag = 15;
                    gs.ActivateSlice(false, move);
                    ResetSlices();
                    BoxCollider collider = GetComponent<BoxCollider>();
                    if (collider == null)
                    {
                        gameObject.AddComponent<BoxCollider>();
                    }
                }
            }

            slicesActive = !slicesActive;
        }

        public void ToggleGraphPointsTransparency(bool toggle)
        {
            foreach (Graph graph in slices)
            {
                graph.MakeAllPointsTransparent(toggle);
            }

            TextureHandler.instance.MakeAllPointsTransparent(toggle);
        }

        /// <summary>
        /// Reset the slices back to their original position inside the parent object.
        /// </summary>
        private void ResetSlices()
        {
            foreach (GraphSlice gs in GetComponentsInChildren<GraphSlice>())
            {
                gs.MoveToGraph();
            }

            if (dispersing)
            {
                StartCoroutine(GatherSlices());
            }
        }

        public GraphSlice GetSlice(string sliceName)
        {
            foreach (GraphSlice slice in GetComponentsInChildren<GraphSlice>())
            {
                if (slice.gameObject.name.Equals(sliceName))
                    return slice;
            }

            return null;
        }

        public void ResetPosition()
        {
            transform.position = startPosition;
        }

        public void ResetSizeAndRotation()
        {
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }
    }
}