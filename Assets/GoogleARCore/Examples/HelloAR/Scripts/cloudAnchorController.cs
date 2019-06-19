using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using System;
using System.Reflection;
using ExitGames.Client.Photon;

public class cloudAnchorController : Photon.MonoBehaviour
{

    [Header("ARCore")]

    /// <summary>
    /// The UI Controller.
    /// </summary>
    //  public NetworkManagerUIController UIController;
    public GameObject AndyPlayer;
    /// <summary>
    /// The root for ARCore-specific GameObjects in the scene.
    /// </summary>
    public GameObject ARCoreRoot;
    private int nw;
    public ARCoreWorldOriginHelper ARCoreWorldOriginHelper;
    /// <summary>
    /// The helper that will calculate the World Origin offset when performing a raycast or
    /// generating planes.
    /// </summary>
    // public ARCoreWorldOriginHelper ARCoreWorldOriginHelper;
    //public 
    //   private bool //m_IsOriginPlaced = false;

    private bool m_IsOriginPlaced = false;


    private bool m_matchStarted=false;
    /// <summary>
    /// Indicates whether the Anchor was already instantiated.
    /// </summary>
    private bool m_AnchorAlreadyInstantiated = false;

    /// <summary>
    /// Indicates whether the Cloud Anchor finished hosting.
    /// </summary>
    private bool m_AnchorFinishedHosting = false;

    /// <summary>
    /// True if the app is in the process of quitting due to an ARCore connection error,
    /// otherwise false.
    /// </summary>
    private bool m_IsQuitting = false;

    /// <summary>
    /// The anchor component that defines the shared world origin.
    /// </summary>
    private Component m_WorldOriginAnchor = null;

    /// <summary>
    /// The last pose of the hit point from AR hit test.
    /// </summary>
    private Pose? m_LastHitPose = null;

    private ApplicationMode m_CurrentMode = ApplicationMode.Ready;
    /// <summary>
    /// The current cloud anchor mode.
    /// </summary>
    // private ApplicationMode m_CurrentMode = ApplicationMode.Ready;

    private bool m_MatchStarted = false;

    public enum ApplicationMode
    {
        Ready,
        Hosting,
        Resolving,
    }
    //.countOfPlayers;
    //   private PhotonNetwork p;

    public void Start()
    {
        // PhotonNetwork.
        // p.
        //  m_NetworkManager = UIController.GetComponent<NetworkManager>();
        PhotonNetwork.ConnectUsingSettings("v1.0");

                   
        nw = 0;
        // A Name is provided to the Game Object so it can be found by other Scripts
        // instantiated as prefabs in the scene.
        gameObject.name = "CloudAnchorsExampleController";
        ARCoreRoot.SetActive(false);
   //    ARKitRoot.SetActive(false);
     //   _ResetStatus();
    }

    public void Update()
    {
        //s  _UpdateApplicationLifecycle();

        UpdateLifeCycle();

        if (m_CurrentMode != ApplicationMode.Hosting &&
               m_CurrentMode != ApplicationMode.Resolving)
        {
            return;
        }
        if (m_CurrentMode == ApplicationMode.Resolving && !m_IsOriginPlaced)
        {
            return;
        }

        // If the player has not touched the screen then the update is complete.
        // If the player has not touched the screen then the update is complete.
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        TrackableHit arcoreHitResult = new TrackableHit();
        m_LastHitPose = null;

        // Raycast against the location the player touched to search for planes.
        if (Application.platform != RuntimePlatform.IPhonePlayer)
        {
            if (ARCoreWorldOriginHelper.Raycast(touch.position.x, touch.position.y,
                    TrackableHitFlags.PlaneWithinPolygon, out arcoreHitResult))
            {
                m_LastHitPose = arcoreHitResult.Pose;
            }
        }
        else
        {

        }

        // If there was an anchor placed, then instantiate the corresponding object.
        if (m_LastHitPose != null)
        {
            // The first touch on the Hosting mode will instantiate the origin anchor. Any
            // subsequent touch will instantiate a star, both in Hosting and Resolving modes.
            if (_CanPlaceStars())
            {
                _InstantiateStar();
            }
            else if (!m_IsOriginPlaced && m_CurrentMode == ApplicationMode.Hosting)
            {
                if (Application.platform != RuntimePlatform.IPhonePlayer)
                {
                    m_WorldOriginAnchor =
                        arcoreHitResult.Trackable.CreateAnchor(arcoreHitResult.Pose);
                }
                else
                {
                   
                }

                SetWorldOrigin(m_WorldOriginAnchor.transform);
                _InstantiateAnchor();
                OnAnchorInstantiated(true);
            }
        }
    }

    /// <summary>
    /// Sets the apparent world origin so that the Origin of Unity's World Coordinate System
    /// coincides with the Anchor. This function needs to be called once the Cloud Anchor is
    /// either hosted or resolved.
    /// </summary>
    /// <param name="anchorTransform">Transform of the Cloud Anchor.</param>
    public void SetWorldOrigin(Transform anchorTransform)
    {
        if (m_IsOriginPlaced)
        {
            _ShowAndroidToastMessage
            ("The World Origin can be set only once.");
            return;
        }

        m_IsOriginPlaced = true;

        if (Application.platform != RuntimePlatform.IPhonePlayer)
        {
            ARCoreWorldOriginHelper.SetWorldOrigin(anchorTransform);
        }
        else
        {
          // m_ARKit.SetWorldOrigin(anchorTransform);
        }
    }


    /// <summary>
    /// Handles user intent to enter a mode where they can place an anchor to host or to exit
    /// this mode if already in it.
    /// </summary>
    public void OnEnterHostingModeClick()
    {

        if (m_CurrentMode == ApplicationMode.Hosting)
        {
            m_CurrentMode = ApplicationMode.Ready;
            _ResetStatus();
            return;
        }
        PhotonNetwork.JoinRandomRoom();
        if (m_CurrentMode == ApplicationMode.Hosting)
        {
            m_CurrentMode = ApplicationMode.Ready;
            _ResetStatus();
            return;
        }
        OnPhotonRandomJoinFailed();
        m_CurrentMode = ApplicationMode.Hosting;
        _SetPlatformActive();
    }

 
    /// <summary>
    /// Handles a user intent to enter a mode where they can input an anchor to be resolved or
    /// exit this mode if already in it.
    /// </summary>
    public void OnEnterResolvingModeClick()
    {
        if (m_CurrentMode == ApplicationMode.Resolving)
        {
            m_CurrentMode = ApplicationMode.Ready;
            _ResetStatus();
            return;
        }
        PhotonNetwork.JoinRandomRoom();
        if (m_CurrentMode == ApplicationMode.Resolving)
        {
            m_CurrentMode = ApplicationMode.Ready;
            _ResetStatus();
            return;
        }
        _ShowAndroidToastMessage("Random Room Joined Failed");
        OnPhotonRandomJoinFailed();
        m_CurrentMode = ApplicationMode.Resolving;
        _SetPlatformActive();
    }

    /// <summary>
    /// Callback indicating that the Cloud Anchor was instantiated and the host request was
    /// made.
    /// </summary>
    /// <param name="isHost">Indicates whether this player is the host.</param>
    public void OnAnchorInstantiated(bool isHost)
    {
        if (m_AnchorAlreadyInstantiated)
        {
            return;
        }

        m_AnchorAlreadyInstantiated = true;
        // UIController.OnAnchorInstantiated(isHost);
        _ShowAndroidToastMessage("Anchor Initiated");
    }

    /// <summary>
    /// Callback indicating that the Cloud Anchor was hosted.
    /// </summary>
    /// <param name="success">If set to <c>true</c> indicates the Cloud Anchor was hosted
    /// successfully.</param>
    /// <param name="response">The response string received.</param>
    public void OnAnchorHosted(bool success, string response)
    {
        m_AnchorFinishedHosting = success;
        //  UIController.OnAnchorHosted(success, response);
        _ShowAndroidToastMessage("Hosted");
    }

    /// <summary>
    /// Callback indicating that the Cloud Anchor was resolved.
    /// </summary>
    /// <param name="success">If set to <c>true</c> indicates the Cloud Anchor was resolved
    /// successfully.</param>
    /// <param name="response">The response string received.</param>
    public void OnAnchorResolved(bool success, string response)
    {
        _ShowAndroidToastMessage("Anchor Resolved");
    }

    /// <summary>
    /// Instantiates the anchor object at the pose of the m_LastPlacedAnchor Anchor. This will
    /// host the Cloud Anchor.
    /// </summary>
    private void _InstantiateAnchor()
    {
        // The anchor will be spawned by the host, so no networking Command is needed.
        //   GameObject.Find("LocalPlayer").GetComponent<LocalPlayerController>()
        //        .SpawnAnchor(Vector3.zero, Quaternion.identity, m_WorldOriginAnchor);
        PhotonNetwork.Instantiate("AndyGreen", ARCoreWorldOriginHelper.ARCoreDeviceTransform.position, ARCoreWorldOriginHelper.ARCoreDeviceTransform.rotation, 0);

        _ShowAndroidToastMessage("Anchord");
    }

    /// <summary>
    /// Instantiates a star object that will be synchronized over the network to other clients.
    /// </summary>
    private void _InstantiateStar()
    {
        // Star must be spawned in the server so a networking Command is used.
        //       GameObject.Find("LocalPlayer").GetComponent<LocalPlayerController>()
        //           .CmdSpawnStar(m_LastHitPose.Value.position, m_LastHitPose.Value.rotation);
        PhotonNetwork.Instantiate("AndyGreen", ARCoreWorldOriginHelper.ARCoreDeviceTransform.position, ARCoreWorldOriginHelper.ARCoreDeviceTransform.rotation, 0);
        _ShowAndroidToastMessage("stard");
    }

    /// <summary>
    /// Sets the corresponding platform active.
    /// </summary>
    private void _SetPlatformActive()
    {
        if (Application.platform != RuntimePlatform.IPhonePlayer)
        {
            ARCoreRoot.SetActive(true);
        //    ARKitRoot.SetActive(false);
        }
        else
        {
            ARCoreRoot.SetActive(false);
         //   ARKitRoot.SetActive(true);
        }
    }

    /// <summary>
    /// Indicates whether a star can be placed.
    /// </summary>
    /// <returns><c>true</c>, if stars can be placed, <c>false</c> otherwise.</returns>
    private bool _CanPlaceStars()
    {
        if (m_CurrentMode == ApplicationMode.Resolving)
        {
            return m_IsOriginPlaced;
        }

        if (m_CurrentMode == ApplicationMode.Hosting)
        {
            return m_IsOriginPlaced && m_AnchorFinishedHosting;
        }

        return false;
    }

    /// <summary>
    /// Resets the internal status.
    /// </summary>
    private void _ResetStatus()
    {
        // Reset internal status.
        m_CurrentMode = ApplicationMode.Ready;
        if (m_WorldOriginAnchor != null)
        {
            Destroy(m_WorldOriginAnchor.gameObject);
        }

        m_WorldOriginAnchor = null;
    }





    public void UpdateLifeCycle()
    {

        if (!m_MatchStarted && PhotonNetwork.connected)
        {
            m_MatchStarted = true;
        }
        var sleepTimeout = SleepTimeout.NeverSleep;

        Screen.sleepTimeout = sleepTimeout;

        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            sleepTimeout = lostTrackingSleepTimeout;
        }

        // appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 5.0f);
        }
        else if (Session.Status.IsError())
        {
           _ShowAndroidToastMessage
           (
                "ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 5.0f);
        }
        else if (m_MatchStarted && !PhotonNetwork.connected)
        {
           _ShowAndroidToastMessage(
                "Network session disconnected!  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 5.0f);
        }
        
    }

    void OnPhotonRandomJoinFailed()
    {
        if (m_CurrentMode == ApplicationMode.Resolving)
        {
            m_CurrentMode = ApplicationMode.Ready;
            _ResetStatus();
            return;
        }
        if (m_CurrentMode == ApplicationMode.Hosting)
        {
            m_CurrentMode = ApplicationMode.Ready;
            _ResetStatus();
            return;
        }
        RoomOptions roomoptions = new RoomOptions() { isVisible = true, maxPlayers = 6 };
        int randNum = UnityEngine.Random.Range(0, 100);

        PhotonNetwork.CreateRoom(randNum.ToString(), roomoptions, TypedLobby.Default);
        _ShowAndroidToastMessage("Created new room : " + randNum);
    }



    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity =
            unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject =
                    toastClass.CallStatic<AndroidJavaObject>(
                        "makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
}


