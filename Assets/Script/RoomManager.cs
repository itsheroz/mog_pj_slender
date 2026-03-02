using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public string playerPrefabName = "Player";
    public float spawnRadius = 3f;

    [Header("Spawn Points")]
    [Tooltip("ลาก Empty GameObject ที่เป็น Spawn Point เข้ามาได้เลย")]
    public Transform[] spawnPoints;

    [Tooltip("ระยะกระจายรอบ SpawnPoint (ป้องกัน player ซ้อนกัน)")]
    public float spawnOffset = 2f;

    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPos;
        Quaternion spawnRot = Quaternion.identity;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // สุ่มจาก SpawnPoint ที่กำหนดไว้ + offset เล็กน้อยป้องกันซ้อนทับ
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            spawnPos = point.position + new Vector3(
                Random.Range(-spawnOffset, spawnOffset),
                0f,
                Random.Range(-spawnOffset, spawnOffset)
            );
            spawnRot = point.rotation;
        }
        else
        {
            // fallback: สุ่มรอบ origin (เหมือนเดิม)
            spawnPos = new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                2f,
                Random.Range(-spawnRadius, spawnRadius)
            );
        }

        PhotonNetwork.Instantiate(playerPrefabName, spawnPos, spawnRot);
        Debug.Log("Player spawned at: " + spawnPos);
    }
}
