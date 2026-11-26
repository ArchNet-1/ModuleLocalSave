// ArchNet · Local Save Module — Save recipe (ArchNet Hub > Services > Save)
// Creates, under Assets/ArchNet/Save/: Scripts/GameSaveData.cs (what is saved),
// Scripts/GameSave.cs (the service) and Prefabs/Save.prefab, placed under the ArchNet Bootstrap.
// This assembly only compiles when the ArchNet Hub (com.archnet.hub 2.0.0+) is installed.
// Docs: https://archnet-plateform.fr/#ModuleLocalSave

using System;
using ArchNet.Core;
using ArchNet.Core.Editor;
using ArchNet.Installer.Editor;
using UnityEditor;
using UnityEngine;

namespace ArchNet.Module.Editor
{
    public sealed class SaveRecipe : ServiceRecipe
    {
        public const string RecipeId = "save";
        const string Templates = "Packages/com.archnet.module.localsave/Editor/Recipe/Templates/";
        const string ServiceScript = "Scripts/GameSave.cs";
        const string PrefabFile = "Prefabs/Save.prefab";

        public override string Id => RecipeId;
        public override string DisplayName => "Save";
        public override string Description => "Save slots for your game data on the device, with autosave and upgrades of old saves. Services.Get<GameSave>().Save().";
        public override string Category => "Data";
        public override string FolderName => "Save";
        public override string[] RequiredServices => new[] { BootstrapRecipe.RecipeId };
        public override bool HasTool => true;

        public override void Create(RecipeContext context)
        {
            context.WriteTemplate("Scripts/GameSaveData.cs", RecipeContext.LoadTemplate(Templates + "GameSaveData.cs.txt"));
            context.WriteTemplate(ServiceScript, RecipeContext.LoadTemplate(Templates + "GameSave.cs.txt"));
        }

        public override void AfterScriptsCompiled(RecipeContext context)
        {
            Type serviceType = RecipeTools.GeneratedType<GameService>(context, "GameSave", ServiceScript);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(context.PathOf(PrefabFile))
                                ?? context.SavePrefab(PrefabFile, new GameObject("Save", serviceType));
            BootstrapRecipe.AddUnderBootstrap(prefab);
        }

        public override void OpenTool() => SaveWindow.Open();
    }
}
