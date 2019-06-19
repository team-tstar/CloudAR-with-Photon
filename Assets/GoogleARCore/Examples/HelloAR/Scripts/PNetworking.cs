using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PNetworking : MonoBehaviour
{
   // [SerializeField] private GameObject playerCamera;
//   [SerializeField] private MonoBehaviour[] scriptToignore;
    // Start is called before the first frame update
    PhotonView photonView;
    void Start()
    {
        photonView = GetComponent<PhotonView>();
         Initialize();
     //   RuntimeInitializeLoadType;=========
    }

     void Initialize()
    {
       //throw new NotImplementedException();
       if(photonView.isMine)
        {

        }
        else
        {
      //      playerCamera.SetActive(false);
    //       foreach(MonoBehaviour item in scriptToignore)
    //        {
    //            item.enabled=(false); 
   //         }
        }
    }
}
