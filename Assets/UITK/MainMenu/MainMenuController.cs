using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Game Data (Drag In)")]
    [Tooltip("Drag the PlayerInventory.asset file here")]
    public PlayerInventorySO playerInventory;

    private AudioManager audioManager;

    private VisualElement sidePanel, buttonGroup, settingsPanel, loadingScreen;
    private Button newGameButton, continueButton, settingsButton, quitButton, backButton;
    private Slider musicVolumeSlider, sfxVolumeSlider;
    private Toggle fullscreenToggle;
    private DropdownField qualityDropdown;
    private Label musicVolumeLabel, sfxVolumeLabel;

    private bool isLoadingScreenVisible = false;

    void OnEnable()
    {
        audioManager = AudioManager.instance;
        audioManager?.PlayMusic("MainTheme");

        var root = GetComponent<UIDocument>().rootVisualElement;

        sidePanel = root.Q("side-panel");
        buttonGroup = root.Q("button-group");
        settingsPanel = root.Q("settings-panel");
        loadingScreen = root.Q("loading-screen");

        newGameButton = root.Q<Button>("new-game-button");
        continueButton = root.Q<Button>("continue-button");
        settingsButton = root.Q<Button>("settings-button");
        quitButton = root.Q<Button>("quit-button");
        backButton = root.Q<Button>("back-button");

        musicVolumeSlider = root.Q<Slider>("music-volume-slider");
        sfxVolumeSlider = root.Q<Slider>("sfx-volume-slider");
        fullscreenToggle = root.Q<Toggle>("fullscreen-toggle");
        qualityDropdown = root.Q<DropdownField>("quality-dropdown");
        musicVolumeLabel = root.Q<Label>("music-volume-label");
        sfxVolumeLabel = root.Q<Label>("sfx-volume-label");

        RegisterButtonCallbacks();
        SetupSettingsControls();

        loadingScreen.style.visibility = Visibility.Visible;
        loadingScreen.AddToClassList("fade-in");
        isLoadingScreenVisible = true;
        StartCoroutine(InitialFadeOut(1.0f));

        ShowMainPanel();
    }

    private void RegisterButtonCallbacks()
    {
        newGameButton?.RegisterCallback<ClickEvent>(evt =>
        {
            audioManager?.PlaySFX("Click");
            StartCoroutine(FadeInAndExit(() =>
            {
                if (playerInventory != null)
                {
                    playerInventory.ResetData();
                    Debug.Log("PlayerInventorySO has been reset!");
                    SceneManager.LoadScene(playerInventory.currentSceneName);
                }
                else
                {
                    Debug.LogError("PlayerInventorySO not assigned in MainMenuController!");
                }
            }));
        });

        continueButton?.RegisterCallback<ClickEvent>(evt =>
        {
            audioManager?.PlaySFX("Click");
            StartCoroutine(FadeInAndExit(() =>
            {
                if (playerInventory != null && !string.IsNullOrEmpty(playerInventory.currentSceneName))
                {
                    SceneManager.LoadScene(playerInventory.currentSceneName);
                }
                else
                {
                    Debug.LogWarning("No current scene found, starting new game...");
                    playerInventory.ResetData();
                    SceneManager.LoadScene(playerInventory.currentSceneName);
                }
            }));
        });

        settingsButton?.RegisterCallback<ClickEvent>(evt =>
        {
            audioManager?.PlaySFX("Click");
            ShowSettingsPanel();
        });

        quitButton?.RegisterCallback<ClickEvent>(evt =>
        {
            audioManager?.PlaySFX("Click");
            StartCoroutine(FadeInAndExit(() =>
            {
                Debug.Log("Quit Game Requested.");
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }));
        });

        backButton?.RegisterCallback<ClickEvent>(evt =>
        {
            audioManager?.PlaySFX("Click");
            ShowMainPanel();
        });
    }

    private void ShowMainPanel()
    {
        buttonGroup?.RemoveFromClassList("hidden");
        settingsPanel?.AddToClassList("hidden");
    }

    private void ShowSettingsPanel()
    {
        buttonGroup?.AddToClassList("hidden");
        settingsPanel?.RemoveFromClassList("hidden");
    }

    public void ToggleMenu() => sidePanel?.ToggleInClassList("open");
    public void ShowMenu() => sidePanel?.AddToClassList("open");
    public void HideMenu() => sidePanel?.RemoveFromClassList("open");

    private IEnumerator InitialFadeOut(float delay)
    {
        yield return new WaitForSeconds(delay);
        loadingScreen.RemoveFromClassList("fade-in");
        isLoadingScreenVisible = false;
        yield return new WaitForSeconds(2.0f);
        loadingScreen.style.visibility = Visibility.Hidden;
        ShowMenu();
    }

    private IEnumerator FadeInAndExit(System.Action exitAction)
    {
        HideMenu();
        loadingScreen.style.visibility = Visibility.Visible;
        loadingScreen.AddToClassList("fade-in");
        isLoadingScreenVisible = true;
        yield return new WaitForSeconds(2.0f);
        exitAction?.Invoke();
    }

    private IEnumerator ToggleLoadingScreen(bool show)
    {
        if (show == isLoadingScreenVisible) yield break;

        isLoadingScreenVisible = show;
        if (show)
        {
            loadingScreen.style.visibility = Visibility.Visible;
            loadingScreen.AddToClassList("fade-in");
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            loadingScreen.RemoveFromClassList("fade-in");
            yield return new WaitForSeconds(2.0f);
            loadingScreen.style.visibility = Visibility.Hidden;
        }
    }

    private void SetupSettingsControls()
    {
        if (audioManager != null)
        {
            musicVolumeSlider.value = audioManager.GetMusicVolume();
            musicVolumeLabel.text = Mathf.RoundToInt(musicVolumeSlider.value * 100).ToString();
            musicVolumeSlider.RegisterValueChangedCallback(evt => {
                audioManager.SetMusicVolume(evt.newValue);
                musicVolumeLabel.text = Mathf.RoundToInt(evt.newValue * 100).ToString();
            });

            sfxVolumeSlider.value = audioManager.GetSFXVolume();
            sfxVolumeLabel.text = Mathf.RoundToInt(sfxVolumeSlider.value * 100).ToString();
            sfxVolumeSlider.RegisterValueChangedCallback(evt => {
                audioManager.SetSFXVolume(evt.newValue);
                sfxVolumeLabel.text = Mathf.RoundToInt(evt.newValue * 100).ToString();
            });
        }
        else
        {
            musicVolumeSlider.value = 1f;
            musicVolumeLabel.text = "100";
            musicVolumeSlider.RegisterValueChangedCallback(evt => {
                musicVolumeLabel.text = Mathf.RoundToInt(evt.newValue * 100).ToString();
            });

            sfxVolumeSlider.value = 1f;
            sfxVolumeLabel.text = "100";
            sfxVolumeSlider.RegisterValueChangedCallback(evt => {
                sfxVolumeLabel.text = Mathf.RoundToInt(evt.newValue * 100).ToString();
            });
        }

        fullscreenToggle.value = Screen.fullScreen;
        fullscreenToggle.RegisterValueChangedCallback(evt => Screen.fullScreen = evt.newValue);

        qualityDropdown.choices = new List<string>(QualitySettings.names);
        qualityDropdown.index = QualitySettings.GetQualityLevel();
        qualityDropdown.RegisterValueChangedCallback(evt => {
            QualitySettings.SetQualityLevel(qualityDropdown.choices.IndexOf(evt.newValue));
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (sidePanel != null && !sidePanel.ClassListContains("open")) { ShowMenu(); return; }
            if (settingsPanel != null && !settingsPanel.ClassListContains("hidden")) { ShowMainPanel(); }
            else { HideMenu(); }
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            StartCoroutine(ToggleLoadingScreen(!isLoadingScreenVisible));
        }
    }
}