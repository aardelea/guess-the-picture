using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScreenController : MonoBehaviour
{
    public GameObject aboutPopup;
    public GameObject hintPopup;

    private AudioSource audioSource;
    public AudioClip buttonPressClip;

    public Button muteButton;
    public Button exitButton;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (aboutPopup != null)
        {
            aboutPopup.SetActive(false);
        }
        if (hintPopup != null)
        {
            hintPopup.SetActive(false);
        }
        if (muteButton != null)
        {
            muteButton.onClick.AddListener(ToggleMusicMute);
        }
    }

    void PlayButtonSound()
    {
        if (audioSource != null && buttonPressClip != null)
        {
            audioSource.PlayOneShot(buttonPressClip);
        }
    }

    public void StartGame()
    {
        PlayButtonSound();
        SceneManager.LoadScene("Game");
    }

    public void ShowAboutPopup()
    {
        PlayButtonSound();
        if (aboutPopup != null)
        {
            aboutPopup.SetActive(true);
        }
    }

    public void HideAboutPopup()
    {
        PlayButtonSound();
        if (aboutPopup != null)
        {
            aboutPopup.SetActive(false);
        }
    }

    public void ShowHintPopup()
    {
        PlayButtonSound();
        if (hintPopup != null)
        {
            hintPopup.SetActive(true);
        }
    }

    public void HideHintPopup()
    {
        PlayButtonSound();
        if (hintPopup != null)
        {
            hintPopup.SetActive(false);
        }
    }

    private void ToggleMusicMute()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMute();
        }
        else
        {
            Debug.LogWarning("AudioManager instance not found!");
        }
    }

    // Method to exit the game
    public void ExitGame()
    {
        PlayButtonSound();
        Application.Quit();
        Debug.Log("Exit!");
    }
}
