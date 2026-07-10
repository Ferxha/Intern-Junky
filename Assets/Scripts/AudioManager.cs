using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    //Variables donde asignar los clips de audio desde el inspector de Unity
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip sfxCauldron;
    [SerializeField] private AudioClip sfxTrash;
    [SerializeField] private AudioClip sfxPotionSuccess;
    [SerializeField] private AudioClip sfxPotionFailure;

    void Awake()
    {
        // Configuración clásica del Singleton
        if (Instance == null)
        {
            Instance = this;
            //Hace que el audio no se destruya al cambiar de escena, para música de fondo
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayBackgroundMusic(backgroundMusic);
    }

    void PlayBackgroundMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return; //CAMBIAR CUANDO SE TENGA PISTA DE AUDIO

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    //Creo métodos públicos para reproducir los efectos de sonido desde otros scripts
    public void PlaySFXCauldron()
    {
        if (sfxSource != null && sfxCauldron != null)
        {
            sfxSource.PlayOneShot(sfxCauldron);
        }
    }

    public void PlaySFXTrash()
    {
        if (sfxSource != null && sfxTrash != null)
        {
            sfxSource.PlayOneShot(sfxTrash);
        }
    }

    public void PlaySFXPotionSuccess()
    {
        if (sfxSource != null && sfxPotionSuccess != null)
        {
            sfxSource.PlayOneShot(sfxPotionSuccess);
        }
    }

    public void PlaySFXPotionFailure()
    {
        if (sfxSource != null && sfxPotionFailure != null)
        {
            sfxSource.PlayOneShot(sfxPotionFailure);
        }
    }
}
