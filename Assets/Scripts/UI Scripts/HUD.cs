using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    private readonly string prototypeVersion = "prototype-2";

    [Header("UI Elements")]
    public TextMeshProUGUI text;
    public TextMeshProUGUI nightTextItem;
    public TextMeshProUGUI currentItemText;
    public Image healthBar;
    public Color fullHealthColor, lowHealthColor;
    public GameObject inventorySlotPrefab;

    [Header("Inventory")]
    public PlayerInventory playerInventoryObject;
    public GameObject inventoryIconContainer;
    
    [Header("Rendering")]
    public Camera camera;

    private Canvas canvas;
    private InventoryItem<Entity>[] inventory;
    private GameObject[] inventoryIcons;
    private GameObject[] craftableIcons;
    private bool craftingOpen = false;
    private int craftingIndex = 0;
    private int selectedBlockIndex = -100;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
        inventory = playerInventoryObject.GetInventory();

        UpdateBuildInfoText();

        CreateIcons(ref inventoryIcons, inventory);
        currentItemText.gameObject.SetActive(false);
    }
    
    public void SetPlayerInventory(PlayerInventory playerInventory)
    {
        playerInventoryObject = playerInventory;
        inventory = playerInventoryObject.GetInventory();
        CreateIcons(ref inventoryIcons, inventory); 
    }

    private void CreateIcons(ref GameObject[] inventoryIcons, InventoryItem<Entity>[] inventory, int y = 0)
    {
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
            float slotPositionY = iconWidth + y;

            slotObject.transform.position = new Vector3(slotPositionX, slotPositionY, 0);
            inventoryIcons[i] = slotObject;
        }
    }
    
    IEnumerator ShowCurrentItemLabel(string text, float duration = 2)
    {
        currentItemText.text = text;
        currentItemText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        currentItemText.gameObject.SetActive(false);
    }
    
    void OnInventoryIndexChanged(int index)
    {
        if (index < 0 || index >= inventory.Length)
        {
            return;
        }
        var item = inventory[index];
        if (item.item == null)
        {
            return;
        }
        StartCoroutine(ShowCurrentItemLabel(item.item.name));
    }


    private void Update()
    {
        if (!craftingOpen)
            UpdateInventoryIcons();
        else
            UpdateCraftingIcons();
        

        UpdateHealthBar();
        UpdateNightText();
        
        if(selectedBlockIndex != playerInventoryObject.GetSelectedBlockIndex())
        {
            selectedBlockIndex = playerInventoryObject.GetSelectedBlockIndex();
            OnInventoryIndexChanged(selectedBlockIndex);
        }
        
        if (craftingOpen && craftableIcons == null)
        {
            ShowCraftableItems();
        }
        else if (!craftingOpen && craftableIcons != null)
        {
            for(int i = 0; i < craftableIcons.Length; i++)
            {
                Destroy(craftableIcons[i]);
            }
            craftableIcons = null;
        }
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            
            craftingOpen = !craftingOpen; 
            GameManager.Instance.InputEnabled = !craftingOpen;
        }

        if (Input.GetKeyDown(KeyCode.Return) && craftingOpen)
        {
            playerInventoryObject.CraftItem(craftingIndex);
            craftingOpen = false;
            GameManager.Instance.InputEnabled = true;
        }
    }
    
    private void UpdateNightText()
    {
        string nightText = $"Night: {GameManager.Instance.NightsSurvived}";
        nightTextItem.text = nightText;
    }

    private void UpdateBuildInfoText()
    {
        string buildInfo = $"Build: {Application.version} Platform: {Application.platform} Unity: {Application.unityVersion} OS: {SystemInfo.operatingSystem}";
        buildInfo += $"\n{Application.productName}, {Application.companyName}, {prototypeVersion}";
        text.text = buildInfo;
    }

    private void UpdateInventoryIcons()
    {
        float iconWidth = inventoryIconContainer.GetComponent<RectTransform>().rect.width /
                          playerInventoryObject.inventoryCapacity;

        // Clear existing icons in inventoryIconContainer
        foreach (Transform child in inventoryIconContainer.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < inventory.Length; i++)
        {
            var inventoryItem = inventory[i];
            

            RetrieveIcon(i, iconWidth, inventoryItem.item, inventoryItem.count,playerInventoryObject.GetSelectedBlockIndex() == i);
        }
    }

    private void UpdateCraftingIcons()
    {
        if(craftableIcons == null)
        {
            ShowCraftableItems();
            return;
        }
        for (int i = 0; i < craftableIcons.Length; ++i)
        {
            string num = i.ToString().Length == 1 ? i.ToString() : "0";
            if(Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                craftingIndex = i;
            }
        }
        for (int i = 0; i < craftableIcons.Length; ++i)
        {
            var icon = craftableIcons[i];
            icon.GetComponent<Image>().color = i == craftingIndex ? Color.red : Color.white;
        }
        
    }

    private void RetrieveIcon(int index, float iconWidth, Entity entity,int quantity,bool selected = false)
    {
        

        var selectedSlot = inventoryIcons[index];
        selectedSlot.GetComponent<Image>().color = selected ? Color.red : Color.white;

        var iconImage = selectedSlot.transform.GetChild(0);
        var text = selectedSlot.transform.GetChild(1);
        
        var image = iconImage.GetComponent<Image>();

        if (entity == null)
        {
            image.sprite = null;
            image.color = Color.clear;
            text.GetComponent<TextMeshProUGUI>().text = "(0)";
            return;
        }

        Sprite icon = GenerateIcon(entity, index);
        image.sprite = icon;
        image.color = Color.white;
        
        
        text.GetComponent<TextMeshProUGUI>().text = "("+quantity+")";
        
        inventoryIcons[index] = selectedSlot;

    }
    
    
    private Sprite GenerateIcon(Entity entity, int index)
    {
        if(entity.icon != null)
        {
            return entity.icon;
        }
        
        var camObject = Instantiate(this.camera , new Vector3(0,0,0), Quaternion.identity);
        var cam = camObject.GetComponent<Camera>();
        cam.name = "IconMakerCamera " + index;
        cam.transform.position = new Vector3(index*100,500,0);

        GameObject item = Instantiate(entity.prefab, cam.transform.position + cam.transform.forward * 2, Quaternion.identity);
        //Rotate to be angled in icon
        item.transform.Rotate(new Vector3(-20, 45, -20));
        var renderer = item.GetComponent<Renderer>();
        
        
        
        cam.orthographicSize = renderer != null ? renderer.bounds.extents.y + 0.1f : 1;
        
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

        foreach (Transform child in camObject.transform)
        {
            Destroy(child.gameObject);
            
        }
        Destroy(camObject);
        Destroy(item);
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
        entity.icon = sprite;
        return sprite;
    }


    private void ShowCraftableItems()
    {
        
        var allCraftableItems = playerInventoryObject.GetAllCraftableItems();
        var craftableItems = allCraftableItems.Select(item => new InventoryItem<Entity>(item.Item1, item.Item2)).ToArray();
        //Convert to 10 length array
        if (craftableItems.Length > 10)
        {
            craftableItems = craftableItems.Take(10).ToArray();
        }
        else
        {
            craftableItems = craftableItems.Concat(Enumerable.Repeat(new InventoryItem<Entity>(null, 0), 10 - craftableItems.Length)).ToArray();
        }
        
        
        CreateIcons(ref craftableIcons, craftableItems, Screen.height -200);

        for (int i = 0; i < craftableIcons.Length; ++i)
        {
            var icon = craftableIcons[i];
            var image = icon.transform.GetChild(0).GetComponent<Image>();
            var text = icon.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            var item = craftableItems[i];
            if (item.item == null)
            {
                image.sprite = null;
                image.color = Color.clear;
                text.text = "";
                continue;
            }
            image.sprite = GenerateIcon(item.item, i);
            image.color = Color.white;
            text.text = item.count.ToString(); 
        }

    }


    private void UpdateHealthBar()
    {
        float normalizedHealth = (float)playerInventoryObject.GetCurrentHealth() / playerInventoryObject.GetMaxHealth();

        healthBar.transform.localScale = new Vector3(normalizedHealth, 1, 1);
        healthBar.color = Color.Lerp(lowHealthColor, fullHealthColor, normalizedHealth);
    }
    
}
