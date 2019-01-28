using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumperPlayerManager : Photon.PunBehaviour, IPunObservable
{

    #region Public Variables

    [Tooltip("The current Health of our player")]
    public float Health = 1f;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    [Tooltip("The player's charater object/model/logic etc to spawn in")]
    public GameObject playerCharacterPrefab;

    [Tooltip("The Player's UI GameObject Prefab")]
    public GameObject PlayerUiPrefab;

    //[Tooltip("Canvas prefab to spawn in levels")]
    //public GameObject canvasPrefab;


    #endregion

    #region Private Variables

    //True, when the user is firing
    bool IsFiring;
    GameObject uiRef;
    Camera _ManagerCamera;
    GameObject playerCharacter;
    //GameObject canvasRef;

    #endregion

    #region MonoBehaviour CallBacks

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    void Awake()
    {

        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.isMine)
        {
            JumperPlayerManager.LocalPlayerInstance = this.gameObject;
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);

    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {
        /*
        CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();

        //Debug.Log(this.gameObject.GetComponent<CameraWork>());

        if (_cameraWork != null)
        {
            if (photonView.isMine)
            {
                _cameraWork.OnStartFollowing();
                //Debug.Log("playermanager _cameraWork.OnStartFollowing()");
            }
        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }
        */
        /*
#if UNITY_5_4_OR_NEWER
        // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, loadingMode) =>
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        };
#endif
*/
        //_ManagerCamera = GetComponent<Camera>();
        _ManagerCamera = GetComponentInChildren<Camera>();

        //_ManagerCamera.enabled = false;

        if (PlayerUiPrefab != null)
        {
            GameObject _uiGo = Instantiate(PlayerUiPrefab) as GameObject;
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }
        else
        {
            Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
        }

    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity on every frame.
    /// </summary>
    void Update()
    {
        if (PhotonNetwork.connected)
        {
            if (photonView.isMine)
            {
                ProcessInputs();
            }
            else
            {
                _ManagerCamera.enabled = false;
            }
        }

        if (playerCharacter)
        {
            // Look though the player's eyes, not the manager's eyes
            _ManagerCamera.enabled = false;
        }
        else
        {
            if (!PhotonNetwork.connected || photonView.isMine)
            {
                _ManagerCamera.enabled = true;
            }
        }

        // TODO: Make a menu to spawn characters
        if (!playerCharacter)
        {
            // Spawn character

            if (PhotonNetwork.connected == true)
            {
                if (photonView.isMine == true)
                {
                    playerCharacter = PhotonNetwork.Instantiate(this.playerCharacterPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                }
            } else { 
                Debug.LogError("Not connected to network; spawning local player character.");
                playerCharacter = Instantiate(playerCharacterPrefab, new Vector3(0f, 5f, 0f), Quaternion.identity);
            }
        }
        


        if (Health <= 0f)
        {
            //JumperGameManager.Instance.LeaveRoom();
        }

        if (PlayerUiPrefab != null && !uiRef)
        {

            //GameObject _uiGo = Instantiate(this.PlayerUiPrefab) as GameObject;
            //_uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);

            uiRef = Instantiate(this.PlayerUiPrefab) as GameObject;
            uiRef.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            Debug.Log("Spawned new player UI");
        }
    }

    /// <summary>
    /// MonoBehaviour method called when the Collider 'other' enters the trigger.
    /// Affect Health of the Player if the collider is a beam
    /// Note: when jumping and firing at the same, you'll find that the player's own beam intersects with itself
    /// One could move the collider further away to prevent this or check if the beam belongs to the player.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {


        if (!photonView.isMine)
        {
            return;
        }


        // We are only interested in Beamers
        // we should be using tags but for the sake of distribution, let's simply check by name.
        if (!other.name.Contains("Beam"))
        {
            return;
        }


        //Health -= beamDamage;
    }


    /// <summary>
    /// MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
    /// We're going to affect health while the beams are touching the player
    /// </summary>
    /// <param name="other">Other.</param>
    void OnTriggerStay(Collider other)
    {


        // we dont' do anything if we are not the local player.
        if (!photonView.isMine)
        {
            return;
        }


        // We are only interested in Beamers
        // we should be using tags but for the sake of distribution, let's simply check by name.
        if (!other.name.Contains("Beam"))
        {
            return;
        }


        // we slowly affect health when beam is constantly hitting us, so player has to move to prevent death.
        //Health -= beamDamage * Time.deltaTime;
    }

#if !UNITY_5_4_OR_NEWER
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        void OnLevelWasLoaded(int level)
        {
           this.CalledOnLevelWasLoaded(level);
        }
#endif

    void CalledOnLevelWasLoaded(int level)
    {
        // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
        if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
        {
            transform.position = new Vector3(0f, 5f, 0f);
        }

        //GameObject _uiGo = Instantiate(this.PlayerUiPrefab) as GameObject;
        //_uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);

        //Debug.Log("playermanager CalledOnLevelWasLoaded  - Instantiate(this.PlayerUiPrefab)");
    }

    #endregion

    #region Custom

    /// <summary>
    /// Processes the inputs. Maintain a flag representing when the user is pressing Fire.
    /// </summary>
    void ProcessInputs()
    {

        if (Input.GetButtonDown("Fire1"))
        {
            if (!IsFiring)
            {
                IsFiring = true;
            }
        }

        if (Input.GetButtonUp("Fire1"))
        {
            if (IsFiring)
            {
                IsFiring = false;
            }
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        /*
        if (stream.isWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(IsFiring);
            stream.SendNext(Health);
        }
        else
        {
            // Network player, receive data
            this.IsFiring = (bool)stream.ReceiveNext();
            this.Health = (float)stream.ReceiveNext();
            //Debug.Log("this.IsFiring " + this.IsFiring + " this.Health " + this.Health);
        }
        */
    }
    #endregion

}

