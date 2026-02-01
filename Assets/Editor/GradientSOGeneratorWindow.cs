using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GradientSOGeneratorWindow : EditorWindow
{
    string outputFolder = "Assets/UIThemes/Gradients";
    bool overwriteExisting = false;
    bool createSubfoldersByCollection = true;

    [MenuItem("Tools/UI/Generate GradientSO Themes")]
    static void Open() => GetWindow<GradientSOGeneratorWindow>("GradientSO Generator");

    void OnGUI()
    {
        GUILayout.Label("Generate GradientSO Themes (by Collection)", EditorStyles.boldLabel);

        outputFolder = EditorGUILayout.TextField("Root Folder", outputFolder);
        createSubfoldersByCollection = EditorGUILayout.Toggle("Subfolders by Collection", createSubfoldersByCollection);
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing", overwriteExisting);

        EditorGUILayout.Space(8);

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

            var existing = AssetDatabase.LoadAssetAtPath<GradientSO>(path);

            if (existing != null && !overwriteExisting)
            {
                skipped++;
                continue;
            }

            GradientSO asset = existing;
            if (asset == null)
            {
                asset = CreateInstance<GradientSO>();
                AssetDatabase.CreateAsset(asset, path);
                created++;
            }
            else
            {
                updated++;
            }

            asset.MainGradient = CreateGradient(def.hexStops);

            bool darkText = ShouldUseDarkText(def.hexStops);
            asset.UseDarkText = darkText;

            asset.TextColor = darkText ? new Color(0.08f, 0.10f, 0.14f, 1f) : Color.white;
            asset.ButtonColor = PickButtonColor(def.hexStops, darkText);

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
        // Acento = última cor. Pra synthwave isso funciona muito bem.
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

        // 15 Relaxing
        list.Add(new Def("Relaxing", "Mist Dawn", "#EAF4FF", "#CFE7F5", "#BFD4E6"));
        list.Add(new Def("Relaxing", "Lavender Calm", "#F2ECFF", "#D7C8FF", "#BBA7F5"));
        list.Add(new Def("Relaxing", "Sage Breeze", "#E9F3EE", "#CBE3D7", "#A9CBBE"));
        list.Add(new Def("Relaxing", "Sea Glass", "#DFF7F5", "#AEE7E3", "#6DCBC7"));
        list.Add(new Def("Relaxing", "Peach Hush", "#FFF1EB", "#FFD7C2", "#F7B79A"));
        list.Add(new Def("Relaxing", "Sand and Sky", "#F7F1E3", "#D8E7F3", "#A9C8E8"));
        list.Add(new Def("Relaxing", "Soft Mint", "#ECFFF7", "#BFF2DE", "#7FD9BF"));
        list.Add(new Def("Relaxing", "Cloudy Blue", "#F3F8FF", "#D8E6FF", "#AFC7F5"));
        list.Add(new Def("Relaxing", "Rose Water", "#FFF0F4", "#F7C8D6", "#E59CB7"));
        list.Add(new Def("Relaxing", "Evening Fog", "#EDEFF3", "#C8CEDA", "#9AA4B2"));
        list.Add(new Def("Relaxing", "Icy Lilac", "#F7F3FF", "#DDD3FF", "#B8A3FF"));
        list.Add(new Def("Relaxing", "Driftwood", "#F6F2EB", "#D9CBB8", "#B59E82"));
        list.Add(new Def("Relaxing", "Aqua Whisper", "#E7FEFF", "#B7F3F5", "#7EDFE6"));
        list.Add(new Def("Relaxing", "Sunset Milk", "#FFF6F0", "#FFD9E2", "#CBB8FF"));
        list.Add(new Def("Relaxing", "Night Spa", "#0B1320", "#1B2E3E", "#2F5D62"));

        // 15 Synthwave (no lugar do Cool)
        list.Add(new Def("Synthwave", "Neon Violet", "#0B0620", "#2A0B5E", "#B517FF"));
        list.Add(new Def("Synthwave", "Hot Magenta", "#12001C", "#3A0057", "#FF2BD6"));
        list.Add(new Def("Synthwave", "Laser Cyan", "#050816", "#0B2C5A", "#00E5FF"));
        list.Add(new Def("Synthwave", "Miami Sunset", "#0B1026", "#7B1FA2", "#FF4D6D"));
        list.Add(new Def("Synthwave", "Arcade Orange", "#0B0F1A", "#2B1644", "#FF7A00"));
        list.Add(new Def("Synthwave", "Retro Pink Sky", "#140A2A", "#4B1B73", "#FF77E9"));
        list.Add(new Def("Synthwave", "Electric Grape", "#0B0614", "#2A0B3D", "#A855F7"));
        list.Add(new Def("Synthwave", "Purple Heat", "#070A16", "#3B0F3F", "#FF2E63"));
        list.Add(new Def("Synthwave", "Neon Horizon", "#030712", "#1E1B4B", "#38BDF8"));
        list.Add(new Def("Synthwave", "VHS Glow", "#0C1020", "#3A155F", "#00F5D4"));
        list.Add(new Def("Synthwave", "Ultraviolet Wave", "#050816", "#34106B", "#6D28D9"));
        list.Add(new Def("Synthwave", "Pink and Blue Rush", "#0A0B1E", "#1D4ED8", "#FF3DF2"));
        list.Add(new Def("Synthwave", "Neon Grid", "#060A14", "#0F2A33", "#B7FF00"));
        list.Add(new Def("Synthwave", "Crimson Synth", "#090A10", "#2A0B3D", "#FF003D"));
        list.Add(new Def("Synthwave", "Glitch Cyan Purple", "#050816", "#1F1147", "#00D9FF"));

        return list;
    }
}
