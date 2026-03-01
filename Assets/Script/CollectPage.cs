using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class CollectPage : MonoBehaviourPun
{
    public AudioClip collectSound;

    // เก็บ reference ของ local player ที่อยู่ในระยะเก็บ
    private PhotonView nearbyLocalPlayer;
    private GameObject collectText;
    private GameObject gameLogic;

    void Start()
    {
        gameLogic = GameObject.FindGameObjectWithTag("GameLogic");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Reach")
        {
            // เช็คว่าเป็น local player ของเราหรือเปล่า
            PhotonView pv = other.transform.root.GetComponent<PhotonView>();
            if (pv == null || !pv.IsMine) return; // ไม่ใช่ของเรา → ไม่ทำอะไร

            nearbyLocalPlayer = pv;

            // หา "Collect" text จาก Player ที่เข้ามาใกล้
            Transform playerRoot = other.transform.root;
            Transform found = FindChildByName(playerRoot, "Collect");
            if (found != null)
            {
                collectText = found.gameObject;
                collectText.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Reach")
        {
            // เช็คว่าเป็น local player ที่ออกไป
            PhotonView pv = other.transform.root.GetComponent<PhotonView>();
            if (pv == null || !pv.IsMine) return;

            nearbyLocalPlayer = null;

            if (collectText != null)
            {
                collectText.SetActive(false);
                collectText = null;
            }
        }
    }

    void Update()
    {
        // เฉพาะ local player ที่อยู่ในระยะเท่านั้นที่กดเก็บได้
        if (nearbyLocalPlayer != null && Input.GetButtonDown("collect"))
        {
            if (collectText != null)
            {
                collectText.SetActive(false);
                collectText = null;
            }

            nearbyLocalPlayer = null;

            photonView.RPC("CollectPageRPC", RpcTarget.All);
        }
    }

    /// <summary>
    /// หา child ชื่อที่กำหนดจาก hierarchy ทั้งหมด (รวม inactive)
    /// </summary>
    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
            {
                return child;
            }
        }
        return null;
    }

    [PunRPC]
    private void CollectPageRPC()
    {
        // เฉพาะ MasterClient เท่านั้นที่เพิ่ม pageCount (ป้องกันนับซ้ำ)
        if (PhotonNetwork.IsMasterClient)
        {
            // หา GameLogic ใหม่ถ้ายังไม่มี
            if (gameLogic == null)
            {
                gameLogic = GameObject.FindGameObjectWithTag("GameLogic");
                Debug.Log("[CollectPage] Re-finding GameLogic: " + (gameLogic != null ? "Found!" : "NOT FOUND!"));
            }

            if (gameLogic != null)
            {
                GameLogic gl = gameLogic.GetComponent<GameLogic>();
                if (gl != null)
                {
                    gl.AddPage();
                    Debug.Log("[CollectPage] AddPage called! pageCount = " + gl.pageCount);
                }
                else
                {
                    Debug.LogWarning("[CollectPage] GameLogic component not found on object!");
                }
            }
            else
            {
                Debug.LogWarning("[CollectPage] GameLogic object not found! Make sure Tag is set to 'GameLogic'");
            }
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(collectSound);

        gameObject.SetActive(false);
    }
}
