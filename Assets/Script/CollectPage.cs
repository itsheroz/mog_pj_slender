using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class CollectPage : MonoBehaviourPun
{
    public AudioClip collectSound;
    private bool inReach;
    private GameObject gameLogic;
    private GameObject collectText; // อ้างอิง text "Collect" ของผู้เล่นที่อยู่ใกล้

    void Start()
    {
        inReach = false;
        gameLogic = GameObject.FindGameObjectWithTag("GameLogic");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Reach")
        {
            inReach = true;

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
            inReach = false;

            if (collectText != null)
            {
                collectText.SetActive(false);
                collectText = null;
            }
        }
    }

    void Update()
    {
        if (inReach && Input.GetButtonDown("collect"))
        {
            if (collectText != null)
            {
                collectText.SetActive(false);
                collectText = null;
            }

            photonView.RPC("CollectPageRPC", RpcTarget.All);
        }
    }

    /// <summary>
    /// หา child ชื่อที่กำหนดจาก hierarchy ทั้งหมด (รวม inactive)
    /// </summary>
    private Transform FindChildByName(Transform parent, string name)
    {
        // GetComponentsInChildren(true) หาได้แม้ object จะ inactive อยู่
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
        if (gameLogic != null)
        {
            GameLogic gl = gameLogic.GetComponent<GameLogic>();
            if (gl != null)
            {
                gl.AddPage();
            }
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(collectSound);

        gameObject.SetActive(false);
        inReach = false;
    }
}
