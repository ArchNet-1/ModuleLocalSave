// ArchNet · Local Save Module — Save tool (ArchNet Hub > Services > Save > Open tool)
// Lists the save files of this computer, shows their content, opens their folder and deletes
// them (to test a first launch).
// Docs: https://archnet-plateform.fr/#ModuleLocalSave

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArchNet.Module.Editor
{
    public class SaveWindow : EditorWindow
    {
        const string StylePath = "Packages/com.archnet.hub/Editor/UI/Hub.uss";
        static string Folder => Path.Combine(Application.persistentDataPath, "saves");

        public static void Open()
        {
            var window = GetWindow<SaveWindow>();
            window.titleContent = new GUIContent("Saves");
            window.minSize = new Vector2(460, 320);
        }

        void OnFocus() => Rebuild();
        void CreateGUI() => Rebuild();

        void Rebuild()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            root.AddToClassList("archnet-hub");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);
            if (style != null) root.styleSheets.Add(style);
            root.style.paddingLeft = root.style.paddingRight = root.style.paddingTop = 8;

            var toolbar = new VisualElement();
            toolbar.AddToClassList("row");
            toolbar.AddToClassList("toolbar");
            toolbar.Add(new Button(() => EditorUtility.RevealInFinder(Directory.Exists(Folder) ? Folder : Application.persistentDataPath)) { text = "Open folder" });
            toolbar.Add(new Button(Rebuild) { text = "Refresh" });
            var spacer = new VisualElement();
            spacer.AddToClassList("grow");
            toolbar.Add(spacer);
            toolbar.Add(new Button(DeleteAll) { text = "Delete all saves" });
            root.Add(toolbar);

            string[] files = Directory.Exists(Folder)
                ? Directory.GetFiles(Folder, "*.json").OrderBy(f => f).ToArray()
                : new string[0];
            if (files.Length == 0)
            {
                var empty = new Label($"No save yet in {Folder}. Saves appear here once the game calls Save().");
                empty.AddToClassList("intro");
                root.Add(empty);
                return;
            }

            var list = new ScrollView();
            list.style.flexGrow = 1;
            foreach (string file in files)
            {
                var card = new VisualElement();
                card.AddToClassList("card");
                var title = new Label($"{Path.GetFileNameWithoutExtension(file)} · {File.GetLastWriteTime(file):yyyy-MM-dd HH:mm}");
                title.AddToClassList("card-title");
                card.Add(title);
                var content = new Label(File.ReadAllText(file));
                content.AddToClassList("muted");
                card.Add(content);
                list.Add(card);
            }
            root.Add(list);
        }

        void DeleteAll()
        {
            if (!Directory.Exists(Folder)) return;
            if (!EditorUtility.DisplayDialog("Delete all saves", $"Delete every file in {Folder}? This cannot be undone.", "Delete", "Cancel")) return;
            Directory.Delete(Folder, true);
            Rebuild();
        }
    }
}
