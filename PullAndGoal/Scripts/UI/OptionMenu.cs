using UnityEngine;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{
    [SerializeField] private GameObject OptionPanel;
    [SerializeField] private Slider musicSlider, sfxSlider;

    [Header("View")]
    [SerializeField] private Image musicImage;
    [SerializeField] private Image sfxImage;

    private void Start()
    {
        InitToggle();

        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);

        sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("SFXVolume", 1f));
        AudioManager.Instance.SetSFXVolume(sfxSlider.value);
    }

    private void InitToggle()
    {
        if (PlayerPrefs.GetInt("MusicMuted", 0) == 1)
        {
            musicImage.color = Color.gray;
            AudioManager.Instance.SetMusicMute(true);
        }

        if (PlayerPrefs.GetInt("SFXMuted", 0) == 1)
        {
            sfxImage.color = Color.gray;
            AudioManager.Instance.SetSFXMute(true);
        }
    }

    public void OpenOptionPanel()
    {
        OptionPanel.SetActive(true);
    }

    public void CloseOptionPanel()
    {
        OptionPanel.SetActive(false); 
    }

    public void ToggleMusic()
    {
        AudioManager.Instance.ToggleMusic();
        PlayerPrefs.SetInt("MusicMuted", AudioManager.Instance.musicSource.mute ? 1 : 0);

        if (AudioManager.Instance.musicSource.mute)
        {
            musicImage.color = Color.gray;
        }
        else
        {
            musicImage.color = Color.white;
        }
    }

    public void ToggleSFX()
    {
        AudioManager.Instance.ToggleSFX();
        PlayerPrefs.SetInt("SFXMuted", AudioManager.Instance.sfxSource.mute ? 1 : 0);

        if (AudioManager.Instance.sfxSource.mute)
        {
            sfxImage.color = Color.gray;
        }
        else 
        {
            sfxImage.color = Color.white;
        }
    }

    public void MusicVolume()
    {
        AudioManager.Instance.SetMusicVolume(musicSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
    }

    public void SFXVolume()
    {
        AudioManager.Instance.SetSFXVolume(sfxSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);

        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxSounds[Random.Range(0, AudioManager.Instance.sfxSounds.Length)].name);
    }
}
