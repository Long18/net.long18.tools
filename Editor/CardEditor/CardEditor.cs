using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Long18.Tools
{
    public class CardEditor : EditorWindow
    {
        [SerializeField] private VisualTreeAsset _uxml;
        [SerializeField] private StyleSheet _styleSheet;

        [MenuItem("Long 18/Tool Cards")]
        public static void ShowWindow()
        {
            var window = GetWindow<CardEditor>();
            window.titleContent = new GUIContent("Card");
            window.minSize = new Vector2(800, 600);
        }

        private void OnEnable()
        {
            rootVisualElement.Add(_uxml.CloneTree());
            rootVisualElement.styleSheets.Add(_styleSheet);

            CreateListView();
        }

        private void CreateListView()
        {
            FindDatas(out Object[] cards, "locationSO");

            var cardList = rootVisualElement.Q<ListView>("card-list");
            cardList.itemsSource = cards;
            cardList.fixedItemHeight = 16;
            cardList.selectionType = SelectionType.Single;

            cardList.onSelectionChange += OnChoosingItem;
            cardList.RefreshItems();
        }

        private void OnChoosingItem(IEnumerable<object> items)
        {
            foreach (var item in items)
            {
                var cardInfoBox = rootVisualElement.Q<Box>("card-info");
                cardInfoBox.Clear();
                var data = item;

                SerializedObject sObject = new SerializedObject((Object)data);
                SerializedProperty sDataProperty = sObject.GetIterator();
                sDataProperty.Next(true);

                while (sDataProperty.NextVisible(false))
                {
                    PropertyField prop = new PropertyField(sDataProperty);
                    prop.Bind(sObject);
                    cardInfoBox.Add(prop);
                }
            }
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
    }
}