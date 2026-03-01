using UnityEngine;
using System.Collections;

public class BGMPlayer : MonoBehaviour
{
    [Header("BGM Settings")]
    [SerializeField] private AudioClip bgmClip; // ลากไฟล์เสียง BGM มาใส่ตรงนี้ใน Inspector
    [SerializeField][Range(0f, 1f)] private float volume = 0.5f; // ปรับความดังได้ 0-1
    [SerializeField] private bool playOnStart = true; // เล่นอัตโนมัติตอนเริ่ม Scene

    private AudioListener tempListener; // AudioListener ชั่วคราว

    private void Awake()
    {
        // ถ้าใน Scene ยังไม่มี AudioListener (เพราะ Player ยังไม่ Spawn)
        // ให้เพิ่มตัวชั่วคราวไว้ก่อน
        if (FindFirstObjectByType<AudioListener>() == null)
        {
            tempListener = gameObject.AddComponent<AudioListener>();
        }
    }

    private void Start()
    {
        if (playOnStart && bgmClip != null)
        {
            SoundManager.Instance.PlayMusic(bgmClip, volume);
        }

        // เริ่ม Coroutine เช็ค AudioListener ซ้ำทุก 1 วินาที (แทน Update ทุก frame)
        if (tempListener != null)
        {
            StartCoroutine(CheckForDuplicateListener());
        }
    }

    /// <summary>
    /// เช็คทุก 1 วินาทีว่ามี AudioListener มากกว่า 1 ตัวไหม
    /// ถ้ามี (Player Spawn เข้ามาแล้ว) → ลบตัวชั่วคราวออก
    /// </summary>
    private IEnumerator CheckForDuplicateListener()
    {
        while (tempListener != null)
        {
            yield return new WaitForSeconds(1f);

            AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            if (listeners.Length > 1)
            {
                Destroy(tempListener);
                tempListener = null;
            }
        }
    }

    // เรียกจาก Script อื่น หรือจาก Event ได้
    public void PlayBGM()
    {
        if (bgmClip != null)
        {
            SoundManager.Instance.PlayMusic(bgmClip, volume);
        }
    }

    public void StopBGM()
    {
        SoundManager.Instance.StopMusic();
    }
}
