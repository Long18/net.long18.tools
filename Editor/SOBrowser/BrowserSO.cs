using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Long18.SOBrowser;
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
        
        private ListView _listView;
        private Box _cardInfoBox;

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
        }
        private void OnFocus()
        {
            ReloadBrowserEditors();
            CreateListView();
        }

        #region UI

        private void InitUI()
        {
            rootVisualElement.Add(_uxml.CloneTree());
            rootVisualElement.styleSheets.Add(_styleSheet);

            _listView = rootVisualElement.Q<ListView>("card-list");
            _cardInfoBox = rootVisualElement.Q<Box>("card-info");
            _toolBarButton = rootVisualElement.Q<Button>("toolbar-plus-button");
            _tollBarMoreButton = rootVisualElement.Q<Button>("toolbar-more-button");
            _settingsButton = rootVisualElement.Q<Button>("settings-button");
            _searchingField = rootVisualElement.Q<ToolbarSearchField>("toolbar-search-field");

            _toolBarButton.style.backgroundImage =(StyleBackground)
                EditorGUIUtility.IconContent("Toolbar Plus").image;
            _tollBarMoreButton.style.backgroundImage = (StyleBackground)
                EditorGUIUtility.IconContent("Toolbar Plus More").image;
            _settingsButton.style.backgroundImage = (StyleBackground)
                EditorGUIUtility.IconContent("SettingsIcon").image;
            
            _toolBarButton.clicked += () =>
            {var rect = new Rect
                         {
                             position = position.position
                         };
             
                         rect.y += 50;
                         rect.x += 32;
             
                         rect.width =  position.width /2;
                         rect.height = 18;
                
            PopupWindow.Show(rect, new CreateNewEntryPopup(rect, "", (assetName) =>
            {
                Debug.Log($"Get name asset here: {assetName}");
            }));
            };
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

            _listView.itemsSource = cards;
            _listView.fixedItemHeight = 16;
            _listView.selectionType = SelectionType.Single;

            _listView.onSelectionChange += OnChoosingItem;
            _listView.RefreshItems();
        }


        private void OnChoosingItem(IEnumerable<object> items)
        {
            foreach (var item in items)
            {
                _cardInfoBox.Clear();
                var data = item;

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