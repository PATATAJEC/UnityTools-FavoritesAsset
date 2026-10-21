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
            return new AssetData(Guid, LocalId)
            {
                Index = Index,
                LabelSuffix = LabelSuffix,
                UseCustomColor = UseCustomColor,
                BookmarkColor = BookmarkColor,
            };
        }

        public Object GetObject()
        {
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
