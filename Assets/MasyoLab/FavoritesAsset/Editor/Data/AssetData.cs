#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//=========================================================
//
//  developer : MasyoLab
//  github    : https://github.com/MasyoLab/UnityTools-FavoritesAsset
//
//=========================================================

namespace MasyoLab.Editor.FavoritesAsset
{
    enum BookmarkKind
    {
        Asset = 0,
        MenuCommand = 1,
        Separator = 2,
    }

    /// <summary>
    /// アセットリスト
    /// </summary>
    [System.Serializable]
    class AssetData
    {
        /// <summary>
        /// アセットのGUID
        /// </summary>
        public string Guid = string.Empty;
        /// <summary>
        /// アセットパス
        /// </summary>
        public string Path { get; private set; }
        /// <summary>
        /// アセット名
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// LocalID (fileID)
        /// </summary>
        public long LocalId = 0;

        // Zero remains an asset so existing save files keep their original meaning.
        public BookmarkKind Kind = BookmarkKind.Asset;
        public string MenuPath = string.Empty;

        // Bookmark appearance is independent of the referenced asset and its name.
        public string LabelSuffix = string.Empty;
        public bool UseCustomColor = false;
        public Color BookmarkColor = Color.white;

        public string DisplayName => string.IsNullOrWhiteSpace(LabelSuffix)
            ? Name
            : $"{Name} — {LabelSuffix.Trim()}";

        public int Index { get; set; }

        public AssetData(string guid, long localId)
        {
            Guid = guid;
            LocalId = localId;
            UpdateData();
        }

        public AssetData Copy()
        {
            var copy = new AssetData(Guid, LocalId)
            {
                Kind = Kind,
                MenuPath = MenuPath,
                Index = Index,
                LabelSuffix = LabelSuffix,
                UseCustomColor = UseCustomColor,
                BookmarkColor = BookmarkColor,
            };
            copy.UpdateData();
            return copy;
        }

        public static bool IsValidMenuPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(new[] { '\r', '\n', '\\' }) >= 0)
            {
                return false;
            }
            var parts = path.Trim().Split('/');
            return parts.Length >= 2 && System.Array.TrueForAll(parts, part => !string.IsNullOrWhiteSpace(part));
        }

        public static string GetMenuCommandName(string path)
        {
            path = (path ?? string.Empty).Trim();
            return path.Substring(path.LastIndexOf('/') + 1);
        }

        public static AssetData CreateMenuCommand(string path)
        {
            if (!IsValidMenuPath(path))
            {
                throw new System.ArgumentException("Enter a full menu path, for example Assets/Show in Explorer.", nameof(path));
            }
            var entry = new AssetData(string.Empty, 0)
            {
                Kind = BookmarkKind.MenuCommand,
                MenuPath = path.Trim(),
            };
            entry.UpdateData();
            return entry;
        }

        public static AssetData CreateSeparator()
        {
            return new AssetData(string.Empty, 0) { Kind = BookmarkKind.Separator };
        }

        public bool TryExecuteMenuCommand(out string error)
        {
            if (Kind != BookmarkKind.MenuCommand || !IsValidMenuPath(MenuPath))
            {
                error = "This bookmark does not contain a valid menu command.";
                return false;
            }
            if (!Menu.GetEnabled(MenuPath))
            {
                error = $"Menu command not found or currently unavailable:\n{MenuPath}\nCheck the path and the current selection in Unity.";
                return false;
            }
            if (!EditorApplication.ExecuteMenuItem(MenuPath))
            {
                error = $"Could not execute menu command:\n{MenuPath}";
                return false;
            }
            error = string.Empty;
            return true;
        }

        public Object GetObject()
        {
            if (Kind != BookmarkKind.Asset || string.IsNullOrEmpty(Guid))
            {
                return null;
            }
            // GUIDでパスを取得
            var assetPath = AssetDatabase.GUIDToAssetPath(Guid);
            if (assetPath == string.Empty)
            {
                return null;
            }

            // アセットを取得
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset == null)
            {
                return null;
            }

            // 登録したデータが SubAssets
            var assetDatas = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
            if (assetDatas.Length != 0)
            {
                foreach (var obj in assetDatas)
                {
                    // SubAssets なら localID で識別
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long localid))
                    {
                        if (LocalId != localid)
                        {
                            continue;
                        }
                        asset = obj;
                        break;
                    }
                }
            }
            return asset;
        }

        /// <summary>
        /// データを更新
        /// </summary>
        public void UpdateData()
        {
            if (Kind != BookmarkKind.Asset)
            {
                Path = Kind == BookmarkKind.MenuCommand ? MenuPath ?? string.Empty : string.Empty;
                Name = Kind == BookmarkKind.MenuCommand ? GetMenuCommandName(MenuPath) : string.Empty;
                return;
            }
            var asset = GetObject();
            Path = asset == null ? string.Empty : AssetDatabase.GUIDToAssetPath(Guid);
            Name = asset == null ? string.Empty : asset.name;
        }
    }

    [System.Serializable]
    class AssetDB
    {
        public string Guid = string.Empty;
        public List<AssetData> Ref = new List<AssetData>();
    }
}
#endif
