using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JumperGameManager : Photon.PunBehaviour
{
    #region Public Properties

    static public JumperGameManager Instance;

    //[Tooltip("The prefab to use for representing the player")]
    //public GameObject playerPrefab;

    [Tooltip("The prefab representing the player manager (which will then spawn a player character)")]
    public GameObject playerManagerPrefab;

    #endregion

    #region Photon Messages


    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }


    #endregion

    void Start()
    {
        Instance = this;

        //if (playerPrefab == null)
        if (playerManagerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            if (JumperPlayerManager.LocalPlayerInstance == null)
            {
                /*
                Debug.Log("We are Instantiating LocalPlayer from " + Application.loadedLevelName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate

                PhotonNetwork.InstantiateInRoomOnly = false;

                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                */
                PhotonNetwork.InstantiateInRoomOnly = false;    // Doesn't seem to work

                GameObject playerManager = PhotonNetwork.Instantiate(this.playerManagerPrefab.name, new Vector3(0f, 0f, 0f), Quaternion.identity, 0);
                if (!playerManager)
                {
                    Debug.LogError("Failed to spawn playerManagerPrefab; Spawning now. Network connected status: " + PhotonNetwork.connected);
                    playerManager = Instantiate(playerManagerPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
                }
                playerManager.transform.parent = this.transform;
            }
            else
            {
                //Debug.Log("Ignoring scene load for " + Application.loadedLevelName);
            }
        }
    }

    #region Public Methods


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer other)
    {
        Debug.Log("OnPhotonPlayerConnected() " + other.NickName); // not seen if you're the player connecting


        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected


            //LoadArena();
        }
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


    #region Private Methods

    /*
    void LoadArena()
    {
        if (!PhotonNetwork.isMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.Log("PhotonNetwork : Loading Level : " + PhotonNetwork.room.PlayerCount);
        PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.room.playerCount);
    }
    */


    #endregion
}

