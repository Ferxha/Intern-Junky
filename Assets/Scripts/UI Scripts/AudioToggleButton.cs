using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioToggleButton : MonoBehaviour
{
    public Sprite audioOnIcon;
    public Sprite audioOffIcon;
    
    private Button button;
    private Image buttonImage;
    private TextMeshProUGUI buttonText;
    private bool isAudioOn = true;
    
    void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        
        if (button != null)
        {
            button.onClick.AddListener(ToggleAudio);
        }
        
        isAudioOn = PlayerPrefs.GetInt("AudioEnabled", 1) == 1;
        UpdateVisuals();
        ApplyAudioState();
    }

    void ToggleAudio()
    {
        isAudioOn = !isAudioOn;
        PlayerPrefs.SetInt("AudioEnabled", isAudioOn ? 1 : 0);
        PlayerPrefs.Save();
        
        UpdateVisuals();
        ApplyAudioState();
    }

    void UpdateVisuals()
    {
        if (buttonImage != null)
        {
            if (isAudioOn && audioOnIcon != null)
                buttonImage.sprite = audioOnIcon;
            else if (!isAudioOn && audioOffIcon != null)
                buttonImage.sprite = audioOffIcon;
        }
        
    }

    void ApplyAudioState()
    {
        AudioListener.volume = isAudioOn ? 1f : 0f;
        Debug.Log($"Audio {(isAudioOn ? "activado" : "desactivado")}");
    }
}
