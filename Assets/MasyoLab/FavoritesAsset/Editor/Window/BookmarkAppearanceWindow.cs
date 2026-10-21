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
        private bool m_creating;
        private string m_groupFileName;
        private string m_menuPath;

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

        public static BookmarkAppearanceWindow Show(AssetData asset, FavoritesManager favorites, EditorWindow owner)
        {
            var window = CreateInstance<BookmarkAppearanceWindow>();
            window.titleContent = new GUIContent(asset.Kind == BookmarkKind.MenuCommand
                ? "Edit menu command" : asset.Kind == BookmarkKind.Separator ? "Edit separator" : "Edit bookmark");
            window.m_asset = asset;
            window.m_favorites = favorites;
            window.m_owner = owner;
            window.m_suffix = asset.LabelSuffix ?? string.Empty;
            window.m_menuPath = asset.MenuPath ?? string.Empty;
            window.m_useColor = asset.UseCustomColor;
            window.m_color = asset.UseCustomColor ? asset.BookmarkColor : Palette[4];
            window.m_focusSuffix = true;
            window.minSize = window.maxSize = new Vector2(400f, asset.Kind == BookmarkKind.MenuCommand ? 350f : 290f);
            // A utility window stays open when Unity's color picker takes focus.
            window.ShowUtility();
            return window;
        }

        public static void ShowNewMenuCommand(FavoritesManager favorites, EditorWindow owner, string groupFileName)
        {
            var draft = new AssetData(string.Empty, 0) { Kind = BookmarkKind.MenuCommand };
            var window = Show(draft, favorites, owner);
            window.titleContent = new GUIContent("Add menu command");
            window.m_creating = true;
            window.m_groupFileName = groupFileName;
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
                var isCommand = m_asset.Kind == BookmarkKind.MenuCommand;
                var isSeparator = m_asset.Kind == BookmarkKind.Separator;
                if (isCommand)
                {
                    GUI.SetNextControlName("MenuCommandPath");
                    m_menuPath = EditorGUILayout.TextField("Menu path", m_menuPath);
                    EditorGUILayout.HelpBox("Enter the exact path from Unity's menu, e.g. Assets/Show in Explorer. Saving does not run the command.", MessageType.Info);
                }
                else if (isSeparator)
                {
                    EditorGUILayout.LabelField("Separator", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("An empty spacer with an optional caption and color.", EditorStyles.miniLabel);
                }
                else
                {
                    EditorGUILayout.LabelField("Original asset", m_asset.Name);
                    EditorGUILayout.LabelField(new GUIContent(m_asset.Path, m_asset.Path), EditorStyles.miniLabel);
                }
                EditorGUILayout.Space();

                GUI.SetNextControlName("BookmarkSuffix");
                m_suffix = EditorGUILayout.TextField(isSeparator ? "Caption (optional)" : "Name suffix", m_suffix);
                if (m_focusSuffix && ev.type == EventType.Repaint)
                {
                    EditorGUI.FocusTextInControl(m_creating ? "MenuCommandPath" : "BookmarkSuffix");
                    m_focusSuffix = false;
                }

                var originalName = isCommand ? AssetData.GetMenuCommandName(m_menuPath) : m_asset.Name;
                var preview = isSeparator ? (m_suffix ?? string.Empty).Trim()
                    : string.IsNullOrWhiteSpace(m_suffix) ? originalName : $"{originalName} — {m_suffix.Trim()}";
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

                EditorGUILayout.HelpBox(isCommand
                    ? "Click the saved bookmark to run the command. Commands use the current selection in Unity."
                    : isSeparator ? "Changes affect this separator only. Drag its handle to reposition it."
                    : "Changes affect this bookmark only. The asset name and path stay unchanged.", MessageType.Info);
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
                    using (new EditorGUI.DisabledScope(isCommand && !AssetData.IsValidMenuPath(m_menuPath)))
                    {
                        if (GUILayout.Button("Save"))
                        {
                            bool saved;
                            if (m_creating)
                            {
                                var entry = AssetData.CreateMenuCommand(m_menuPath);
                                entry.LabelSuffix = (m_suffix ?? string.Empty).Trim();
                                entry.UseCustomColor = m_useColor;
                                entry.BookmarkColor = new Color(m_color.r, m_color.g, m_color.b, 1f);
                                saved = m_favorites.AddEntry(entry, m_groupFileName);
                            }
                            else
                            {
                                saved = m_favorites.SetAppearance(m_asset, m_suffix, m_useColor, m_color, m_menuPath);
                            }
                            if (!saved)
                            {
                                EditorUtility.DisplayDialog("Bookmark unavailable", "This bookmark or its group was removed. No changes were saved.", "OK");
                            }
                            if (m_owner != null)
                            {
                                m_owner.Repaint();
                            }
                            Close();
                        }
                    }
                }
                EditorGUILayout.Space();
            }
        }
    }
}
#endif
