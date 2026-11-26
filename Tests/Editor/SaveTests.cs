using System;
using System.Collections.Generic;
using System.IO;
using ArchNet.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArchNet.Module.Tests
{
    [Serializable]
    public class TestData
    {
        public int coins;
        public List<string> items = new List<string>();
    }

    public class TestSave : SaveService<TestData>
    {
        public int MigratedFrom = -1;
        protected override void OnNewGame(TestData data) => data.coins = 5;
        protected override void Migrate(int fromVersion, TestData data) => MigratedFrom = fromVersion;
    }

    public class SaveServiceTests
    {
        GameObject _object;
        TestSave _save;
        string _folder;

        [SetUp]
        public void SetUp()
        {
            _folder = "archnet-tests/" + Guid.NewGuid().ToString("N");
            _object = new GameObject("Save");
            _save = _object.AddComponent<TestSave>();
            _save.Configure(_folder, 2, 1);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_object);
            string folder = JsonStorage.FullPath(_folder);
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
        }

        [Test]
        public void SavesAndLoadsASlot()
        {
            _save.NewGame(1);
            Assert.AreEqual(5, _save.Current.coins, "OnNewGame fills new data");
            _save.Current.items.Add("sword");
            _save.Save();

            _save.NewGame(0);
            Assert.IsTrue(_save.Load(1));
            Assert.AreEqual(1, _save.CurrentSlot);
            Assert.AreEqual("sword", _save.Current.items[0]);
            Assert.IsTrue(_save.GetSlot(1).Exists);
            Assert.IsFalse(_save.GetSlot(0).Exists);
        }

        [Test]
        public void AnEmptySlotDoesNotLoad()
        {
            Assert.IsFalse(_save.Load(0));
            Assert.AreEqual(-1, _save.CurrentSlot);
        }

        [Test]
        public void OlderSavesGoThroughMigrate()
        {
            _save.NewGame(0);
            _save.Save();
            _save.Configure(_folder, 2, 3);

            Assert.IsTrue(_save.Load(0));
            Assert.AreEqual(1, _save.MigratedFrom);
        }

        [Test]
        public void SaveNeedsASlotFirst()
        {
            Assert.Throws<InvalidOperationException>(() => _save.Save());
            Assert.Throws<ArgumentOutOfRangeException>(() => _save.NewGame(2));
        }

        [Test]
        public void DeleteEmptiesTheSlot()
        {
            _save.NewGame(0);
            _save.Save();
            _save.Delete(0);

            Assert.IsFalse(_save.HasSave(0));
            Assert.AreEqual(-1, _save.CurrentSlot);
        }
    }
}
