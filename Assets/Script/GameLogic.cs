using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

public class GameLogic : MonoBehaviourPunCallbacks
{
    public int pageCount;

    private const string PAGE_COUNT_KEY = "PageCount";

    void Start()
    {
        pageCount = 0;

        // โหลด pageCount จาก Room Properties (กรณี late-join)
        if (PhotonNetwork.InRoom)
        {
            object value;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(PAGE_COUNT_KEY, out value))
            {
                pageCount = (int)value;
            }
        }
    }

    // ลบ Update() ออก — ไม่ต้องอัพเดท UI แล้ว (PlayerController ทำแทน)

    public void AddPage()
    {
        pageCount++;

        if (PhotonNetwork.InRoom)
        {
            Hashtable props = new Hashtable();
            props[PAGE_COUNT_KEY] = pageCount;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        CheckWinCondition();
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(PAGE_COUNT_KEY))
        {
            pageCount = (int)propertiesThatChanged[PAGE_COUNT_KEY];
            CheckWinCondition();
        }
    }

    private void CheckWinCondition()
    {
        if (pageCount >= 8)
        {
            Debug.Log("All 8 pages collected! You Win!");

            // หยุดเกม เลิกขยับ
            Time.timeScale = 0f;

            if (DeathScreenManager.Instance != null)
            {
                DeathScreenManager.Instance.ShowCompleteScreen();
            }
        }
    }
}
