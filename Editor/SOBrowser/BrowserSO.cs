using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Long18.SOBrowser;
using Long18.Tools.Component;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using PopupWindow = UnityEditor.PopupWindow;

namespace Long18.Tools
{
    public class BrowserSO : EditorWindow
    {
        [SerializeField] private VisualTreeAsset _uxml;
        [SerializeField] private StyleSheet _styleSheet;

        [SerializeField] private VisualTreeAsset _itemUxml;
        [SerializeField] private StyleSheet _itemStyleSheet;

        private static readonly List<Type> _browsableTypes = new();
        private static string[] _browsableTypesName = Array.Empty<string>();
        private static Dictionary<Type, BrowserSOEditor> _editors;

        protected List<AssetEntry> _assetList = new();
        protected List<AssetEntry> _sortedAssetList = new();

        private BrowserSOEditor _currentSoEditor;
        private Object _currentObject;

        protected int _currentTypeIndex;

        private Button _toolBarButton;
        private Button _tollBarMoreButton;
        private Button _settingsButton;
        private ToolbarSearchField _searchingField;

        private ScrollView _scrollView;
        private Box _cardInfoBox;

        private List<VisualElement> _rowParents;
        private ItemComponent _selectedItem;

        [MenuItem("Long 18/Tool Cards")]
        public static BrowserSO ShowWindow()
        {
            ReloadBrowserEditors();

            BrowserSO[] windows = Resources.FindObjectsOfTypeAll<BrowserSO>();
            if (windows.Length > 0) return windows[0];

            BrowserSO window = GetWindow<BrowserSO>();

            window.ShowTab();
            return window;
        }

        private void OnEnable()
        {
            InitUI();

            ReloadBrowserEditors();
            CreateListView();

            ItemComponent.ItemClicked += OnItemClicked;
        }

        private void OnFocus()
        {
            ReloadBrowserEditors();
            CreateListView();

            ItemComponent.ItemClicked += OnItemClicked;
        }


        #region UI

        private void InitUI()
        {
            rootVisualElement.Add(_uxml.CloneTree());
            rootVisualElement.styleSheets.Add(_styleSheet);

            _cardInfoBox = rootVisualElement.Q<Box>("card-info");
            _toolBarButton = rootVisualElement.Q<Button>("toolbar-plus-button");
            _tollBarMoreButton = rootVisualElement.Q<Button>("toolbar-more-button");
            _settingsButton = rootVisualElement.Q<Button>("settings-button");
            _searchingField = rootVisualElement.Q<ToolbarSearchField>("toolbar-search-field");

            _scrollView = rootVisualElement.Q<ScrollView>("card-list");
            _rowParents = _scrollView.Children() as List<VisualElement>;

            _toolBarButton.style.backgroundImage = (StyleBackground)
                EditorGUIUtility.IconContent("Toolbar Plus").image;
            _tollBarMoreButton.style.backgroundImage = (StyleBackground)
                EditorGUIUtility.IconContent("Toolbar Plus More").image;
            _settingsButton.style.backgroundImage = (StyleBackground)
                EditorGUIUtility.IconContent("SettingsIcon").image;

            _toolBarButton.clicked += () =>
            {
                var rect = new Rect
                {
                    position = position.position
                };

                rect.y += 50;
                rect.x += 32;

                rect.width = position.width / 2;
                rect.height = 18;

                PopupWindow.Show(rect,
                    new CreateNewEntryPopup(rect, "",
                        (assetName) => { Debug.Log($"Get name asset here: {assetName}"); }));
            };

            ClearRows();
        }

        private void CreateListView()
        {
            Type currentType = null;

            if (_currentSoEditor == null && _currentObject != null)
            {
                OpenObject(_currentObject);
            }
            else if (_browsableTypes.Count > 0)
            {
                currentType = GetType(_browsableTypes[0]);
            }

            if (currentType == null) return;

            FindDatas(out Object[] cards, currentType.Name);

            _assetList.Clear();
            _sortedAssetList.Clear();

            foreach (var card in cards)
            {
                _assetList.Add(CreateAssetEntry(card));
            }

            ShowItems(_assetList.ToArray());
        }

        protected AssetEntry CreateAssetEntry(Object asset)
        {
            string name = asset.name;

            string path = $"{AssetDatabase.GetAssetPath(asset)}.{name}";

            AssetEntry entry = new AssetEntry(path: path, name: name, asset: asset);

            return entry;
        }

        private void OnItemClicked(ItemComponent item)
        {
            SelectItem(_selectedItem, false);

            SelectItem(item, true);

            _cardInfoBox.Clear();
            Object data = item.ItemData.Asset;

            SerializedObject sObject = new SerializedObject((Object)data);
            SerializedProperty sDataProperty = sObject.GetIterator();
            sDataProperty.Next(true);

            while (sDataProperty.NextVisible(false))
            {
                PropertyField prop = new PropertyField(sDataProperty);
                prop.Bind(sObject);
                _cardInfoBox.Add(prop);
            }
        }

        private void SelectItem(ItemComponent item, bool state)
        {
            if (item == null) return;

            _selectedItem = (state) ? item : null;
            item.CheckItem(state);
        }

        private void ShowItems(AssetEntry[] items)
        {
            int maxCount = Mathf.Clamp(items.Length, 0, 20 * _rowParents.Count);

            ClearRows();

            for (int i = 0; i < maxCount; i++)
            {
                int parentIndex = i / 20;
                CreateItemButton(items[i], _rowParents[parentIndex]);
            }

            // HideEmptyRows();
        }

        private void HideEmptyRows()
        {
            for (int i = 0; i < _rowParents.Count; i++)
            {
                _rowParents[i].style.display = (IsRowEmpty(_rowParents[i]))
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            }
        }

        private bool IsRowEmpty(VisualElement rowParent)
        {
            VisualElement itemElement = rowParent.Q<VisualElement>(className: "item__type");
            return itemElement == null;
        }

        private void ClearRows()
        {
            foreach (var element in _rowParents)
            {
                if (element == null) continue;
                element.Clear();
            }
        }

        private void CreateItemButton(AssetEntry data, VisualElement parentElement)
        {
            if (parentElement == null)
            {
                Debug.Log("InventoryScreen.CreateGearItemButton: missing parent element");
                return;
            }

            TemplateContainer itemUIElement = _itemUxml.Instantiate();
            itemUIElement.styleSheets.Add(_itemStyleSheet);

            ItemComponent itemComponent = new ItemComponent(data);

            itemComponent.SetVisualElements(itemUIElement);
            itemComponent.SetData(itemUIElement);
            itemComponent.RegisterButtonCallbacks();

            parentElement.Add(itemUIElement);
        }

        #endregion

        #region Over Engineer

        private static void ReloadBrowserEditors()
        {
            if (_editors != null) return;

            _editors = new();
            List<string> typesName = new List<string>();

            Assembly[] allFiles = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var file in allFiles)
            {
                foreach (var type in file.GetTypes())
                {
                    if (type.BaseType is not { IsGenericType: true }) continue;
                    if (type.BaseType.GetGenericTypeDefinition() != typeof(BrowserSOEditor<>)) continue;
                    if (type.BaseType == null && _editors != null) return;

                    Type genericArgument = type.BaseType?.GetGenericArguments()[0];
                    _editors[genericArgument!] = (BrowserSOEditor)Activator.CreateInstance(type);

                    _browsableTypes.Add(genericArgument);
                    typesName.Add(genericArgument?.Namespace);
                }
            }

            _browsableTypesName = _browsableTypesName.ToArray();
        }

        private static void OpenObject(Object obj)
        {
            BrowserSO window = ShowWindow();
            Type type = obj.GetType();

            window.GetType(type);
            BrowserSOEditor soEditor = window._currentSoEditor;

            Object[] target = { obj };
            soEditor.SetTargetObjects(target);
        }

        private Type GetType(Type type)
        {
            while (type != null && _editors.ContainsKey(type) == false) type = type.BaseType;
            if (type == null) return null;

            _currentTypeIndex = _browsableTypes.IndexOf(type);
            _currentSoEditor = _editors[type];
            _currentSoEditor.Editor_SetBrowser(this);

            return type;
        }

        private void FindDatas<T>(out T[] data, string name = "") where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{name}");

            data = new T[guids.Length];

            for (var index = 0; index < guids.Length; index++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[index]);
                data[index] = AssetDatabase.LoadAssetAtPath<T>(path);
            }
        }

        #endregion
    }
}