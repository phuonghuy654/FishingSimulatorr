using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using Cinemachine;

public class InGameUIManager : MonoBehaviour
{
    public enum InGameState
    {
        Idle,
        Paused,
        PanelOpen
    }
    private InGameState currentState;
    private InGameState previousStateBeforePause;

    [Header("UI Templates (Drag In)")]
    public VisualTreeAsset fishListItemTemplate;
    public VisualTreeAsset shopListItemTemplate;
    public VisualTreeAsset travelCardTemplate;

    [Header("Game Data (Drag In)")]
    public PlayerInventorySO playerInventory;

    [Tooltip("Drag the RodDatabaseSO asset file here")]
    public RodDatabaseSO shopRodDatabase;

    [Tooltip("Drag the MapDatabaseSO asset file here")]
    public MapDatabaseSO mapDatabase;

    [Tooltip("Drag the Camera object that has the CinemachineBrain component (usually Main Camera)")]
    public CinemachineBrain cinemachineBrain;

    private AudioManager audioManager;

    private VisualElement root, sellPanel, shopPanel, loadingScreen;
    private ScrollView fishListScrollView, shopItemListScrollView;
    private Button closeSellButton, closeShopButton, sellAllButton;
    private Button sellTabFromSell, shopTabFromSell, sellTabFromShop, shopTabFromShop;
    private Label moneyLabel, rodLabel, totalValueLabel, shopMoneyLabel;
    private Label musicVolumeLabel, sfxVolumeLabel;
    private Button pauseButton;
    private VisualElement dimOverlay, pauseMenu;
    private Button resumeButton, inventoryButtonPause, settingsButtonPause, howToPlayButtonPause, mainMenuButtonPause, quitButtonPause;
    private VisualElement settingsPanelIngame;
    private Slider musicVolumeSlider, sfxVolumeSlider;
    private Toggle fullscreenToggle;
    private DropdownField qualityDropdown;
    private DropdownField languageDropdown;
    private Button configureControlsButton, backButtonSettings;
    private VisualElement controlsPanelIngame;
    private Button backButtonControls;
    private VisualElement howToPlayPanel;
    private Button htpTabCasting, htpTabReeling, htpTabSelling, backButtonHtp;
    private VisualElement htpContentCasting, htpContentReeling, htpContentSelling;
    private VisualElement inventoryPanel;
    private Label invMoneyLabel, invRodLabel;
    private Button invTabFish, invTabRods, invTabItems, closeInventoryButton;
    private VisualElement invContentFish, invContentRods, invContentItems;
    private ScrollView inventoryFishList, inventoryRodList, inventoryItemList;

    private VisualElement travelPanel;
    private ScrollView travelScrollView;
    private Button closeTravelButton;
    private VisualElement travelCardListContainer;

    private bool isLoadingScreenVisible = false;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        audioManager = AudioManager.instance;
        QueryUIElements();
        RegisterCallbacks();
        SetupSettingsControls();
        InitializeUIState();

        if (cinemachineBrain == null)
        {
            Debug.LogWarning("InGameUIManager: CinemachineBrain not assigned. Attempting to find on Main Camera.");
            if (Camera.main != null)
            {
                cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
            }
        }

        if (cinemachineBrain == null)
        {
            Debug.LogError("InGameUIManager: CinemachineBrain reference is missing and could not be found on Main Camera!");
        }
    }

    void Update()
    {
        HandleGlobalInput();
    }

    private void QueryUIElements()
    {
        sellPanel = root.Q("sell-panel");
        shopPanel = root.Q("shop-panel");
        loadingScreen = root.Q("loading-screen");

        fishListScrollView = root.Q<ScrollView>("fish-list");
        shopItemListScrollView = root.Q<ScrollView>("shop-item-list");
        closeSellButton = root.Q<Button>("close-sell-button");
        closeShopButton = root.Q<Button>("close-shop-button");
        sellAllButton = root.Q<Button>("sell-all-button");
        moneyLabel = root.Q<Label>("money-label");
        rodLabel = root.Q<Label>("rod-label");
        totalValueLabel = root.Q<Label>("total-value-label");
        shopMoneyLabel = root.Q<Label>("shop-money-label");
        sellTabFromSell = root.Q<Button>("sell-tab-button--from-sell");
        shopTabFromSell = root.Q<Button>("shop-tab-button--from-sell");
        sellTabFromShop = root.Q<Button>("sell-tab-button--from-shop");
        shopTabFromShop = root.Q<Button>("shop-tab-button--from-shop");
        pauseButton = root.Q<Button>("pause-button");
        dimOverlay = root.Q("dim-overlay");
        pauseMenu = root.Q("pause-menu");
        resumeButton = root.Q<Button>("resume-button");
        inventoryButtonPause = root.Q<Button>("inventory-button");
        settingsButtonPause = root.Q<Button>("settings-button-pause");
        howToPlayButtonPause = root.Q<Button>("how-to-play-button");
        mainMenuButtonPause = root.Q<Button>("main-menu-button");
        quitButtonPause = root.Q<Button>("quit-button-pause");

        settingsPanelIngame = root.Q("settings-panel-ingame");
        musicVolumeSlider = root.Q<Slider>("music-volume-slider");
        musicVolumeLabel = root.Q<Label>("music-volume-label");
        sfxVolumeSlider = root.Q<Slider>("sfx-volume-slider");
        sfxVolumeLabel = root.Q<Label>("sfx-volume-label");

        fullscreenToggle = root.Q<Toggle>("fullscreen-toggle-ingame");
        qualityDropdown = root.Q<DropdownField>("quality-dropdown-ingame");
        languageDropdown = root.Q<DropdownField>("language-dropdown");
        configureControlsButton = root.Q<Button>("configure-controls-button");
        backButtonSettings = root.Q<Button>("back-button-settings");
        controlsPanelIngame = root.Q("controls-panel-ingame");
        backButtonControls = root.Q<Button>("back-button-controls");
        howToPlayPanel = root.Q("how-to-play-panel");
        htpTabCasting = root.Q<Button>("htp-tab-casting");
        htpTabReeling = root.Q<Button>("htp-tab-reeling");
        htpTabSelling = root.Q<Button>("htp-tab-selling");
        backButtonHtp = root.Q<Button>("back-button-htp");
        htpContentCasting = root.Q("htp-content-casting");
        htpContentReeling = root.Q("htp-content-reeling");
        htpContentSelling = root.Q("htp-content-selling");
        inventoryPanel = root.Q("inventory-panel");
        invMoneyLabel = root.Q<Label>("inv-money-label");
        invRodLabel = root.Q<Label>("inv-rod-label");
        invTabFish = root.Q<Button>("inv-tab-fish");
        invTabRods = root.Q<Button>("inv-tab-rods");
        invTabItems = root.Q<Button>("inv-tab-items");
        closeInventoryButton = root.Q<Button>("close-inventory-button");
        invContentFish = root.Q("inv-content-fish");
        invContentRods = root.Q("inv-content-rods");
        invContentItems = root.Q("inv-content-items");
        inventoryFishList = root.Q<ScrollView>("inventory-fish-list");
        inventoryRodList = root.Q<ScrollView>("inventory-rod-list");
        inventoryItemList = root.Q<ScrollView>("inventory-item-list");

        travelPanel = root.Q("travel-panel");
        travelScrollView = root.Q<ScrollView>("travel-scroll-view");
        closeTravelButton = root.Q<Button>("close-travel-button");
        travelCardListContainer = root.Q<VisualElement>("travel-card-list");
    }

    private void RegisterCallbacks()
    {
        closeSellButton.clicked += () => { audioManager?.PlaySFX("Click"); HideAllPanels(); };
        closeShopButton.clicked += () => { audioManager?.PlaySFX("Click"); HideAllPanels(); };
        sellAllButton.clicked += SellAllFish;
        shopTabFromSell.clicked += () => { audioManager?.PlaySFX("Click"); SwitchToShopPanel(); };
        sellTabFromShop.clicked += () => { audioManager?.PlaySFX("Click"); SwitchToSellPanel(); };
        pauseButton.clicked += () => { audioManager?.PlaySFX("Click"); TogglePause(); };
        resumeButton.clicked += () => { audioManager?.PlaySFX("Click"); ResumeGame(); };
        inventoryButtonPause.clicked += () => { audioManager?.PlaySFX("Click"); ShowInventoryPanel(); };
        settingsButtonPause.clicked += () => { audioManager?.PlaySFX("Click"); ShowSettingsPanel(); };
        howToPlayButtonPause.clicked += () => { audioManager?.PlaySFX("Click"); ShowHowToPlayPanel(); };

        mainMenuButtonPause.clicked += () =>
        {
            audioManager?.PlaySFX("Click");
            StartCoroutine(FadeInAndExit(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            }));
        };

        quitButtonPause.clicked += () =>
        {
            audioManager?.PlaySFX("Click");
            StartCoroutine(FadeInAndExit(() =>
            {
                Debug.Log("Quitting Game...");
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }));
        };

        backButtonSettings.clicked += () => { audioManager?.PlaySFX("Click"); HideSettingsPanel(); };
        configureControlsButton.clicked += () => { audioManager?.PlaySFX("Click"); ShowControlsPanel(); };
        backButtonControls.clicked += () => { audioManager?.PlaySFX("Click"); HideControlsPanel(); };
        backButtonHtp.clicked += () => { audioManager?.PlaySFX("Click"); HideHowToPlayPanel(); };
        htpTabCasting.clicked += () => { audioManager?.PlaySFX("Click"); SwitchHtpTab("casting"); };
        htpTabReeling.clicked += () => { audioManager?.PlaySFX("Click"); SwitchHtpTab("reeling"); };
        htpTabSelling.clicked += () => { audioManager?.PlaySFX("Click"); SwitchHtpTab("selling"); };
        closeInventoryButton.clicked += () => { audioManager?.PlaySFX("Click"); HideInventoryPanel(); };
        invTabFish.clicked += () => { audioManager?.PlaySFX("Click"); SwitchInventoryTab("fish"); };
        invTabRods.clicked += () => { audioManager?.PlaySFX("Click"); SwitchInventoryTab("rods"); };
        invTabItems.clicked += () => { audioManager?.PlaySFX("Click"); SwitchInventoryTab("items"); };
        closeTravelButton.clicked += () => { audioManager?.PlaySFX("Click"); HideTravelPanel(); };
    }

    private void SetupSettingsControls()
    {
        if (audioManager != null)
        {
            float initialMusicVol = audioManager.GetMusicVolume();
            musicVolumeSlider.value = initialMusicVol;
            musicVolumeLabel.text = (initialMusicVol * 100f).ToString("F0");

            musicVolumeSlider.RegisterValueChangedCallback(evt => {
                audioManager.SetMusicVolume(evt.newValue);
                musicVolumeLabel.text = (evt.newValue * 100f).ToString("F0");
            });

            float initialSfxVol = audioManager.GetSFXVolume();
            sfxVolumeSlider.value = initialSfxVol;
            sfxVolumeLabel.text = (initialSfxVol * 100f).ToString("F0");

            sfxVolumeSlider.RegisterValueChangedCallback(evt => {
                audioManager.SetSFXVolume(evt.newValue);
                sfxVolumeLabel.text = (evt.newValue * 100f).ToString("F0");
            });
        }

        fullscreenToggle.value = Screen.fullScreen;
        fullscreenToggle.RegisterValueChangedCallback(evt => Screen.fullScreen = evt.newValue);

        qualityDropdown.choices = new List<string>(QualitySettings.names);
        qualityDropdown.index = QualitySettings.GetQualityLevel();
        qualityDropdown.RegisterValueChangedCallback(evt => {
            QualitySettings.SetQualityLevel(qualityDropdown.choices.IndexOf(evt.newValue));
        });

        if (languageDropdown != null)
        {
            languageDropdown.choices = new List<string> { "English", "Vietnamese", "Japanese" };
            languageDropdown.index = 0;
            languageDropdown.RegisterValueChangedCallback(evt => { });
        }
    }

    private void InitializeUIState()
    {
        UpdatePlayerHUD();
        HideAllPanels(false);

        loadingScreen.style.visibility = Visibility.Visible;
        loadingScreen.AddToClassList("fade-in");
        isLoadingScreenVisible = true;
        StartCoroutine(InitialFadeOut(1.0f));

        SwitchState(InGameState.Idle);

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    private void UpdatePlayerHUD()
    {
        if (playerInventory == null) return;

        moneyLabel.text = playerInventory.money.ToString("N0");
        rodLabel.text = playerInventory.currentRod != null ? playerInventory.currentRod.rodName : "None";

        invMoneyLabel.text = $"Money: {playerInventory.money.ToString("N0")}";
        invRodLabel.text = $"Current Rod: {(playerInventory.currentRod != null ? playerInventory.currentRod.rodName : "None")}";
        shopMoneyLabel.text = $"Your Money: {playerInventory.money.ToString("N0")}";
    }

    private void HandleGlobalInput()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (currentState == InGameState.Idle)
            {
                TogglePanels();
            }
            else if (currentState == InGameState.PanelOpen)
            {
                if (sellPanel.ClassListContains("is-active") || shopPanel.ClassListContains("is-active"))
                {
                    HideAllPanels();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (currentState == InGameState.Idle)
            {
                ShowTravelPanel();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == InGameState.Paused)
            {
                if (controlsPanelIngame.ClassListContains("is-active")) HideControlsPanel();
                else if (settingsPanelIngame.ClassListContains("is-active")) HideSettingsPanel();
                else if (inventoryPanel.ClassListContains("is-active")) HideInventoryPanel();
                else if (howToPlayPanel.ClassListContains("is-active")) HideHowToPlayPanel();
                else ResumeGame();
            }
            else if (currentState == InGameState.PanelOpen)
            {
                HideAllPanels();
            }
            else if (currentState == InGameState.Idle)
            {
                PauseGame();
            }
        }
    }

    private void SwitchState(InGameState newState)
    {
        if (currentState == newState) return;
        OnStateExit(currentState);
        currentState = newState;
        OnStateEnter(newState);
    }

    private void OnStateEnter(InGameState state)
    {
        switch (state)
        {
            case InGameState.Idle:
                pauseButton.SetEnabled(true);
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;
                break;
            case InGameState.Paused:
                dimOverlay.AddToClassList("is-active");
                pauseMenu.AddToClassList("is-active");
                pauseButton.text = "▶";
                Time.timeScale = 0f;
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
                break;
            case InGameState.PanelOpen:
                pauseButton.SetEnabled(false);
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
                if (cinemachineBrain != null)
                {
                    cinemachineBrain.enabled = false;
                }
                break;
        }
    }

    private void OnStateExit(InGameState state)
    {
        switch (state)
        {
            case InGameState.Paused:
                dimOverlay.RemoveFromClassList("is-active");
                pauseMenu.RemoveFromClassList("is-active");
                inventoryPanel.RemoveFromClassList("is-active");
                settingsPanelIngame.RemoveFromClassList("is-active");
                controlsPanelIngame.RemoveFromClassList("is-active");
                howToPlayPanel.RemoveFromClassList("is-active");
                pauseButton.text = "❚❚";
                Time.timeScale = 1f;
                if (previousStateBeforePause == InGameState.Idle)
                {
                    UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                    UnityEngine.Cursor.visible = false;
                }
                break;
            case InGameState.PanelOpen:
                pauseButton.SetEnabled(true);
                if (cinemachineBrain != null)
                {
                    cinemachineBrain.enabled = true;
                }
                if (currentState == InGameState.Idle)
                {
                    UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                    UnityEngine.Cursor.visible = false;
                }
                break;
        }
    }

    private void TogglePause()
    {
        if (currentState == InGameState.Paused)
        {
            ResumeGame();
        }
        else if (currentState == InGameState.Idle)
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        if (currentState == InGameState.Paused) return;
        previousStateBeforePause = currentState;
        SwitchState(InGameState.Paused);
    }

    private void ResumeGame()
    {
        if (currentState != InGameState.Paused) return;
        SwitchState(previousStateBeforePause);
    }

    private void ShowPanel(VisualElement panel)
    {
        HideAllPanels(false);
        dimOverlay.AddToClassList("is-active");
        panel.AddToClassList("is-active");
        UpdatePlayerHUD();
        SwitchState(InGameState.PanelOpen);
    }

    public void HideAllPanels(bool switchToIdle = true)
    {
        sellPanel.RemoveFromClassList("is-active");
        shopPanel.RemoveFromClassList("is-active");
        inventoryPanel.RemoveFromClassList("is-active");
        settingsPanelIngame.RemoveFromClassList("is-active");
        controlsPanelIngame.RemoveFromClassList("is-active");
        howToPlayPanel.RemoveFromClassList("is-active");
        travelPanel.RemoveFromClassList("is-active");

        dimOverlay.RemoveFromClassList("is-active");
        pauseMenu.RemoveFromClassList("is-active");

        if (switchToIdle && (currentState == InGameState.PanelOpen || currentState == InGameState.Paused))
        {
            SwitchState(InGameState.Idle);
        }
    }

    public void TogglePanels()
    {
        if (currentState != InGameState.Idle) return;
        PopulateFishList();
        ShowPanel(sellPanel);
        shopPanel.AddToClassList("is-above");
    }

    private void SwitchToShopPanel()
    {
        PopulateShopList();
        sellPanel.RemoveFromClassList("is-active");
        shopPanel.AddToClassList("is-active");
        shopPanel.RemoveFromClassList("is-above");
    }

    private void SwitchToSellPanel()
    {
        PopulateFishList();
        shopPanel.RemoveFromClassList("is-active");
        shopPanel.AddToClassList("is-above");
        sellPanel.AddToClassList("is-active");
    }

    private void ShowInventoryPanel()
    {
        if (currentState != InGameState.Paused) return;
        pauseMenu.RemoveFromClassList("is-active");
        UpdatePlayerHUD();
        SwitchInventoryTab("fish");
        inventoryPanel.AddToClassList("is-active");
    }

    private void HideInventoryPanel()
    {
        inventoryPanel.RemoveFromClassList("is-active");
        pauseMenu.AddToClassList("is-active");
    }

    private void ShowSettingsPanel()
    {
        if (currentState != InGameState.Paused) return;
        pauseMenu.RemoveFromClassList("is-active");
        settingsPanelIngame.AddToClassList("is-active");
    }

    private void HideSettingsPanel()
    {
        settingsPanelIngame.RemoveFromClassList("is-active");
        pauseMenu.AddToClassList("is-active");
    }

    private void ShowControlsPanel()
    {
        if (currentState != InGameState.Paused) return;
        settingsPanelIngame.RemoveFromClassList("is-active");
        controlsPanelIngame.AddToClassList("is-active");
    }

    private void HideControlsPanel()
    {
        if (currentState != InGameState.Paused) return;
        controlsPanelIngame.RemoveFromClassList("is-active");
        settingsPanelIngame.AddToClassList("is-active");
    }

    private void ShowHowToPlayPanel()
    {
        if (currentState != InGameState.Paused) return;
        pauseMenu.RemoveFromClassList("is-active");
        SwitchHtpTab("casting");
        howToPlayPanel.AddToClassList("is-active");
    }

    private void HideHowToPlayPanel()
    {
        howToPlayPanel.RemoveFromClassList("is-active");
        pauseMenu.AddToClassList("is-active");
    }

    // This is the public method Teleporter.cs will call
    public void ShowTravelPanel()
    {
        if (currentState != InGameState.Idle) return;
        PopulateTravelList();
        ShowPanel(travelPanel);
    }

    // This is the public method Teleporter.cs will call
    public void HideTravelPanel()
    {
        // Only hide if the travel panel is the one open
        if (currentState == InGameState.PanelOpen && travelPanel.ClassListContains("is-active"))
        {
            HideAllPanels();
        }
    }

    private void SwitchInventoryTab(string tabName)
    {
        invContentFish.AddToClassList("hidden");
        invContentRods.AddToClassList("hidden");
        invContentItems.AddToClassList("hidden");
        invTabFish.RemoveFromClassList("tab-button--active");
        invTabRods.RemoveFromClassList("tab-button--active");
        invTabItems.RemoveFromClassList("tab-button--active");

        if (tabName == "fish")
        {
            invContentFish.RemoveFromClassList("hidden");
            invTabFish.AddToClassList("tab-button--active");
            PopulateInventoryFishList();
        }
        else if (tabName == "rods")
        {
            invContentRods.RemoveFromClassList("hidden");
            invTabRods.AddToClassList("tab-button--active");
            PopulateInventoryRodList();
        }
        else if (tabName == "items")
        {
            invContentItems.RemoveFromClassList("hidden");
            invTabItems.AddToClassList("tab-button--active");
        }
    }

    private void SwitchHtpTab(string tabName)
    {
        htpContentCasting.AddToClassList("hidden");
        htpContentReeling.AddToClassList("hidden");
        htpContentSelling.AddToClassList("hidden");
        htpTabCasting.RemoveFromClassList("tab-button--active");
        htpTabReeling.RemoveFromClassList("tab-button--active");
        htpTabSelling.RemoveFromClassList("tab-button--active");

        if (tabName == "casting")
        {
            htpContentCasting.RemoveFromClassList("hidden");
            htpTabCasting.AddToClassList("tab-button--active");
        }
        else if (tabName == "reeling")
        {
            htpContentReeling.RemoveFromClassList("hidden");
            htpTabReeling.AddToClassList("tab-button--active");
        }
        else if (tabName == "selling")
        {
            htpContentSelling.RemoveFromClassList("hidden");
            htpTabSelling.AddToClassList("tab-button--active");
        }
    }

    private void PopulateFishList()
    {
        if (fishListItemTemplate == null || playerInventory == null) return;
        fishListScrollView.Clear();
        int totalValue = 0;

        foreach (var fishInstance in playerInventory.GetCaughtFishes())
        {
            var fishItemInstance = fishListItemTemplate.Instantiate();

            fishItemInstance.Q<Label>("fish-name").text = fishInstance.baseData.fishName;
            fishItemInstance.Q<Label>("fish-weight").text = $"{fishInstance.actualWeight:F2} kg";

            int price = Mathf.RoundToInt(fishInstance.baseData.CostPerKg * fishInstance.actualWeight);
            fishItemInstance.Q<Label>("fish-price").text = price.ToString("N0");

            var fishIconElement = fishItemInstance.Q<VisualElement>("fish-icon");
            if (fishInstance.baseData.fishIcon != null)
            {
                fishIconElement.style.backgroundImage = new StyleBackground(fishInstance.baseData.fishIcon);
            }

            fishListScrollView.Add(fishItemInstance);
            totalValue += price;
        }
        totalValueLabel.text = $"Total: {totalValue:N0}";
    }

    private void PopulateShopList()
    {
        if (shopListItemTemplate == null) return;

        if (shopRodDatabase == null)
        {
            Debug.LogError("RodDatabaseSO has not been assigned in InGameUIManager!");
            return;
        }

        shopItemListScrollView.Clear();

        foreach (var rodData in shopRodDatabase.allRods)
        {
            var itemInstance = shopListItemTemplate.Instantiate();
            itemInstance.Q<Label>("item-name").text = rodData.rodName;
            itemInstance.Q<Label>("item-stats").text = $"Pull Power: {rodData.pullPower}";
            itemInstance.Q<Label>("item-price").text = rodData.price.ToString("N0");

            var iconElement = itemInstance.Q<VisualElement>("item-icon");
            if (rodData.rodIcon != null)
            {
                iconElement.style.backgroundImage = new StyleBackground(rodData.rodIcon);
            }

            var buyButton = itemInstance.Q<Button>("buy-button");

            if (playerInventory.ownedRods.Contains(rodData))
            {
                if (playerInventory.currentRod == rodData)
                {
                    buyButton.text = "Equipped";
                    buyButton.SetEnabled(false);
                }
                else
                {
                    buyButton.text = "Equip";
                    buyButton.clicked += () => {
                        audioManager?.PlaySFX("Click");
                        EquipRod(rodData);
                    };
                }
            }
            else
            {
                buyButton.text = "Buy";
                buyButton.clicked += () =>
                {
                    audioManager?.PlaySFX("Click");
                    BuyItem(rodData);
                };
            }
            shopItemListScrollView.Add(itemInstance);
        }
    }

    private void PopulateInventoryFishList()
    {
        if (fishListItemTemplate == null || inventoryFishList == null || playerInventory == null) return;
        inventoryFishList.Clear();

        foreach (var fishInstance in playerInventory.GetCaughtFishes())
        {
            var fishItemInstance = fishListItemTemplate.Instantiate();

            fishItemInstance.Q<Label>("fish-name").text = fishInstance.baseData.fishName;
            fishItemInstance.Q<Label>("fish-weight").text = $"{fishInstance.actualWeight:F2} kg";

            int price = Mathf.RoundToInt(fishInstance.baseData.CostPerKg * fishInstance.actualWeight);
            fishItemInstance.Q<Label>("fish-price").text = price.ToString("N0");
            fishItemInstance.Q<Label>("fish-price").style.color = Color.gray;

            var fishIconElement = fishItemInstance.Q<VisualElement>("fish-icon");
            if (fishInstance.baseData.fishIcon != null)
            {
                fishIconElement.style.backgroundImage = new StyleBackground(fishInstance.baseData.fishIcon);
            }

            inventoryFishList.Add(fishItemInstance);
        }
    }

    private void PopulateInventoryRodList()
    {
        if (shopListItemTemplate == null || inventoryRodList == null || playerInventory == null) return;
        inventoryRodList.Clear();

        foreach (var rodData in playerInventory.ownedRods)
        {
            var itemInstance = shopListItemTemplate.Instantiate();
            itemInstance.Q<Label>("item-name").text = rodData.rodName;
            itemInstance.Q<Label>("item-stats").text = $"Pull Power: {rodData.pullPower}";

            var priceContainer = itemInstance.Q<VisualElement>(className: "price-container");
            if (priceContainer != null)
            {
                priceContainer.style.display = DisplayStyle.None;
            }

            var iconElement = itemInstance.Q<VisualElement>("item-icon");
            if (rodData.rodIcon != null)
            {
                iconElement.style.backgroundImage = new StyleBackground(rodData.rodIcon);
            }

            var equipButton = itemInstance.Q<Button>("buy-button");

            if (playerInventory.currentRod == rodData)
            {
                equipButton.text = "Equipped";
                equipButton.SetEnabled(false);
            }
            else
            {
                equipButton.text = "Equip";
                equipButton.clicked += () => {
                    audioManager?.PlaySFX("Click");
                    EquipRod(rodData);
                };
            }

            inventoryRodList.Add(itemInstance);
        }
    }

    private void PopulateTravelList()
    {
        if (travelCardTemplate == null || travelCardListContainer == null || mapDatabase == null || playerInventory == null)
        {
            Debug.LogError("Cannot populate travel list. A required component is missing.");
            return;
        }

        travelCardListContainer.Clear();

        foreach (var map in mapDatabase.allMaps)
        {
            var card = travelCardTemplate.Instantiate();
            card.Q<Label>("travel-card-name").text = map.mapName;
            card.Q<Label>("travel-card-description").text = map.mapDescription;

            var costLabel = card.Q<Label>("travel-card-cost-value");
            var travelButton = card.Q<Button>("travel-button");
            var cardImage = card.Q<VisualElement>("travel-card-image");

            if (map.mapImage != null)
            {
                cardImage.style.backgroundImage = new StyleBackground(map.mapImage);
            }

            if (playerInventory.currentSceneName == map.sceneName)
            {
                costLabel.text = "Current";
                travelButton.text = "Current";
                travelButton.SetEnabled(false);
            }
            else if (playerInventory.unlockedSceneNames.Contains(map.sceneName))
            {
                costLabel.text = "Free";
                travelButton.text = "Travel";
                travelButton.clicked += () =>
                {
                    audioManager?.PlaySFX("Click");
                    TravelToLocation(map);
                };
            }
            else
            {
                costLabel.text = map.unlockPrice.ToString("N0");
                travelButton.text = "Unlock";
                travelButton.clicked += () =>
                {
                    audioManager?.PlaySFX("Click");
                    UnlockMap(map);
                };
            }

            travelCardListContainer.Add(card);
        }
    }

    private void BuyItem(FishingRodData rodData)
    {
        if (playerInventory.money >= rodData.price)
        {
            playerInventory.money -= rodData.price;
            playerInventory.ownedRods.Add(rodData);
            audioManager?.PlaySFX("Coin");
            UpdatePlayerHUD();

            root.schedule.Execute(PopulateShopList);

            Debug.Log($"Bought {rodData.rodName}!");
        }
        else
        {
            Debug.Log($"Not enough money for {rodData.rodName}");
            audioManager?.PlaySFX("Error");
        }
    }

    private void SellAllFish()
    {
        if (playerInventory == null) return;

        if (playerInventory.GetCaughtFishes().Count > 0)
        {
            audioManager?.PlaySFX("Coin");
            playerInventory.SellAllFish();
            UpdatePlayerHUD();
            PopulateFishList();
            Debug.Log("Called SellAllFish from PlayerInventorySO.");
        }
    }

    private void EquipRod(FishingRodData rodToEquip)
    {
        if (playerInventory == null || rodToEquip == null) return;

        playerInventory.currentRod = rodToEquip;

        UpdatePlayerHUD();

        root.schedule.Execute(() => {
            PopulateInventoryRodList();
            PopulateShopList();
        });

        Debug.Log($"Equipped {rodToEquip.rodName}");
    }

    private void UnlockMap(MapData map)
    {
        if (playerInventory.money >= map.unlockPrice)
        {
            playerInventory.money -= map.unlockPrice;
            playerInventory.unlockedSceneNames.Add(map.sceneName);
            audioManager?.PlaySFX("Coin");
            UpdatePlayerHUD();

            root.schedule.Execute(PopulateTravelList);

            Debug.Log($"Unlocked {map.mapName}!");
        }
        else
        {
            Debug.Log($"Not enough money to unlock {map.mapName}");
            audioManager?.PlaySFX("Error");
        }
    }

    private void TravelToLocation(MapData map)
    {
        if (playerInventory.currentSceneName == map.sceneName) return;

        playerInventory.currentSceneName = map.sceneName;
        UpdatePlayerHUD();
        Debug.Log($"Traveling to {map.mapName}...");
        HideAllPanels();
        StartCoroutine(FadeInAndExit(() =>
        {
            SceneManager.LoadScene(map.sceneName);
        }));
    }

    private IEnumerator InitialFadeOut(float delay)
    {
        yield return new WaitForSeconds(delay);

        loadingScreen.RemoveFromClassList("fade-in");
        isLoadingScreenVisible = false;

        yield return new WaitForSeconds(2.0f);

        loadingScreen.style.visibility = Visibility.Hidden;
    }

    private IEnumerator FadeInAndExit(System.Action exitAction)
    {
        loadingScreen.style.visibility = Visibility.Visible;
        loadingScreen.AddToClassList("fade-in");
        isLoadingScreenVisible = true;

        yield return new WaitForSecondsRealtime(2.0f);

        exitAction?.Invoke();
    }

    private IEnumerator ToggleLoadingScreen(bool show, float delay = 0.5f)
    {
        if (show == isLoadingScreenVisible) yield break;

        isLoadingScreenVisible = show;
        if (show)
        {
            loadingScreen.style.visibility = Visibility.Visible;
            loadingScreen.AddToClassList("fade-in");
            yield return new WaitForSecondsRealtime(delay);
            if (isLoadingScreenVisible)
            {
                StartCoroutine(ToggleLoadingScreen(false));
            }
        }
        else
        {
            loadingScreen.RemoveFromClassList("fade-in");
            yield return new WaitForSecondsRealtime(2.0f);
            loadingScreen.style.visibility = Visibility.Hidden;
        }
    }

    public InGameState GetCurrentState()
    {
        return currentState;
    }
}