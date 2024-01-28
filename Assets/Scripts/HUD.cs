using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    private readonly string prototypeVersion = "prototype-2";

    [Header("UI Elements")]
    public TextMeshProUGUI text;
    public TextMeshProUGUI inventoryText; // Will be replaced with a UI element
    public Image healthBar;
    public Color fullHealthColor, lowHealthColor;

    [Header("Inventory")]
    public PlayerInventory playerInventoryObject;
    public GameObject inventoryIconContainer;

    private Canvas canvas;
    private InventoryItem<Entity>[] inventory;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
        inventory = playerInventoryObject.getInventory();

        UpdateBuildInfoText();
    }

    private void Update()
    {
        UpdateInventoryIcons();
        UpdateHealthBar();
    }

    private void UpdateBuildInfoText()
    {
        string buildInfo = $"Build: {Application.version} Platform: {Application.platform} Unity: {Application.unityVersion} OS: {SystemInfo.operatingSystem}";
        buildInfo += $"\n{Application.productName}, {Application.companyName}, {prototypeVersion}";
        text.text = buildInfo;
    }

    private void UpdateInventoryIcons()
    {
        float iconWidth = inventoryIconContainer.GetComponent<RectTransform>().rect.width / inventory.Length;

        // Clear existing icons in inventoryIconContainer
        foreach (Transform child in inventoryIconContainer.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < inventory.Length; i++)
        {
            var inventoryItem = inventory[i];

            if (inventoryItem.item == null)
            {
                continue;
            }

            CreateInventoryIcon(i + 1, iconWidth, inventoryItem.item, playerInventoryObject.GetSelectedBlockIndex() == i);
        }
    }

    private void CreateInventoryIcon(int index, float iconWidth, Entity entity,bool selected = false)
    {
        if (entity.icon == null)
        {
            CreatePlaceholderText(index, iconWidth, entity, selected);
        }
        else
        {
            CreateIconObject(index, iconWidth, entity, selected);
        }
    }

    private void CreatePlaceholderText(int index, float iconWidth, Entity entity, bool selected = false)
    {
        TextMeshProUGUI placeholderText = GetPlaceholderText(entity);
        placeholderText.rectTransform.sizeDelta = new Vector2(iconWidth, iconWidth);
        placeholderText.rectTransform.SetParent(inventoryIconContainer.transform);
        placeholderText.transform.position = new Vector3(index * iconWidth, inventoryIconContainer.transform.position.y, 0);
        placeholderText.color = selected ? Color.red : Color.black;
    }

    private void CreateIconObject(int index, float iconWidth, Entity entity, bool selected = false)
    {
        GameObject iconObject = new GameObject(entity.name + " Icon");

        Image image = iconObject.AddComponent<Image>();
        image.sprite = entity.icon;
        image.preserveAspect = true;
        image.rectTransform.sizeDelta = new Vector2(iconWidth, iconWidth);
        image.rectTransform.SetParent(inventoryIconContainer.transform);
        image.transform.position = new Vector3(index * iconWidth, inventoryIconContainer.transform.position.y, 0);
        image.transform.localScale = new Vector3(.5f, .5f, .5f);
        image.color = selected ? Color.red : Color.white;
    }

    private void UpdateHealthBar()
    {
        float normalizedHealth = (float)playerInventoryObject.GetCurrentHealth() / playerInventoryObject.GetMaxHealth();

        healthBar.transform.localScale = new Vector3(normalizedHealth, 1, 1);
        healthBar.color = Color.Lerp(lowHealthColor, fullHealthColor, normalizedHealth);
    }

    private TextMeshProUGUI GetPlaceholderText(Entity entity)
    {
        GameObject textObject = new GameObject(entity.name + " Text");
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();

        text.text = entity.name;
        text.fontSize = 20;
        text.color = Color.black;

        return text;
    }
}
