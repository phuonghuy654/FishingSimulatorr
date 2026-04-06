using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public GameObject shopPanel;
    public bool isShopOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    public void OpenShop()
    {
        if (shopPanel == null) return;

        shopPanel.SetActive(true);
        isShopOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseShop()
    {
        if (shopPanel == null) return;

        shopPanel.SetActive(false);
        isShopOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
