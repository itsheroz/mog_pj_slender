using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public string playerPrefabName = "Player";
    public byte maxPlayers = 4;
    public float spawnRadius = 3f; // รัศมีสุ่มตำแหน่ง spawn

    void Start()
    {
        Debug.Log("RoomManager Start");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to Server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayers;

        PhotonNetwork.JoinOrCreateRoom("test", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined Room");

        // สุ่มตำแหน่ง spawn เล็กน้อยเพื่อไม่ให้ผู้เล่นซ้อนกัน
        Vector3 spawnPos = new Vector3(
            Random.Range(-spawnRadius, spawnRadius),
            2f,
            Random.Range(-spawnRadius, spawnRadius)
        );

        PhotonNetwork.Instantiate(playerPrefabName, spawnPos, Quaternion.identity);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.LogError("Join Room Failed: " + message);
    }
}
