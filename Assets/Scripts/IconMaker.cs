using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class IconMaker : MonoBehaviour
{
    public Image icon;

    public Camera cam;
    public GameObject item;


    public Sprite GetIcon()
    {
        cam.orthographicSize = item.GetComponent<Renderer>().bounds.extents.y + 0.1f;
        
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
        
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
    }

    void Start()
    {
        icon.sprite = GetIcon();
    }
    
    void Update()
    {
        
    }
}
