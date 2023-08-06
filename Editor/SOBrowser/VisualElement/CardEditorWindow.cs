using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Long18.Editor.Tools.Editor.SOBrowser
{
    public class CardEditorWindow : EditorWindow
    {
        private static CardEditorWindow _window;
        public static List<Type> FoundTypes = new();
        private static AssetEntry[] _currentData = new AssetEntry[0];

        [SerializeField] private VisualTreeAsset _visualTreeAsset = default;
        [SerializeField] private StyleSheet _styleSheet = default;

        private bool _doRefresh;
        private UnityEditor.Editor _tableEditor;

        [MenuItem("SO Browser", menuItem = "Tools/SO Browser %#o")]
        private static void Display()
        {
            if (_window != null) return;

            RefreshBrowser();
            _window = GetWindow<CardEditorWindow>("SO Browser");

            _window.Show();
        }

        private void Update()
        {
            if (!_doRefresh) return;

            RefreshBrowser();
            CreateListView();
            _doRefresh = false;
        }

        private void OnEnable()
        {
            rootVisualElement.Add(_visualTreeAsset.CloneTree());

            string nameLabel = $"label-{(EditorGUIUtility.isProSkin ? "pro" : "personal")}";
            rootVisualElement.Query<Label>().Build().ForEach(label => label.AddToClassList(nameLabel));

            rootVisualElement.styleSheets.Add(_styleSheet);

            minSize = new Vector2(800, 600);

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange playMode)
        {
            if (playMode != PlayModeStateChange.EnteredPlayMode) return;

            RefreshBrowser();
            CreateListView();
            _doRefresh = true;
        }

        private void OnFocus()
        {
            if (!_doRefresh)
            {
                _doRefresh = true;
            }
        }

        private void OnLostFocus()
        {
            ListView listView = rootVisualElement.Q<ListView>(className: "table-list");
            listView.onSelectionChange -= OnListSelectionChanged;
        }

        private void OnListSelectionChanged(IEnumerable<object> obj)
        {
            IMGUIContainer editor = rootVisualElement.Q<IMGUIContainer>(className: "table-editor");
            editor.onGUIHandler = null;

            List<object> list = (List<object>)obj;

            if (list.Count == 0) return;

            var table = (AssetEntry)list[0];
            if (table == null) return;

            if (_tableEditor == null)
            {
                _tableEditor = UnityEditor.Editor.CreateEditor(table.Asset, typeof(CardTableEditor));
            }
            else if (_tableEditor.target != table.Asset)
            {
                UnityEditor.Editor.CreateCachedEditor(table.Asset, typeof(CardTableEditor), ref _tableEditor);
            }

            Debug.Log(_tableEditor.target);
            editor.onGUIHandler = () =>
            {
                if (!_tableEditor.target)
                {
                    editor.onGUIHandler = null;
                    return;
                }

                ListView listView = rootVisualElement.Q<ListView>(className: "table-list");
                
                if (listView.selectedItem != _tableEditor.target)
                {
                    var state = listView.itemsSource.IndexOf(_tableEditor.target);
                    listView.selectedIndex = state;
                    Debug.Log(state);
                    if (state < 0)
                    {
                        editor.onGUIHandler = null;
                        return;
                    }
                }

                _tableEditor.OnInspectorGUI();
            };
        }

        private static void RefreshBrowser()
        {
            Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            IEnumerable<Type> types = allAssemblies.SelectMany(ass => ass.GetTypes())
                .Where(t =>
                    t.BaseType != null &&
                    t.BaseType.IsGenericType &&
                    t.BaseType.GetGenericTypeDefinition() == typeof(SOBrowserEditor<>));

            foreach (var type in types)
            {
                if (type.BaseType == null) return;

                Type generic = type.BaseType.GetGenericArguments()[0];
                FoundTypes.Add(generic);
            }

            _currentData = FindAssets();
        }

        private void CreateListView()
        {
            var assets = _currentData;

            ListView listView = rootVisualElement.Q<ListView>(className: "table-list");

            listView.makeItem = null;
            listView.bindItem = null;

            listView.itemsSource = assets;
            listView.fixedItemHeight = 16;

            string labelClass = $"label-{(EditorGUIUtility.isProSkin ? "pro" : "personal")}";
            listView.makeItem = () =>
            {
                var label = new Label();
                label.AddToClassList(labelClass);
                return label;
            };

            listView.bindItem = (element, i) => ((Label)element).text = assets[i].Name;
            listView.selectionType = SelectionType.Single;

            listView.onSelectionChange -= OnListSelectionChanged;
            listView.onSelectionChange += OnListSelectionChanged;

            if (_tableEditor && _tableEditor.target)
            {
                listView.selectedIndex = Array.IndexOf(assets, _tableEditor.target);
            }
        }

        private static AssetEntry[] FindAssets()
        {
            string[] assets = AssetDatabase.FindAssets($"t:{FoundTypes[0].Name}");

            HashSet<Object> foundData = new();
            List<AssetEntry> data = new();

            string lastPath = string.Empty;
            foreach (var guid in assets)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (path == lastPath) continue;
                lastPath = path;

                Object[] loadedAssets = AssetDatabase.LoadAllAssetsAtPath(path);

                foreach (var asset in loadedAssets)
                {
                    if (FoundTypes[0].IsInstanceOfType(asset))
                    {
                        foundData.Add(asset);
                    }
                }
            }

            foreach (var allData in foundData)
            {
                data.Add(CreateAssetEntry(allData));
            }

            return data.ToArray();
        }

        private static AssetEntry CreateAssetEntry(Object asset)
        {
            string name = asset.name;

            string path = $"{AssetDatabase.GetAssetPath(asset)}.{name}";

            AssetEntry entry = new AssetEntry(path, name, asset);

            return entry;
        }
    }
}