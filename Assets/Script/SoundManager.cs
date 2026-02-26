using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource; // สำหรับเพลง BGM
    [SerializeField] private AudioSource sfxSource;   // สำหรับ Sound Effect (2D)

    private void Awake()
    {
        // Singleton Pattern: ให้มี SoundManager ได้แค่ตัวเดียวในเกม
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ไม่ให้ทำลายเมื่อเปลี่ยนฉาก
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ฟังก์ชันเล่น Sound Effect แบบ 2D (เช่น เสียง UI, เสียงเก็บของ)
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    // ฟังก์ชันเล่น Sound Effect แบบ 3D ณ ตำแหน่งที่กำหนด (เช่น เสียงระเบิด, เสียงเดิน)
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }

    // ฟังก์ชันเล่นเพลง Background Music
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (musicSource != null && musicSource.clip != clip)
        {
            musicSource.clip = clip;
            musicSource.volume = volume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // ฟังก์ชันหยุดเพลง
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
}
