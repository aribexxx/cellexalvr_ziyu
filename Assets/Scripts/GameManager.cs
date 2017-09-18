﻿using System;
using System.Collections;


using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is responsible for passing commands that are about to be sent to a connected client.
/// </summary>
public class GameManager : Photon.PunBehaviour
{
    #region Public Properties

    public ReferenceManager referenceManager;
    static public GameManager Instance;
    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;
    public GameObject serverCoordinatorPrefab;

    public GraphManager GraphManager;
    public CellManager CellManager;
    public SelectionToolHandler SelectionToolHandler;
    public HeatmapGenerator HeatmapGenerator;
    private ServerCoordinator serverCoordinator;
    private ServerCoordinator clientCoordinator;

    #endregion

    #region Photon Messages
    public void InformGraphPointChangedColor(string graphname, string label, Color color)
    {
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("InformGraphpointChangedColor", PhotonTargets.Others, graphname, label, color.r, color.g, color.b);
        }
        else
        {
            serverCoordinator.photonView.RPC("InformGraphpointChangedColor", PhotonTargets.Others, graphname, label, color.r, color.g, color.b);
        }
    }

    public void InformConfirmSelection()
    {
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendConfirmSelection", PhotonTargets.Others);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendConfirmSelection", PhotonTargets.Others);
        }
    }

    public void InformMoveGraph(string moveGraphName, Vector3 pos, Quaternion rot)
    {
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendMoveGraph", PhotonTargets.Others, moveGraphName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendMoveGraph", PhotonTargets.Others, moveGraphName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w);
        }
    }

    public void InformMoveHeatmap(string moveHeatmapName, Vector3 pos, Quaternion rot)
    {
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendMoveHeatmap", PhotonTargets.Others, moveHeatmapName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendMoveHeatmap", PhotonTargets.Others, moveHeatmapName, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w);
        }
    }

    public void InformSelectedAdd(string graphName, string label)
    {
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendAddSelect", PhotonTargets.Others, graphName, label);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendAddSelect", PhotonTargets.Others, graphName, label);
        }
    }

    public void InformCreateHeatmap()
    {
        if (PhotonNetwork.isMasterClient)
        {
            clientCoordinator.photonView.RPC("SendCreateHeatmap", PhotonTargets.Others);
        }
        else
        {
            serverCoordinator.photonView.RPC("SendCreateHeatmap", PhotonTargets.Others);
        }
    }

    public void DoGraphpointChangeColor(string graphname, string label, Color col)
    {
        GraphManager.RecolorGraphPoint(graphname, label, col);
    }

    public void DoMoveGraph(string moveGraphName, float x, float y, float z, float rotX, float rotY, float rotZ, float rotW)
    {
        Graph g = GraphManager.FindGraph(moveGraphName);
        g.transform.position = new Vector3(x, y, z);
        g.transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
    }


    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer other)
    {

        Debug.Log("OnPhotonPlayerConnected() " + other.NickName); // not seen if you're the player connecting


        Debug.Log("MASTER JOINED ROOM");
        //LoadArena();
        StartCoroutine(FindClientCoordinator());

    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("CLIENT JOINED ROOM");
        StartCoroutine(FindServerCoordinator());
    }

    private IEnumerator FindClientCoordinator()
    {
        yield return new WaitForSeconds(1f);
        clientCoordinator = GameObject.Find("ClientCoordinator(Clone)").GetComponent<ServerCoordinator>();
        Debug.Log("Client Coordinator Found");
        //while (serverCoordinator == null)
        //{
        //    yield return new WaitForSeconds(1f);
        //    serverCoordinator = GameObject.Find("ServerCoordinator(Clone)").GetComponent<ServerCoordinator>();
        //    Debug.Log("Server Coordinator Found");
        //}
    }

    private IEnumerator FindServerCoordinator()
    {
        yield return new WaitForSeconds(1f);
        serverCoordinator = GameObject.Find("ServerCoordinator(Clone)").GetComponent<ServerCoordinator>();
        Debug.Log("Server Coordinator Found");
    }


    public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        Debug.Log("OnPhotonPlayerDisconnected() " + other.NickName); // seen when other disconnects


        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("OnPhotonPlayerDisonnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected


            //LoadArena();
        }
    }


    #endregion

    #region Public Methods

    private void Start()
    {
        GraphManager = referenceManager.graphManager;
        CellManager = referenceManager.cellManager;
        SelectionToolHandler = referenceManager.selectionToolHandler;
        HeatmapGenerator = referenceManager.heatmapGenerator;

        Instance = this;
        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            Debug.Log("We are Instantiating LocalPlayer from " + SceneManager.GetActiveScene().name);
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            if (PlayerManager.LocalPlayerInstance == null)
            {
                Debug.Log("We are Instantiating LocalPlayer from " + SceneManager.GetActiveScene().name);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                if (PhotonNetwork.isMasterClient)
                {
                    serverCoordinator = PhotonNetwork.Instantiate(this.serverCoordinatorPrefab.name, Vector3.zero, Quaternion.identity, 0).GetComponent<ServerCoordinator>();
                    // serverCoordinator.RegisterClient(this);
                }
                else
                {
                    PhotonNetwork.Instantiate("ClientCoordinator", Vector3.zero, Quaternion.identity, 0);

                    //GameObject.Find("ServerCoordinator").GetComponent<ServerCoordinator>().RegisterClient(this);
                }
            }
            else
            {
                Debug.Log("Ignoring scene load for " + SceneManager.GetActiveScene().name);
            }
        }
    }


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    #region Private Methods

    void LoadArena()
    {
        if (!PhotonNetwork.isMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.Log("PhotonNetwork : Loading Level : PUN_vrjeans_scene");
        PhotonNetwork.LoadLevel("PUN_vrjeans_scene");
    }


    #endregion
}
