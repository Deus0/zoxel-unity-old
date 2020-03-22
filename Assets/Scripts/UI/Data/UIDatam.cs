using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

namespace Zoxel
{
    [CreateAssetMenu(fileName = "UIData", menuName = "ZoxelUI/UIData")]
    public class UIDatam : ScriptableObject
    {

        [Header("Panel")]
        public float orbitLerpSpeed;    // 12
        public float orbitDepth;        // 0.25, 0.5?
        public float2 defaultIconSize;  // 0.06
        public Material defaultPlayerPanel;
        public Material defaultPlayerIcon;
        public Material defaultPlayerOutline;
        public Color selectedMenuColor;
        public Color defaultMenuColor;
        public Color overlayTextColor;
        public Color menuTextColor;

        [Header("Font")]
        public float fontSize;          // 0.24
        public FontDatam font;

        [Header("Menu")]
        public Material menuButton;
        public float buttonAnimationTime;
        public float2 menuPadding;
        public float2 menuMargins;

        [Header("Crosshair")]
        public float crosshairLerpSpeed;
        public float crosshairSize;
        public float3 crosshairPosition;
        public Material crosshairMaterial;

        [Header("Grid Sizes")]
        public float2 questlogGridSize;
        public float2 inventoryGridSize;
        public float2 statsUIGridSize;
        public int skillbarIconsCount;      // 10

        [Header("Actionbar")]
        public Material actionbarPanel;
        public float2 actionbarIconSize;  // 0.04f, 0.06?
        public float3 actionbarPosition;// = new float3(0, -0.12f, 0.5f);
        public float2 skillbarMargins;
        public float skillbarPadding;

        [Header("Inventory")]
        public Texture2D defaultItemIcon;

        [Header("Map")]
        public Material mapPanel;
        public Material mapIcon;

        [Header("Statbars")]
        public float fadeIn;    // 1f
        public float fadeOut;   // 1.5f
        public Material frontBarMaterial;
        public Material backBarMaterial;
        public Color backbarColor;
        public Color frontbarColor;

        [Header("Popups")]
        public float2 popupLifetime; // 2, 3
        public float2 popupVariationX;// = new float2(-0.05f, 0.05f);
        public float2 popupVariationY;// = new float2(3, 4);
        public float2 popupVariationZ;// = new float2(-0.05f, 0.05f);
    }
}

//[Header("Selection UI")]
//public Texture2D selectedDefault;
//public Texture2D selectedActioning;
//public Material selectedMaterial;
//[Header("Input")]
//public Texture2D cursor;
//public GameObject controllerAnimation;
//public GameObject keyboardAnimation;
//public GameObject touchAnimation;

/*[Header("MainMenu")]
public Material menuSelectionMaterial;
public Texture2D menuSelectionTexture;*/
//public Material inventoryPanel;
// public Material inventoryIcon;
// [Header("Stats UI")]
//public Material statsUIPanel;
//public Material statsUIIcon;
