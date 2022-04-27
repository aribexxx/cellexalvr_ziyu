﻿using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;
using CellexalVR.General;
using CellexalVR.AnalysisLogic;
using CellexalVR.Interaction;

namespace CellexalVR.AnalysisObjects
{
    /// <summary>
    /// Represents one node in a network, it handles the coloring of the connections and part of the network creation process,
    /// </summary>
    public class NetworkNode : MonoBehaviour
    {
        public TextMeshPro geneName;
        public GameObject edgePrefab;
        public GameObject arcDescriptionPrefab;
        public Transform CameraToLookAt { get; set; }
        public NetworkCenter Center { get; set; }
        public Material standardMaterial;
        public Material highlightMaterial;
        private string label;
        public string Label
        {
            get { return label; }
            set
            {
                label = value;
                geneName.text = value;
            }
        }
        public Vector3[] LayoutPositions { get; set; } = new Vector3[2];

        public ReferenceManager referenceManager;
        public HashSet<NetworkNode> neighbours = new HashSet<NetworkNode>();
        public List<Tuple<NetworkNode, NetworkNode, LineRenderer, float>> edges = new List<Tuple<NetworkNode, NetworkNode, LineRenderer, float>>();


        private List<Color> edgeColors = new List<Color>();
        private Vector3 normalScale;
        private Vector3 largerScale;
        private bool controllerInside;
        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.Interaction.Toolkit.ActionBasedController rightController;
        private CellManager cellManager;
        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.InputDevice device;
        private bool edgesAdded;
        private float lineWidth;
        private string laserCollider = "[VRTK][AUTOGEN][RightControllerScriptAlias][StraightPointerRenderer_Tracer]";

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            GetComponent<Renderer>().sharedMaterial = standardMaterial;
            GetComponent<Collider>().enabled = false;
            normalScale = transform.localScale;
            largerScale = normalScale * 2f;
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            this.name = geneName.text;

            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
        }

        void Update()
        {
            // some math make the text not be mirrored
            transform.LookAt(2 * transform.position - CameraToLookAt.position);
        }

        public override bool Equals(object other)
        {
            var node = other as NetworkNode;
            if (node == null)
                return false;
            return label == node.label;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Tells this networknode that the networkcenter it belongs to is being brought back to the networkhandler.
        /// </summary>
        public void BringBack()
        {
            // the networkcenter turns off this networknode's collider, so the controller is not inside any longer.
            controllerInside = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            bool active = Center.Enlarged;
            //bool touched = other.gameObject.name.Equals(laserCollider) || (other.gameObject.CompareTag("GameController"));
            bool touched = other.gameObject.CompareTag("Smaller Controller Collider");
            //print(touched);
            if (active && touched && !Center.controllerInsideSomeNode)
            {
                Center.ToggleNodeColliders(false, gameObject.name);
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == this.name);
                controllerInside = Center.controllerInsideSomeNode = true;
                foreach (GameObject obj in objects)
                {
                    NetworkNode nn = obj.GetComponent<NetworkNode>();
                    nn.Highlight();
                    referenceManager.multiuserMessageSender.SendMessageHighlightNetworkNode(Center.Handler.name, Center.name, nn.geneName.text);
                }
            }
        }

        //private void OnTriggerStay(Collider other)
        //{
        //}

        private void OnTriggerClick()
        {
            // Open XR
            //device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside)
            {
                cellManager.ColorGraphsByGene(Label.ToLower(), referenceManager.graphManager.GeneExpressionColoringMethod);
                referenceManager.multiuserMessageSender.SendMessageColorGraphsByGene(Label.ToLower());
                controllerInside = Center.controllerInsideSomeNode = false;
                Center.ToggleNodeColliders(true, gameObject.name);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            //bool touched = other.gameObject.name.Equals(laserCollider) || (other.gameObject.CompareTag("GameController"));
            bool touched = other.gameObject.CompareTag("Smaller Controller Collider");
            if (touched)
            {
                Center.ToggleNodeColliders(true, gameObject.name);
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == this.name);
                controllerInside = Center.controllerInsideSomeNode = false;
                foreach (GameObject obj in objects)
                {
                    NetworkNode nn = obj.GetComponent<NetworkNode>();
                    nn.UnHighlight();
                    referenceManager.multiuserMessageSender.SendMessageUnhighlightNetworkNode(Center.Handler.name, Center.name, nn.geneName.text);
                }

            }

        }

        public void SetReferenceManager(ReferenceManager referenceManager)
        {
            this.referenceManager = referenceManager;
            cellManager = referenceManager.cellManager;
            rightController = referenceManager.rightController;
        }

        /// <summary>
        /// Adds a neighbour to this node. A neughbour should be a gene that is correlated to this node's gene.
        /// This will also add this node as the neighbour's neighbour, so it's basically a bidirectional edge between two vertices.
        /// A gene may have many neighbours.
        /// </summary>
        /// <param name="buddy"> The new neighbour </param>
        public void AddNeighbour(NetworkNode buddy, float pcor)
        {
            // add this connection both ways
            neighbours.Add(buddy);
            buddy.neighbours.Add(this);

            NetworkGenerator networkGenerator = referenceManager.networkGenerator;
            GameObject edge = Instantiate(edgePrefab);
            LineRenderer renderer = edge.GetComponent<LineRenderer>();
            edge.transform.parent = transform.parent;
            edge.transform.localPosition = Vector3.zero;
            edge.transform.rotation = Quaternion.identity;
            edge.transform.localScale = Vector3.one;
            // renderer.sharedMaterial = networkGenerator.LineMaterials[UnityEngine.Random.Range(0, LineMaterials.Length)];
            renderer.startWidth = renderer.endWidth = CellexalConfig.Config.NetworkLineWidth;
            renderer.enabled = false;
            var newEdge = new Tuple<NetworkNode, NetworkNode, LineRenderer, float>(this, buddy, renderer, pcor);
            edges.Add(newEdge);
            //edgeColors.Add(renderer.material.color);
            buddy.edges.Add(newEdge);
            //buddy.edgeColors.Add(renderer.material.color);

        }

        /// <summary>
        /// Makes this node and all outgoing edges big and white.
        /// </summary>
        public void Highlight()
        {
            GetComponent<Renderer>().sharedMaterial = highlightMaterial;
            transform.localScale = largerScale;
            foreach (var tuple in edges)
            {
                var line = tuple.Item3;
                line.material.color = Color.white;
                line.startWidth = line.endWidth = CellexalConfig.Config.NetworkLineWidth * 3;
            }
        }

        /// <summary>
        /// Makes this node and all outgoing edges small and whatever color they were before.
        /// </summary>
        public void UnHighlight()
        {
            GetComponent<Renderer>().sharedMaterial = standardMaterial;
            transform.localScale = normalScale;
            for (int i = 0; i < edges.Count; ++i)
            {
                var line = edges[i].Item3;
                line.material.color = edgeColors[i];
                line.startWidth = line.endWidth = CellexalConfig.Config.NetworkLineWidth;
            }
        }

        /// <summary>
        /// Returns a list of all connected nodes.
        /// </summary>
        public List<NetworkNode> AllConnectedNodes()
        {
            List<NetworkNode> result = new List<NetworkNode>();
            AllConnectedNodesRec(ref result);
            return result;
        }

        private void AllConnectedNodesRec(ref List<NetworkNode> result)
        {
            result.Add(this);
            foreach (var neighbour in neighbours)
            {
                if (!result.Contains(neighbour))
                {
                    neighbour.AllConnectedNodesRec(ref result);
                }
            }
        }

        /// <summary>
        /// Repositions the edges based on this nodes neighbours. Called many times when a layout is switched for a smooth transitions.
        /// </summary>
        public void RepositionEdges()
        {
            for (int i = 0; i < edges.Count; ++i)
            {
                edges[i].Item3.SetPositions(new Vector3[] { edges[i].Item1.transform.localPosition, edges[i].Item2.transform.localPosition });
            }
        }

        /// <summary>
        /// Colors the edges of this network node according to the coloring method defined in <see cref="CellexalConfig.Config"/>.
        /// </summary>
        public void ColorEdges()
        {
            float minNegPcor = Center.MinNegPcor;
            float maxNegPcor = Center.MaxNegPcor;
            float minPosPcor = Center.MinPosPcor;
            float maxPosPcor = Center.MaxPosPcor;

            var colors = referenceManager.networkGenerator.LineMaterials;
            if (CellexalConfig.Config.NetworkLineColoringMethod == 0)
            {
                int numColors = CellexalConfig.Config.NumberOfNetworkLineColors;
                foreach (var edge in edges)
                {
                    edge.Item3.enabled = true;
                    float pcor = edge.Item4;
                    if (pcor < 0f)
                    {
                        int colorIndex;
                        // these are some special cases that can make the index go out of bounds
                        if (pcor == minNegPcor)
                        {
                            colorIndex = 0;
                        }
                        else if (pcor == maxNegPcor)
                        {
                            colorIndex = (numColors / 2) - 1;
                            if (colorIndex < 0)
                            {
                                colorIndex = 0;
                            }
                        }
                        else
                        {
                            colorIndex = (int)((1 - ((pcor - maxNegPcor) / (minNegPcor - maxNegPcor))) * (numColors / 2));
                        }
                        edge.Item3.material.color = colors[colorIndex].color;
                        edgeColors.Add(colors[colorIndex].color);
                    }
                    else
                    {
                        int colorIndex;
                        if (pcor == maxPosPcor)
                        {
                            colorIndex = colors.Length - 1;
                        }
                        else
                        {
                            colorIndex = (int)(((pcor - minPosPcor) / (maxPosPcor - minPosPcor)) * (numColors / 2)) + (numColors / 2);
                        }
                        edge.Item3.material.color = colors[colorIndex].color;
                        edgeColors.Add(colors[colorIndex].color);
                    }
                }
            }
            else if (CellexalConfig.Config.NetworkLineColoringMethod == 1)
            {
                foreach (var edge in edges)
                {
                    edge.Item3.enabled = true;
                    int colorIndex = UnityEngine.Random.Range(0, colors.Length);
                    edge.Item3.material.color = colors[colorIndex].color;
                    edgeColors.Add(colors[colorIndex].color);
                }
            }
        }
    }
}