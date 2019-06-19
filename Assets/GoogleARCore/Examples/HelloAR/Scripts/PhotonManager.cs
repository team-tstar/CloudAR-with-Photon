using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonManager : Photon.MonoBehaviour
{
    // Start is called before the first frame update
   [SerializeField] private GameObject player;
 //   [SerializeField] private GameObject lobbyCamera;
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings("1.0");

    }

  void OnJoinedLobby()
    {
        PhotonNetwork.JoinOrCreateRoom("Room",new RoomOptions() { MaxPlayers = 3 },TypedLobby.Default);

    }

    void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate("Playerrr", player.transform.position, Quaternion.identity, 0);
   //     lobbyCamera.SetActive(false) ;

    }
}