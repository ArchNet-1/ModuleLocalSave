// ArchNet · Local Save Module
// Save slots for your game data (any [Serializable] class) as JSON files under
// persistentDataPath/saves/. The Hub creates GameSaveData (what is saved) and GameSave (the
// service) in your project:
//
//   var save = Services.Get<GameSave>();
//   save.NewGame(0);              // or: if (!save.Load(0)) save.NewGame(0);
//   save.Current.coins += 10;
//   save.Save();                  // also done for you when the game pauses or quits
//
// Files are written safely (temporary file + previous version kept), and each save records
// a version number: raise it when GameSaveData changes and upgrade old saves in Migrate().
// Docs: https://archnet-plateform.fr/#ModuleLocalSave

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using ArchNet.Core;
using UnityEngine;

[assembly: InternalsVisibleTo("ArchNet.Module.LocalSave.Tests")]

namespace ArchNet.Module
{
    /// <summary>What a save file holds: the data plus when and with which version it was written.</summary>
    [Serializable]
    public class SaveFile<T>
    {
        public int version;
        public string savedAt;
        public T data;
    }

    public readonly struct SaveSlotInfo
    {
        public readonly int Slot;
        public readonly bool Exists;
        public readonly DateTime SavedAt; // local time; default when the slot is empty

        public SaveSlotInfo(int slot, bool exists, DateTime savedAt)
        {
            Slot = slot;
            Exists = exists;
            SavedAt = savedAt;
        }
    }

    public abstract class SaveService<T> : GameService where T : class, new()
    {
        [SerializeField, Min(1)] int _slots = 3;
        [Tooltip("Raise it when you change the saved data; older saves then go through Migrate().")]
        [SerializeField, Min(1)] int _version = 1;
        [Tooltip("Save the current slot when the game is paused (mobile) or closed.")]
        [SerializeField] bool _autoSave = true;
        [Tooltip("Folder under Application.persistentDataPath.")]
        [SerializeField] string _folder = "saves";

        /// <summary>The data being played. Change it freely, then Save().</summary>
        public T Current { get; private set; } = new T();

        /// <summary>The slot being played, or -1 before NewGame / Load.</summary>
        public int CurrentSlot { get; private set; } = -1;

        public int SlotCount => _slots;
        public int Version => _version;

        public event Action<int> Saved;
        public event Action<int> Loaded;

        internal void Configure(string folder, int slots, int version)
        {
            _folder = folder;
            _slots = slots;
            _version = version;
        }

        /// <summary>Starts fresh data in a slot (nothing is written until Save).</summary>
        public void NewGame(int slot)
        {
            Check(slot);
            Current = new T();
            CurrentSlot = slot;
            OnNewGame(Current);
        }

        /// <summary>Loads a slot. Returns false when it is empty or unreadable.</summary>
        public bool Load(int slot)
        {
            Check(slot);
            if (!JsonStorage.TryLoad(PathOf(slot), out SaveFile<T> file) || file.data == null) return false;
            if (file.version < _version) Migrate(file.version, file.data);
            Current = file.data;
            CurrentSlot = slot;
            Loaded?.Invoke(slot);
            return true;
        }

        /// <summary>Writes the current data to the current slot.</summary>
        public void Save()
        {
            if (CurrentSlot < 0) throw new InvalidOperationException("Call NewGame(slot) or Load(slot) before Save().");
            Save(CurrentSlot);
        }

        /// <summary>Writes the current data to a slot, which becomes the current one.</summary>
        public void Save(int slot)
        {
            Check(slot);
            OnBeforeSave(Current);
            JsonStorage.Save(PathOf(slot), new SaveFile<T>
            {
                version = _version,
                savedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                data = Current,
            });
            CurrentSlot = slot;
            Saved?.Invoke(slot);
        }

        public bool HasSave(int slot)
        {
            Check(slot);
            return JsonStorage.Exists(PathOf(slot));
        }

        public SaveSlotInfo GetSlot(int slot)
        {
            Check(slot);
            if (!JsonStorage.TryLoad(PathOf(slot), out SaveFile<T> file) || file == null) return new SaveSlotInfo(slot, false, default);
            DateTime.TryParse(file.savedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime savedAt);
            return new SaveSlotInfo(slot, true, savedAt.ToLocalTime());
        }

        public void Delete(int slot)
        {
            Check(slot);
            JsonStorage.Delete(PathOf(slot));
            if (CurrentSlot == slot) CurrentSlot = -1;
        }

        /// <summary>Fills new data (starting level, items…).</summary>
        protected virtual void OnNewGame(T data) { }

        /// <summary>Last chance to update the data before it is written (play time…).</summary>
        protected virtual void OnBeforeSave(T data) { }

        /// <summary>Upgrades data saved by an older version. Called before Load returns.</summary>
        protected virtual void Migrate(int fromVersion, T data) { }

        void OnApplicationPause(bool paused)
        {
            if (paused) AutoSave();
        }

        void OnApplicationQuit() => AutoSave();

        void AutoSave()
        {
            if (!_autoSave || CurrentSlot < 0) return;
            try { Save(); }
            catch (Exception exception) { Debug.LogException(exception, this); }
        }

        string PathOf(int slot) => $"{_folder}/slot{slot}.json";

        void Check(int slot)
        {
            if (slot < 0 || slot >= _slots)
                throw new ArgumentOutOfRangeException(nameof(slot), $"Slots go from 0 to {_slots - 1} (set the count on the save service).");
        }
    }
}
