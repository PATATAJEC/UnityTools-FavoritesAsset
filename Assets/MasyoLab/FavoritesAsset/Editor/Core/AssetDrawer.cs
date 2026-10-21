#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

//=========================================================
//
//  developer : MasyoLab
//  github    : https://github.com/MasyoLab/UnityTools-FavoritesAsset
//
//=========================================================

namespace MasyoLab.Editor.FavoritesAsset
{
    struct AssetDrawer
    {
        private static PtrLinker<GUIStyle> BUTTON_STYLE = new PtrLinker<GUIStyle>(() =>
        {
            return new GUIStyle(GUI.skin.button);
        });

        /// <summary>
        /// アセットの情報を描画
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="info"></param>
        /// <param name="onAction"></param>
        private static void DrawingSetting(IPipeline pipeline, AssetData info, UnityAction<GUIContent, GUIStyle> onAction = null)
        {
            GUIContent content = null;
            Texture assetIcon = null;

            if (info.Kind == BookmarkKind.MenuCommand)
            {
                assetIcon = EditorGUIUtility.IconContent("PlayButton").image;
                content = new GUIContent(info.DisplayName, assetIcon, info.MenuPath + "\nRun menu command using the current Unity selection");
            }
            else
            {
                assetIcon = FavoritesAssetWindow.GetFavoritesAssetIcon?.Invoke(info.Path);
                if (assetIcon == null)
                {
                    assetIcon = AssetDatabase.GetCachedIcon(info.Path);
                }
                if (assetIcon == null)
                {
                    assetIcon = EditorGUIUtility.IconContent(CONST.ICON_ERRORICON).image;
                    content = new GUIContent("(missing asset) " + info.DisplayName, assetIcon, info.Path);
                }
                else
                {
                    content = new GUIContent(info.DisplayName, assetIcon, info.Path);
                }
            }

            var style = BUTTON_STYLE.Inst;
            var originalAlignment = style.alignment;
            var originalFontStyle = style.fontStyle;
            var originalTextColor = style.normal.textColor;

            style.alignment = TextAnchor.MiddleLeft;
            onAction?.Invoke(content, style);
            style.alignment = originalAlignment;
            style.fontStyle = originalFontStyle;
            style.normal.textColor = originalTextColor;
        }

        /// <summary>
        /// アセットを開くボタン
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="rect"></param>
        /// <param name="data"></param>
        /// <param name="onButtonAction"></param>
        public static void OnAssetButton(IPipeline pipeline, Rect rect, AssetData data, UnityAction<AssetData> onButtonAction = null)
        {
            DrawingSetting(pipeline, data, (content, style) =>
            {
                var originalBackground = GUI.backgroundColor;
                try
                {
                    if (data.UseCustomColor)
                    {
                        EditorGUI.DrawRect(new Rect(rect.x, rect.y + 2f, 4f, rect.height - 4f), data.BookmarkColor);
                        rect.x += 6f;
                        rect.width = Mathf.Max(0f, rect.width - 6f);
                        GUI.backgroundColor = originalBackground * Color.Lerp(Color.white, data.BookmarkColor, 0.5f);
                    }

                    if (GUI.Button(rect, content, style))
                    {
                        onButtonAction?.Invoke(data);
                    }
                }
                finally
                {
                    GUI.backgroundColor = originalBackground;
                }
            });
        }

        public static bool OnEditButton(Rect rect)
        {
            var icon = EditorGUIUtility.IconContent(CONST.ICON_EDIT);
            var content = new GUIContent(icon.image, "Edit bookmark settings");
            return GUI.Button(rect, content);
        }

        /// <summary>
        /// アセットをPingする
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="data"></param>
        public static void OnPingObjectButton(Rect rect, AssetData data)
        {
            // アイコンを指定
            var content = EditorGUIUtility.IconContent(CONST.ICON_ANIMATION_VISIBILITY_TOGGLE_ON);
            if (GUI.Button(rect, content))
            {
                // アセットの情報
                var asset = data.GetObject();
                var oldActiveObject = Selection.activeObject;
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = oldActiveObject;
            }
        }

        /// <summary>
        /// お気に入り解除
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="data"></param>
        /// <param name="onButtonAction"></param>
        /// <returns></returns>
        public static bool OnUnfavoriteButton(Rect rect, AssetData data, UnityAction<AssetData> onButtonAction = null)
        {
            // アイコンを指定
            var content = EditorGUIUtility.IconContent(CONST.ICON_CLOSE);
            if (GUI.Button(rect, content))
            {
                onButtonAction?.Invoke(data);
                return true;
            }
            return false;
        }
    }
}
#endif
