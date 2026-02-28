using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Panels")]
    public GameObject mainPanel;         // หน้าแรก: ปุ่ม Play
    public GameObject roomPanel;         // หน้าเลือก: Create / Join / Random
    public GameObject createRoomPanel;   // หน้าสร้างห้อง
    public GameObject joinRoomPanel;     // หน้าแสดงรายชื่อห้อง
    public GameObject loadingPanel;      // แสดงตอนกำลังเชื่อมต่อ/โหลด

    [Header("Create Room")]
    public TMP_InputField roomNameInput; // ช่องใส่ชื่อห้อง
    public TMP_InputField maxPlayersInput; // ช่องใส่จำนวนคนสูงสุด

    [Header("Join Room")]
    public Transform roomListContent;    // Parent ของ room list (Content ใน ScrollView)
    public GameObject roomItemPrefab;    // Prefab สำหรับแต่ละห้องใน list

    [Header("Settings")]
    public string gameSceneName = "SampleScene"; // ชื่อ Scene เกม

    [Header("Status")]
    public TextMeshProUGUI statusText;   // แสดงสถานะ (เชื่อมต่อ, error, ฯลฯ)

    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();

    // ===========================
    // Connection
    // ===========================

    void Start()
    {
        // แสดง loading ตอนเริ่ม
        ShowPanel(loadingPanel);
        SetStatus("กำลังเชื่อมต่อ...");

        // เชื่อมต่อ Photon
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        SetStatus("เชื่อมต่อสำเร็จ!");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        SetStatus("พร้อมเล่น!");
        ShowPanel(mainPanel); // แสดงหน้าแรก
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SetStatus("ขาดการเชื่อมต่อ: " + cause.ToString());
        ShowPanel(mainPanel);
    }

    // ===========================
    // Main Panel
    // ===========================

    /// <summary>
    /// กดปุ่ม Play → แสดงเมนูห้อง
    /// </summary>
    public void OnPlayButton()
    {
        ShowPanel(roomPanel);
    }

    // ===========================
    // Room Panel (Create / Join / Random)
    // ===========================

    /// <summary>
    /// กดปุ่ม Create Room → แสดงหน้าสร้างห้อง
    /// </summary>
    public void OnCreateRoomButton()
    {
        ShowPanel(createRoomPanel);
    }

    /// <summary>
    /// กดปุ่ม Join Room → แสดงรายชื่อห้อง
    /// </summary>
    public void OnJoinRoomButton()
    {
        ShowPanel(joinRoomPanel);
        UpdateRoomListUI();
    }

    /// <summary>
    /// กดปุ่ม Random Room → สุ่มเข้าห้อง
    /// </summary>
    public void OnRandomRoomButton()
    {
        ShowPanel(loadingPanel);
        SetStatus("กำลังหาห้อง...");
        PhotonNetwork.JoinRandomRoom();
    }

    /// <summary>
    /// กดปุ่ม Back → กลับหน้าแรก
    /// </summary>
    public void OnBackToMainButton()
    {
        ShowPanel(mainPanel);
    }

    /// <summary>
    /// กดปุ่ม Back → กลับหน้าเลือกห้อง
    /// </summary>
    public void OnBackToRoomPanelButton()
    {
        ShowPanel(roomPanel);
    }

    // ===========================
    // Create Room
    // ===========================

    /// <summary>
    /// กดปุ่ม Confirm Create → สร้างห้อง
    /// </summary>
    public void OnConfirmCreateRoom()
    {
        string roomName = roomNameInput != null ? roomNameInput.text : "";
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room_" + Random.Range(1000, 9999);
        }

        byte maxPlayers = 4;
        if (maxPlayersInput != null && !string.IsNullOrEmpty(maxPlayersInput.text))
        {
            byte.TryParse(maxPlayersInput.text, out maxPlayers);
            maxPlayers = (byte)Mathf.Clamp(maxPlayers, 1, 8);
        }

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = maxPlayers;

        ShowPanel(loadingPanel);
        SetStatus("กำลังสร้างห้อง...");

        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
    }

    // ===========================
    // Join Room (Room List)
    // ===========================

    // Photon เรียกเมื่อรายชื่อห้องเปลี่ยน
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // อัพเดท cached list
        foreach (RoomInfo info in roomList)
        {
            // ลบห้องที่ปิดหรือเต็มออกจาก cache
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info);
            }
            else
            {
                // อัพเดทหรือเพิ่มห้องใหม่
                int index = cachedRoomList.FindIndex(r => r.Name == info.Name);
                if (index >= 0)
                {
                    cachedRoomList[index] = info;
                }
                else
                {
                    cachedRoomList.Add(info);
                }
            }
        }

        // อัพเดท UI ถ้ากำลังดูหน้า Join Room
        if (joinRoomPanel != null && joinRoomPanel.activeSelf)
        {
            UpdateRoomListUI();
        }
    }

    private void UpdateRoomListUI()
    {
        if (roomListContent == null || roomItemPrefab == null) return;

        // ลบ item เก่าทั้งหมด
        foreach (Transform child in roomListContent)
        {
            Destroy(child.gameObject);
        }

        // สร้าง item ใหม่ตาม cached list
        foreach (RoomInfo info in cachedRoomList)
        {
            if (info.RemovedFromList || !info.IsOpen || !info.IsVisible) continue;

            GameObject item = Instantiate(roomItemPrefab, roomListContent);
            TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = info.Name + "  (" + info.PlayerCount + "/" + info.MaxPlayers + ")";
            }

            // กดปุ่มเข้าห้อง
            Button btn = item.GetComponent<Button>();
            if (btn != null)
            {
                string roomName = info.Name; // ต้อง copy มาก่อนเพราะ closure
                btn.onClick.AddListener(() =>
                {
                    JoinSelectedRoom(roomName);
                });
            }
        }
    }

    private void JoinSelectedRoom(string roomName)
    {
        ShowPanel(loadingPanel);
        SetStatus("กำลังเข้าห้อง...");
        PhotonNetwork.JoinRoom(roomName);
    }

    // ===========================
    // Photon Callbacks
    // ===========================

    public override void OnJoinedRoom()
    {
        SetStatus("เข้าห้องสำเร็จ! กำลังโหลดเกม...");
        PhotonNetwork.LoadLevel(gameSceneName);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetStatus("สร้างห้องไม่สำเร็จ: " + message);
        ShowPanel(createRoomPanel);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SetStatus("เข้าห้องไม่สำเร็จ: " + message);
        ShowPanel(roomPanel);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // ไม่มีห้องว่าง → สร้างห้องใหม่อัตโนมัติ
        SetStatus("ไม่พบห้องว่าง กำลังสร้างห้องใหม่...");
        string roomName = "Room_" + Random.Range(1000, 9999);
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4;
        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
    }

    // ===========================
    // Utility
    // ===========================

    private void ShowPanel(GameObject panel)
    {
        // ซ่อนทุก panel ก่อน
        if (mainPanel != null) mainPanel.SetActive(false);
        if (roomPanel != null) roomPanel.SetActive(false);
        if (createRoomPanel != null) createRoomPanel.SetActive(false);
        if (joinRoomPanel != null) joinRoomPanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);

        // แสดง panel ที่ต้องการ
        if (panel != null) panel.SetActive(true);
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log("[Lobby] " + message);
    }
}
