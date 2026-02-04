using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GradientSOGeneratorWindow : EditorWindow
{
    string outputFolder = "Assets/UIThemes/Gradients";
    bool overwriteExisting  = false;
    bool createSubfoldersByCollection = true;

    [Header("Derived Colors Tuning")]
    float glowIntensity = 4f;
    float skyIntensity = 1.5f;
    float fogIntensity = 1.25f;

    float skySaturationBoost = 1.1f;
    float fogDesaturate = 0.35f;

    [MenuItem("Tools/UI/Generate GradientSO Themes")]
    static void Open() => GetWindow<GradientSOGeneratorWindow>("GradientSO Generator");

    void OnGUI()
    {
        GUILayout.Label("Generate GradientSO Themes (by Collection)", EditorStyles.boldLabel);

        outputFolder = EditorGUILayout.TextField("Root Folder", outputFolder);
        createSubfoldersByCollection = EditorGUILayout.Toggle("Subfolders by Collection", createSubfoldersByCollection);
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing", overwriteExisting);

        EditorGUILayout.Space(8);
        GUILayout.Label("Derived Colors", EditorStyles.boldLabel);

        glowIntensity = EditorGUILayout.Slider("Glow Intensity (HDR)", glowIntensity, 0f, 12f);
        skyIntensity = EditorGUILayout.Slider("Sky Intensity (HDR)", skyIntensity, 0f, 6f);
        fogIntensity = EditorGUILayout.Slider("Fog Intensity (HDR)", fogIntensity, 0f, 6f);

        skySaturationBoost = EditorGUILayout.Slider("Sky Saturation Boost", skySaturationBoost, 0.5f, 2.0f);
        fogDesaturate = EditorGUILayout.Slider("Fog Desaturate", fogDesaturate, 0f, 1f);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Generate / Update"))
            Generate();
    }

    void Generate()
    {
        EnsureFolder(outputFolder);

        int created = 0;
        int updated = 0;
        int skipped = 0;

        foreach (var def in GetDefs())
        {
            string folder = outputFolder;
            if (createSubfoldersByCollection)
            {
                folder = $"{outputFolder}/{Sanitize(def.collection)}";
                EnsureFolder(folder);
            }

            string fileName = $"Theme_{Sanitize(def.name)}.asset";
            string path = $"{folder}/{fileName}";

            var existing = AssetDatabase.LoadAssetAtPath<ThemeSO>(path);

            if (existing != null && !overwriteExisting)
            {
                skipped++;
                continue;
            }

            ThemeSO asset = existing;
            if (asset == null)
            {
                asset = CreateInstance<ThemeSO>();
                AssetDatabase.CreateAsset(asset, path);
                created++;
            }
            else
            {
                updated++;
            }

            var gradient = CreateGradient(def.hexStops);
            asset.MainGradient = gradient;

            bool darkText = ShouldUseDarkText(def.hexStops);
            asset.UseDarkText = darkText;
            asset.TextColor = darkText ? new Color(0.08f, 0.10f, 0.14f, 1f) : Color.white;
            asset.ButtonColor = PickButtonColor(def.hexStops, darkText);

            Color top = gradient.Evaluate(0f);
            Color mid = gradient.Evaluate(0.5f);
            Color bottom = gradient.Evaluate(1f);

            Color accent = ParseHex(def.hexStops[def.hexStops.Length - 1]);

            // Glow (HDR): acento mais forte
            asset.GlowColor = ToHdr(accent, glowIntensity);

            // Sky (HDR): top mais vibrante, bottom mais suave/escuro
            Color skyTop = BoostSaturation(top, skySaturationBoost);
            Color skyBottom = Desaturate(bottom, 0.15f);

            asset.SkyTopColor = ToHdr(skyTop, skyIntensity);
            asset.SkyBottomColor = ToHdr(skyBottom, skyIntensity);

            // Fog (HDR): baseado no mid, desaturado pra parecer neblina
            Color fog = Desaturate(mid, fogDesaturate);
            asset.FakeFogColor = ToHdr(fog, fogIntensity);

            EditorUtility.SetDirty(asset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"GradientSO Themes done. Created: {created}, Updated: {updated}, Skipped: {skipped}. Root: {outputFolder}");
    }

    static Gradient CreateGradient(string[] hexStops)
    {
        var colors = new Color[hexStops.Length];
        for (int i = 0; i < hexStops.Length; i++)
            colors[i] = ParseHex(hexStops[i]);

        var gradient = new Gradient();

        var colorKeys = new GradientColorKey[colors.Length];
        var alphaKeys = new GradientAlphaKey[colors.Length];

        for (int i = 0; i < colors.Length; i++)
        {
            float t = colors.Length == 1 ? 0f : (float)i / (colors.Length - 1);
            colorKeys[i] = new GradientColorKey(colors[i], t);
            alphaKeys[i] = new GradientAlphaKey(1f, t);
        }

        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }

    static Color ParseHex(string hex)
    {
        if (!hex.StartsWith("#")) hex = "#" + hex;
        if (!ColorUtility.TryParseHtmlString(hex, out var c))
            c = Color.magenta;
        return c;
    }

    static bool ShouldUseDarkText(string[] hexStops)
    {
        var mid = ParseHex(hexStops[hexStops.Length / 2]);
        float lum = 0.2126f * mid.r + 0.7152f * mid.g + 0.0722f * mid.b;
        return lum > 0.62f;
    }

    static Color PickButtonColor(string[] hexStops, bool darkText)
    {
        var accent = ParseHex(hexStops[hexStops.Length - 1]);

        if (darkText)
        {
            Color.RGBToHSV(accent, out float h, out float s, out float v);
            s = Mathf.Clamp01(s * 0.75f);
            v = Mathf.Clamp01(Mathf.Max(v, 0.75f));
            return Color.HSVToRGB(h, s, v);
        }

        return accent;
    }

    static Color ToHdr(Color c, float intensity)
    {
        return c * Mathf.Max(0f, intensity);
    }

    static Color BoostSaturation(Color c, float multiplier)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        s = Mathf.Clamp01(s * multiplier);
        return Color.HSVToRGB(h, s, v);
    }

    static Color Desaturate(Color c, float amount01)
    {
        float gray = 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
        return Color.Lerp(c, new Color(gray, gray, gray, c.a), Mathf.Clamp01(amount01));
    }

    static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;

        string parent = "Assets";
        string[] parts = folder.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i];
            if (i == 0 && part == "Assets") continue;

            string current = $"{parent}/{part}";
            if (!AssetDatabase.IsValidFolder(current))
                AssetDatabase.CreateFolder(parent, part);

            parent = current;
        }
    }

    static string Sanitize(string value)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            value = value.Replace(c, '-');

        value = value.Replace(" ", "-");
        value = value.Replace("&", "and");
        return value;
    }

    struct Def
    {
        public string collection;
        public string name;
        public string[] hexStops;

        public Def(string collection, string name, params string[] hexStops)
        {
            this.collection = collection;
            this.name = name;
            this.hexStops = hexStops;
        }
    }

    static List<Def> GetDefs()
    {
        var list = new List<Def>();

        // Relaxing
        list.Add(new Def("Relaxing", "Mist Dawn", "#EAF4FF", "#CFE7F5", "#BFD4E6"));
        list.Add(new Def("Relaxing", "Lavender Calm", "#F2ECFF", "#D7C8FF", "#BBA7F5"));
        list.Add(new Def("Relaxing", "Sage Breeze", "#E9F3EE", "#CBE3D7", "#A9CBBE"));
        list.Add(new Def("Relaxing", "Sea Glass", "#DFF7F5", "#AEE7E3", "#6DCBC7"));
        list.Add(new Def("Relaxing", "Peach Hush", "#FFF1EB", "#FFD7C2", "#F7B79A"));
        list.Add(new Def("Relaxing", "Night Spa", "#0B1320", "#1B2E3E", "#2F5D62"));

        // Synthwave
        list.Add(new Def("Synthwave", "Neon Violet", "#0B0620", "#2A0B5E", "#B517FF"));
        list.Add(new Def("Synthwave", "Hot Magenta", "#12001C", "#3A0057", "#FF2BD6"));
        list.Add(new Def("Synthwave", "Laser Cyan", "#050816", "#0B2C5A", "#00E5FF"));
        list.Add(new Def("Synthwave", "Miami Sunset", "#0B1026", "#7B1FA2", "#FF4D6D"));
        list.Add(new Def("Synthwave", "Neon Grid", "#060A14", "#0F2A33", "#B7FF00"));

        // New Collections (exemplos)
        list.Add(new Def("Horror", "Cold Basement", "#05060A", "#10131C", "#2A2F3A"));
        list.Add(new Def("Horror", "Blood Lamp", "#08060A", "#2A0B12", "#FF003D"));
        list.Add(new Def("Warm", "Golden Hour", "#FFF3D6", "#FFC98A", "#FF7A00"));
        list.Add(new Def("Warm", "Terracotta", "#2A0F08", "#8C3B1E", "#F2A65A"));
        list.Add(new Def("Ocean", "Deep Current", "#020617", "#0B2C5A", "#38BDF8"));
        list.Add(new Def("Ocean", "Seafoam", "#041B1D", "#0EA5A8", "#A7F3D0"));

        return list;
    }
}
