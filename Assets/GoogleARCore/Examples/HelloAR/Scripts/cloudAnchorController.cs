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

    /// <summary>
    /// The current cloud anchor mode.
    /// </summary>
  // private ApplicationMode m_CurrentMode = ApplicationMode.Ready;

    private bool m_MatchStarted = false;
    //.countOfPlayers;
 //   private PhotonNetwork p;

    public void Start()
    {
       // PhotonNetwork.
      // p.
        //  m_NetworkManager = UIController.GetComponent<NetworkManager>();

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
              //  SetWorldOrigin(m_WorldOriginAnchor.transform);
            }
            else
            {
                if (Application.platform != RuntimePlatform.IPhonePlayer)
                {
                    m_WorldOriginAnchor =
                        arcoreHitResult.Trackable.CreateAnchor(arcoreHitResult.Pose);
                }
                else
                {
                 //   m_WorldOriginAnchor = m_ARKit.CreateAnchor(m_LastHitPose.Value);
                }

                SetWorldOrigin(m_WorldOriginAnchor.transform);
                _InstantiateAnchor();
                OnAnchorInstantiated(true);
            }
        }
    }

    private void OnAnchorInstantiated(bool v)
    {
        // throw new NotImplementedException();
        if (v)
            nw = 9;
        else
            nw = 0;
    }

    public void SetWorldOrigin(Transform anchorTransform)
    {
   //     if (m_IsOriginPlaced)
   //     {
   //         Debug.LogWarning("The World Origin can be set only once.");
  //         return;
  //      }

    //    m_IsOriginPlaced = true;

        if (Application.platform != RuntimePlatform.IPhonePlayer)
        {
            ARCoreWorldOriginHelper.SetWorldOrigin(anchorTransform);
        }
        else
        {
         //   m_ARKit.SetWorldOrigin(anchorTransform);
        }
    }
    private void _InstantiateAnchor()
    {
        // The anchor will be spawned by the host, so no networking Command is needed.
 //       GameObject.Find("AndyGreen").GetComponent<LocalPlayerController>()
//           .SpawnAnchor(Vector3.zero, Quaternion.identity, m_WorldOriginAnchor);
    }
    private void _InstantiateStar()
    {
        // Star must be spawned in the server so a networking Command is used.
//        GameObject.Find("AndyGreen").GetComponent<LocalPlayerController>()
//            .CmdSpawnStar(m_LastHitPose.Value.position, m_LastHitPose.Value.rotation);
    }

    private bool _CanPlaceStars()
    {
        if (nw == 0)
            return false;
        else
            return true;

       // return false;
    }

}
