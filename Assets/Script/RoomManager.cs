using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public string playerPrefabName = "Player";
    public float spawnRadius = 3f;

    void Start()
    {
        // ถ้าอยู่ในห้องแล้ว (โหลดมาจาก LobbyManager) → spawn ผู้เล่น
        if (PhotonNetwork.InRoom)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPos = new Vector3(
            Random.Range(-spawnRadius, spawnRadius),
            2f,
            Random.Range(-spawnRadius, spawnRadius)
        );

        PhotonNetwork.Instantiate(playerPrefabName, spawnPos, Quaternion.identity);
        Debug.Log("Player spawned at: " + spawnPos);
    }
}
