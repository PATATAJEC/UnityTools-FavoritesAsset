#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MasyoLab.Editor.FavoritesAsset
{
    class BookmarkAppearanceWindow : EditorWindow
    {
        private AssetData m_asset;
        private FavoritesManager m_favorites;
        private EditorWindow m_owner;
        private string m_suffix;
        private bool m_useColor;
        private Color m_color;
        private bool m_focusSuffix;

        private static readonly Color[] Palette =
        {
            new Color32(239, 108, 108, 255),
            new Color32(240, 166, 80, 255),
            new Color32(232, 207, 95, 255),
            new Color32(112, 192, 132, 255),
            new Color32(92, 182, 207, 255),
            new Color32(122, 153, 224, 255),
            new Color32(179, 135, 220, 255),
            new Color32(220, 140, 182, 255),
        };

        public static void Show(AssetData asset, FavoritesManager favorites, EditorWindow owner)
        {
            var window = CreateInstance<BookmarkAppearanceWindow>();
            window.titleContent = new GUIContent("Edit bookmark");
            window.m_asset = asset;
            window.m_favorites = favorites;
            window.m_owner = owner;
            window.m_suffix = asset.LabelSuffix ?? string.Empty;
            window.m_useColor = asset.UseCustomColor;
            window.m_color = asset.UseCustomColor ? asset.BookmarkColor : Palette[4];
            window.m_focusSuffix = true;
            window.minSize = window.maxSize = new Vector2(400f, 290f);
            // A utility window stays open when Unity's color picker takes focus.
            window.ShowUtility();
        }

        private void OnGUI()
        {
            // Transient editor state is intentionally discarded after a domain reload.
            if (m_asset == null || m_favorites == null)
            {
                Close();
                return;
            }

            var ev = Event.current;
            if (ev.type == EventType.KeyDown && ev.keyCode == KeyCode.Escape)
            {
                ev.Use();
                Close();
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.inspectorDefaultMargins))
            {
                EditorGUILayout.LabelField("Original asset", m_asset.Name);
                EditorGUILayout.LabelField(new GUIContent(m_asset.Path, m_asset.Path), EditorStyles.miniLabel);
                EditorGUILayout.Space();

                GUI.SetNextControlName("BookmarkSuffix");
                m_suffix = EditorGUILayout.TextField("Name suffix", m_suffix);
                if (m_focusSuffix && ev.type == EventType.Repaint)
                {
                    EditorGUI.FocusTextInControl("BookmarkSuffix");
                    m_focusSuffix = false;
                }

                var preview = string.IsNullOrWhiteSpace(m_suffix)
                    ? m_asset.Name : $"{m_asset.Name} — {m_suffix.Trim()}";
                EditorGUILayout.LabelField("Preview", preview);
                EditorGUILayout.Space();

                m_useColor = EditorGUILayout.Toggle("Color code", m_useColor);
                using (new EditorGUI.DisabledScope(!m_useColor))
                {
                    m_color = EditorGUILayout.ColorField(new GUIContent("Bookmark color"), m_color, true, false, false);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Quick colors", GUILayout.Width(EditorGUIUtility.labelWidth));
                    foreach (var color in Palette)
                    {
                        var swatch = GUILayoutUtility.GetRect(20f, 22f, GUILayout.Width(20f));
                        if (GUI.Button(swatch, new GUIContent(string.Empty, "Use this bookmark color")))
                        {
                            m_color = color;
                            m_useColor = true;
                        }
                        EditorGUI.DrawRect(new Rect(swatch.x + 3f, swatch.y + 3f, swatch.width - 6f, swatch.height - 6f), color);
                    }
                }

                EditorGUILayout.HelpBox("Changes affect this bookmark only. The asset name and path stay unchanged.", MessageType.Info);
                GUILayout.FlexibleSpace();

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Reset"))
                    {
                        m_suffix = string.Empty;
                        m_useColor = false;
                        m_color = Palette[4];
                        GUI.FocusControl(null);
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Cancel"))
                    {
                        Close();
                    }
                    if (GUILayout.Button("Save"))
                    {
                        if (!m_favorites.SetAppearance(m_asset, m_suffix, m_useColor, m_color))
                        {
                            EditorUtility.DisplayDialog("Bookmark removed", "This bookmark or its group was removed. No changes were saved.", "OK");
                        }
                        if (m_owner != null)
                        {
                            m_owner.Repaint();
                        }
                        Close();
                    }
                }
                EditorGUILayout.Space();
            }
        }
    }
}
#endif
