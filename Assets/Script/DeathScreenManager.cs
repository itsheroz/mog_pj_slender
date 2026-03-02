using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class DeathScreenManager : MonoBehaviour
{
    public static DeathScreenManager Instance;

    [Header("UI")]
    public GameObject deathPanel;  // ลาก Death Panel เข้ามา
    public GameObject completePanel; // ลาก Complete Panel (ชนะรวบรวม 8 หน้า) เข้ามา

    [Header("Settings")]
    public string lobbySceneName = "LobbyScene";  // ชื่อ Lobby Scene

    void Awake()
    {
        Instance = this;

        // ซ่อน panel ตอนเริ่ม
        if (deathPanel != null) deathPanel.SetActive(false);
        if (completePanel != null) completePanel.SetActive(false);
    }

    /// <summary>
    /// เรียกตอน player ตาย → แสดง death screen
    /// </summary>
    public void ShowDeathScreen()
    {
        if (deathPanel != null)
            deathPanel.SetActive(true);

        // ปลดล็อคเคอร์เซอร์
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// เรียกตอนเก็บครบ 8 หน้า → แสดง win screen
    /// </summary>
    public void ShowCompleteScreen()
    {
        if (completePanel != null)
            completePanel.SetActive(true);

        // ปลดล็อคเคอร์เซอร์
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// ปุ่ม Main Menu → ออกจากห้อง → กลับ Lobby
    /// </summary>
    public void OnMainMenuButton()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(lobbySceneName);
    }

    /// <summary>
    /// ปุ่ม Quit → ปิดเกม
    /// </summary>
    public void OnQuitButton()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
