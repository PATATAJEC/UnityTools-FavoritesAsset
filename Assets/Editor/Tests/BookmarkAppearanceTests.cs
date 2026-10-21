using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MasyoLab.Editor.FavoritesAsset;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MasyoLab.FavoritesAsset.Tests.Editor
{
    public class BookmarkAppearanceTests
    {
        private string m_folder;
        private readonly List<string> m_saveFiles = new List<string>();

        private class TestPipeline : IPipeline
        {
            public FavoritesManager Favorites { get; private set; }
            public GroupManager Group { get; private set; }
            public SettingManager Setting => null;
            public EditorWindow Root => null;
            public Rect WindowSize => Rect.zero;

            public TestPipeline()
            {
                Group = new GroupManager(this);
                Favorites = new FavoritesManager(this);
            }
        }

        [SetUp]
        public void SetUp()
        {
            var name = "__BookmarkAppearanceTests_" + System.Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", name);
            m_folder = "Assets/" + name;
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(m_folder);
            foreach (var file in m_saveFiles)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            m_saveFiles.Clear();
        }

        private AssetData CreateBookmark(bool prefab = false)
        {
            string path;
            if (prefab)
            {
                path = m_folder + "/Original.prefab";
                var gameObject = new GameObject("Original");
                try
                {
                    PrefabUtility.SaveAsPrefabAsset(gameObject, path);
                }
                finally
                {
                    Object.DestroyImmediate(gameObject);
                }
            }
            else
            {
                AssetDatabase.CreateFolder(m_folder, "Original");
                path = m_folder + "/Original";
            }
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long localId));
            return new AssetData(guid, localId);
        }

        // Inject isolated, uniquely named groups so tests never overwrite user bookmarks.
        private string AddGroup(FavoritesManager manager, AssetDB db)
        {
            var key = "BookmarkAppearanceTest_" + System.Guid.NewGuid().ToString("N");
            var field = typeof(FavoritesManager).GetField("m_assetDBDict", BindingFlags.NonPublic | BindingFlags.Instance);
            var groups = (Dictionary<string, PtrLinker<AssetDB>>)field.GetValue(manager);
            groups.Add(key, new PtrLinker<AssetDB>(() => db));
            m_saveFiles.Add(SaveLoad.GetSaveDataPath(key));
            return key;
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void EmptySuffix_ShowsOriginalName(string suffix)
        {
            var asset = CreateBookmark();
            asset.LabelSuffix = suffix;
            Assert.AreEqual("Original", asset.DisplayName);
        }

        [Test]
        public void LegacyJson_LoadsWithoutSuffixOrCustomColor()
        {
            var asset = CreateBookmark();
            var json = "{\"AssetDB\":{\"Ref\":[{\"Guid\":\"" + asset.Guid + "\",\"LocalId\":" + asset.LocalId + "}]}}";
            var loaded = FavoritesJson.FromJson(json).AssetDB.Ref[0];
            loaded.UpdateData();
            Assert.AreEqual("Original", loaded.DisplayName);
            Assert.IsFalse(loaded.UseCustomColor);
            Assert.AreEqual(asset.GetObject(), loaded.GetObject());
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RenameAndMove_KeepSuffixColorAndAssetReference(bool prefab)
        {
            var asset = CreateBookmark(prefab);
            asset.LabelSuffix = "Postać żółta";
            asset.UseCustomColor = true;
            asset.BookmarkColor = Color.cyan;
            var guid = asset.Guid;
            Assert.AreEqual(string.Empty, AssetDatabase.RenameAsset(asset.Path, "Renamed"));
            asset.UpdateData();
            var destination = m_folder + "/Moved" + (prefab ? ".prefab" : string.Empty);
            Assert.AreEqual(string.Empty, AssetDatabase.MoveAsset(asset.Path, destination));
            asset.UpdateData();
            Assert.AreEqual("Moved — Postać żółta", asset.DisplayName);
            Assert.AreEqual(guid, asset.Guid);
            Assert.AreEqual(destination, asset.Path);
            Assert.IsTrue(asset.UseCustomColor);
            Assert.AreEqual(Color.cyan, asset.BookmarkColor);
            Assert.IsNotNull(asset.GetObject());
        }

        [Test]
        public void ExportImport_PreservesAppearanceInDefaultAndNamedGroups()
        {
            var asset = CreateBookmark();
            asset.LabelSuffix = "Characters / główne";
            asset.UseCustomColor = true;
            asset.BookmarkColor = new Color(0.2f, 0.6f, 0.9f, 1f);
            var db = new AssetDB();
            db.Ref.Add(asset);
            var group = new GroupData { GroupName = "Materials" };
            var groups = new GroupDB();
            groups.Data.Add(group);
            var groupDb = new AssetDB { Guid = group.GUID };
            groupDb.Ref.Add(asset.Copy());

            var exported = FavoritesJsonExportData.ToJson(db, groups, new List<AssetDB> { groupDb });
            var imported = FavoritesJsonExportData.FromJson(exported);
            foreach (var bookmark in new[] { imported.AssetDB.Ref[0], imported.GroupData[0].Ref[0] })
            {
                bookmark.UpdateData();
                Assert.AreEqual(asset.DisplayName, bookmark.DisplayName);
                Assert.AreEqual(asset.Guid, bookmark.Guid);
                Assert.AreEqual(asset.LocalId, bookmark.LocalId);
                Assert.IsTrue(bookmark.UseCustomColor);
                Assert.AreEqual(asset.BookmarkColor, bookmark.BookmarkColor);
            }
        }

        [Test]
        public void SaveAppearance_TargetsOwningGroupAndLeavesAssetNameUnchanged()
        {
            var asset = CreateBookmark();
            var originalPath = asset.Path;
            var pipeline = new TestPipeline();
            var db = new AssetDB();
            db.Ref.Add(asset);
            var key = AddGroup(pipeline.Favorites, db);
            AddGroup(pipeline.Favorites, new AssetDB());

            Assert.IsTrue(pipeline.Favorites.SetAppearance(asset, "  Postać  ", true, new Color(1f, 0f, 0f, 0f)));
            var saved = FavoritesJson.FromJson(SaveLoad.Load(SaveLoad.GetSaveDataPath(key))).AssetDB.Ref[0];
            saved.UpdateData();
            Assert.AreEqual("Original — Postać", saved.DisplayName);
            Assert.AreEqual(originalPath, saved.Path);
            Assert.AreEqual("Original", saved.GetObject().name);
            Assert.AreEqual(Color.red, saved.BookmarkColor);
            Assert.IsTrue(saved.UseCustomColor);

            Assert.IsTrue(pipeline.Favorites.SetAppearance(asset, string.Empty, false, Color.white));
            saved = FavoritesJson.FromJson(SaveLoad.Load(SaveLoad.GetSaveDataPath(key))).AssetDB.Ref[0];
            saved.UpdateData();
            Assert.AreEqual("Original", saved.DisplayName);
            Assert.IsFalse(saved.UseCustomColor);
        }

        [Test]
        public void RemovedBookmark_DoesNotGetRecreatedByAppearanceEditor()
        {
            var asset = CreateBookmark();
            var pipeline = new TestPipeline();
            var db = new AssetDB();
            db.Ref.Add(asset);
            var key = AddGroup(pipeline.Favorites, db);
            db.Ref.Clear();
            Assert.IsFalse(pipeline.Favorites.SetAppearance(asset, "Removed", true, Color.red));
            Assert.IsFalse(File.Exists(SaveLoad.GetSaveDataPath(key)));
            Assert.IsTrue(string.IsNullOrEmpty(asset.LabelSuffix));
        }

        [Test]
        public void CopyGroup_ProducesIndependentAppearance()
        {
            var asset = CreateBookmark();
            asset.LabelSuffix = "Source";
            asset.UseCustomColor = true;
            asset.BookmarkColor = Color.cyan;
            var source = new AssetDB();
            source.Ref.Add(asset);
            var target = new AssetDB();
            var pipeline = new TestPipeline();
            var sourceKey = AddGroup(pipeline.Favorites, source);
            var targetKey = AddGroup(pipeline.Favorites, target);

            pipeline.Favorites.ReplicationFavoriteData(sourceKey, targetKey);
            var copy = target.Ref[0];
            Assert.AreNotSame(asset, copy);
            Assert.AreEqual(asset.Guid, copy.Guid);
            Assert.AreEqual(asset.LocalId, copy.LocalId);
            Assert.AreEqual(asset.LabelSuffix, copy.LabelSuffix);
            Assert.AreEqual(asset.BookmarkColor, copy.BookmarkColor);
            Assert.IsTrue(copy.UseCustomColor);

            Assert.IsTrue(pipeline.Favorites.SetAppearance(copy, "Copy", true, Color.red));
            Assert.AreEqual("Source", asset.LabelSuffix);
            Assert.AreEqual(Color.cyan, asset.BookmarkColor);
        }

        [Test]
        public void EditIcon_IsAvailableInUnity()
        {
            Assert.IsNotNull(EditorGUIUtility.IconContent(CONST.ICON_EDIT).image);
        }
    }
}
