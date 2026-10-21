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
    /// お気に入りマネージャー
    /// </summary>
    class FavoritesManager : BaseManager
    {
        private Dictionary<string, PtrLinker<AssetDB>> m_assetDBDict = new Dictionary<string, PtrLinker<AssetDB>>();
        private PtrLinker<AssetDB> m_assetDB => GetSelectFavoritesData();
        public IReadOnlyList<AssetData> Data => m_assetDB.Inst.Ref;
        public AssetDB AssetInfoList => GetSelectFavoritesData(CONST.FAVORITES_DATA).Inst;
        public string SelectedGroupFileName => m_pipeline.Group.SelectGroupFileName;

        public FavoritesManager(IPipeline pipeline) : base(pipeline)
        {
            // グループ削除時の処理を追加
            pipeline.Group.RemoveEvent = (guid) =>
            {
                m_assetDBDict.Remove(guid);
                SaveLoad.Save("{}", SaveLoad.GetSaveDataPath(guid));
            };
        }

        public void Add(string guid, long localId)
        {
            m_assetDB.Inst.Ref.Add(new AssetData(guid, localId));
        }

        public bool AddEntry(AssetData entry, string groupFileName)
        {
            // A utility editor may remain open after switching or removing its target group.
            if (groupFileName != CONST.FAVORITES_DATA &&
                !m_pipeline.Group.GroupDB.Data.Exists(group => group.GUID == groupFileName && !group.IsNull))
            {
                return false;
            }
            if (entry == null || (entry.Kind == BookmarkKind.MenuCommand && !AssetData.IsValidMenuPath(entry.MenuPath)))
            {
                return false;
            }
            var db = GetSelectFavoritesData(groupFileName).Inst;
            entry.UpdateData();
            db.Ref.Add(entry);
            SaveLoad.Save(FavoritesJson.ToJson(db), SaveLoad.GetSaveDataPath(groupFileName));
            return true;
        }

        public void Remove(AssetData info)
        {
            m_assetDB.Inst.Ref.Remove(info);
        }

        public void RemoveAll()
        {
            m_assetDB.Inst.Ref.RemoveRange(0, m_assetDB.Inst.Ref.Count);
        }

        public bool ExistsGUID(string guid, long localId)
        {
            return m_assetDB.Inst.Ref.Exists(x => x.Kind == BookmarkKind.Asset && x.Guid == guid && x.LocalId == localId);
        }

        public void SaveFavoritesData()
        {
            SaveLoad.Save(FavoritesJson.ToJson(m_assetDB.Inst), SaveLoad.GetSaveDataPath(m_pipeline.Group.SelectGroupFileName));
        }

        public bool SetAppearance(AssetData asset, string labelSuffix, bool useCustomColor, Color color, string menuPath = null)
        {
            if (asset.Kind == BookmarkKind.MenuCommand && !AssetData.IsValidMenuPath(menuPath ?? asset.MenuPath))
            {
                return false;
            }
            // The user can switch groups while the appearance editor is open.
            // Save the group that owns this entry, rather than the currently selected group.
            foreach (var entry in m_assetDBDict)
            {
                if (!entry.Value.Inst.Ref.Contains(asset))
                {
                    continue;
                }

                asset.LabelSuffix = labelSuffix == null ? string.Empty : labelSuffix.Trim();
                asset.UseCustomColor = useCustomColor;
                asset.BookmarkColor = new Color(color.r, color.g, color.b, 1f);
                if (asset.Kind == BookmarkKind.MenuCommand)
                {
                    asset.MenuPath = (menuPath ?? asset.MenuPath).Trim();
                }
                asset.UpdateData();
                SaveLoad.Save(FavoritesJson.ToJson(entry.Value.Inst), SaveLoad.GetSaveDataPath(entry.Key));
                return true;
            }

            // The entry or its group may have been removed while editing.
            return false;
        }

        public void SaveAll()
        {
            foreach (var item in m_pipeline.Group.GroupDB.Data)
            {
                var ptrLinker = GetSelectFavoritesData(item.GUID);
                var fileName = m_pipeline.Group.GetGroupFileName(item.GUID);
                SaveLoad.Save(FavoritesJson.ToJson(ptrLinker.Inst), SaveLoad.GetSaveDataPath(fileName));
            }
        }

        /// <summary>
        /// お気に入り登録したアセットを更新
        /// </summary>
        public void CheckFavoritesAsset()
        {
            foreach (var item in m_assetDB.Inst.Ref)
            {
                item.UpdateData();
            }
        }

        /// <summary>
        /// お気に入りデータを読み込む
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static AssetDB LoadFavoritesData(string fileName)
        {
            string jsonData = SaveLoad.Load(SaveLoad.GetSaveDataPath(fileName));
            string guid = fileName == CONST.FAVORITES_DATA ? string.Empty : fileName;
            AssetDB data = null;

            // json から読み込む
            var assets = JsonUtility.FromJson<AssetDB>(jsonData);
            if (assets == null)
            {
                data = new AssetDB();
                data.Guid = guid;
                return data;
            }

            data = FavoritesJson.FromJson(jsonData).AssetDB;
            data.Guid = guid;
            data.Ref.ForEach(v => v.UpdateData());
            return data;
        }

        /// <summary>
        /// ソートデータを受け取る
        /// </summary>
        /// <param name="assetInfos"></param>
        public void SortData(IReadOnlyList<AssetData> assetInfos)
        {
            for (int i = 0; i < assetInfos.Count; i++)
            {
                assetInfos[i].Index = i;
            }

            m_assetDB.Inst.Ref.Sort((itemA, itemB) => itemA.Index - itemB.Index);
            SaveFavoritesData();
        }

        /// <summary>
        /// インポート
        /// </summary>
        /// <param name="importData"></param>
        public void SetImportData(FavoritesJsonExportData importData)
        {
            if (importData == null)
            {
                return;
            }

            m_assetDBDict.Clear();
            m_assetDBDict.Add(CONST.FAVORITES_DATA, new PtrLinker<AssetDB>(() =>
            {
                return importData.AssetDB;
            }));

            foreach (var item in importData.GroupData)
            {
                m_assetDBDict.Add(item.Guid, new PtrLinker<AssetDB>(() =>
                {
                    return item;
                }));
            }

            foreach (var item in m_assetDBDict)
            {
                SaveLoad.Save(FavoritesJson.ToJson(item.Value.Inst), SaveLoad.GetSaveDataPath(item.Key));
            }
        }

        /// <summary>
        /// お気に入りデータを取得
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private PtrLinker<AssetDB> GetSelectFavoritesData(string guid)
        {
            if (m_assetDBDict.ContainsKey(guid))
            {
                return m_assetDBDict[guid];
            }

            var favData = new PtrLinker<AssetDB>(() =>
            {
                return LoadFavoritesData(guid);
            });

            m_assetDBDict.Add(guid, favData);
            return favData;
        }

        /// <summary>
        /// お気に入りデータを切り替える
        /// </summary>
        /// <returns></returns>
        private PtrLinker<AssetDB> GetSelectFavoritesData()
        {
            return GetSelectFavoritesData(m_pipeline.Group.SelectGroupFileName);
        }

        /// <summary>
        /// お気に入りデータを取得
        /// </summary>
        /// <returns></returns>
        public List<AssetDB> GetFavoriteList()
        {
            var returnData = new List<AssetDB>(m_pipeline.Group.GroupDB.Data.Count);
            foreach (var item in m_pipeline.Group.GroupDB.Data)
            {
                returnData.Add(GetSelectFavoritesData(item.GUID).Inst);
            }
            return returnData;
        }

        /// <summary>
        /// お気に入りデータを複製
        /// </summary>
        /// <param name="baseGUID"></param>
        /// <param name="targetGUID"></param>
        public void ReplicationFavoriteData(string baseGUID, string targetGUID)
        {
            var baseLinker = GetSelectFavoritesData(baseGUID);
            var targetLinker = GetSelectFavoritesData(targetGUID);
            targetLinker.Inst.Ref.Clear();
            // Copies must keep their own suffix and color when edited later.
            targetLinker.Inst.Ref.AddRange(baseLinker.Inst.Ref.ConvertAll(asset => asset.Copy()));
        }
    }
}
#endif
