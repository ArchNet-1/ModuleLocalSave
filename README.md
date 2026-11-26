<p align="center">
  <a href="https://archnet-plateform.fr"><img src="https://archnet-plateform.fr/ArchNetLogoWithBck.png" alt="ArchNet" width="240"></a>
</p>

# ArchNet · Local Save Module

Save slots for your game data as JSON files on the device: new game, load, save, delete, autosave, and upgrades of old saves.

- Unity **6000.0** or newer · Needs [ArchNet Core](https://github.com/ArchNet-1/ArchNetCore)
- Part of [ArchNet](https://archnet-plateform.fr), free and open-source Unity packages

## Add it to your game (one click)

*ArchNet > Hub > Services > Save > Add to game* creates, in `Assets/ArchNet/Save/`:

| File | What it is |
|---|---|
| `Scripts/GameSaveData.cs` | Everything your game saves (a `[Serializable]` class: level, coins, play time, unlocked items to start with). |
| `Scripts/GameSave.cs` | The service: starting values (`OnNewGame`), play time, upgrades of old saves (`Migrate`). |
| `Prefabs/Save.prefab` | Under the ArchNet Bootstrap. Its component sets the number of slots, the version and the autosave. |

## Use

```csharp
var save = Services.Get<GameSave>();
if (!save.Load(0)) save.NewGame(0);   // slot 0: continue, or start fresh
save.Current.coins += 10;
save.Save();                          // also done for you when the game pauses (mobile) or quits

SaveSlotInfo slot = save.GetSlot(1);  // Exists, SavedAt: for a "Load game" screen
save.Delete(1);
```

Files go to `persistentDataPath/saves/slot<N>.json`, written safely (temporary file, previous version kept as `.bak` and used if a file is broken). Each file records a version: when you change `GameSaveData` in a released game, raise **Version** on the component and upgrade old data in `Migrate(fromVersion, data)`.

**Open tool**: see the save files of this computer and their content, open their folder, delete them to test a first launch.

## License

MIT, see [LICENSE.md](LICENSE.md).
