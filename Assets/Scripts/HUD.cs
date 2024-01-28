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
    public GameObject inventorySlotPrefab;

    [Header("Inventory")]
    public PlayerInventory playerInventoryObject;
    public GameObject inventoryIconContainer;
    
    [Header("Rendering")]
    public Camera cam;

    private Canvas canvas;
    private InventoryItem<Entity>[] inventory;
    private GameObject[] inventoryIcons;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
        inventory = playerInventoryObject.getInventory();

        UpdateBuildInfoText();

        float containerWidth = Screen.width;
        float iconWidth = containerWidth / inventory.Length * 0.6f;
        float iconGap = 20;

        float totalLength = (iconWidth +iconGap)* inventory.Length;
        float startX = (containerWidth - totalLength) / 2 + iconWidth / 2; // Corrected
        
        inventoryIcons = new GameObject[inventory.Length];

        for (int i = 0; i < inventory.Length; i++)
        {
            // Instantiate inventory slot prefab
            GameObject slotObject = Instantiate(inventorySlotPrefab, canvas.transform);

            slotObject.GetComponent<RectTransform>().sizeDelta = new Vector2(iconWidth, iconWidth);

            float slotPositionX = startX + iconWidth * i + iconGap * i;
            float slotPositionY = iconWidth;

            slotObject.transform.position = new Vector3(slotPositionX, slotPositionY, 0);
            inventoryIcons[i] = slotObject;

            // Optionally, you can customize the appearance or behavior of each slot here
        }
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

            CreateInventoryIcon(i, iconWidth, inventoryItem.item, playerInventoryObject.GetSelectedBlockIndex() == i);
        }
    }

    private void CreateInventoryIcon(int index, float iconWidth, Entity entity,bool selected = false)
    {

        var selectedSlot = inventoryIcons[index];
        selectedSlot.GetComponent<Image>().color = selected ? Color.gray : Color.white;
        
        
        //TODO - Fix this
        // Sprite icon = GetIcon(cam, entity.prefab);
        // GameObject iconObject = new GameObject(entity.name + " Icon");
        // Image image = iconObject.AddComponent<Image>();
        // image.sprite = icon;
        // image.preserveAspect = true;
        // iconObject.transform.SetParent(selectedSlot.transform);
        
        

    }
    
    public Sprite GetIcon(Camera cam, GameObject item)
    {
        //Instantiate item
        var pos = cam.transform.position + cam.transform.forward * 2;
        GameObject itemObject = Instantiate(item, pos, Quaternion.identity);
        
        cam.orthographicSize = itemObject.GetComponent<Renderer>().bounds.extents.y + 0.1f;
        
        //Get dimensions 
        int resX = cam.pixelWidth;
        int resY = cam.pixelHeight;

        int clipX = 0;
        int clipY = 0;
        
        if(resX > resY)
        {
            clipX = resX - resY ;
        }
        else if (resY > resX)
        {
            clipY = resY - resX ;
        }
        
        
        //Initialize everything
        Texture2D tex = new Texture2D(resX - clipX, resY -clipY, TextureFormat.RGBA32, false);
        RenderTexture rt = new RenderTexture(resX, resY, 24);
        cam.targetTexture = rt;
        RenderTexture.active = rt;
        
        cam.Render();
        tex.ReadPixels(new Rect(clipX/2, clipY/2, resX, resY), 0, 0);
        tex.Apply();
        
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(itemObject);
        
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
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
