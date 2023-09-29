using System;
using Long18.SOBrowser;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Long18.Tools.Component
{
    public class ItemComponent
    {
        public static Action<ItemComponent> ItemClicked;

        private const string _icon = "item__icon";
        private const string _checkMark = "item__checkmark";
        private const string _itemName = "item__name";
        private const string _itemType = "item__type";

        public AssetEntry ItemData { get; private set; }

        public VisualElement IconElement { get; private set; }
        public VisualElement CheckMarkElement { get; private set; }

        public Label ItemNameElement { get; private set; }
        public Label ItemTypeElement { get; private set; }

        public ItemComponent(AssetEntry itemData)
        {
            ItemData = itemData;
        }

        public void SetVisualElements(TemplateContainer itemElement)
        {
            if (itemElement == null)
                return;

            IconElement = itemElement.Q(_icon);
            ItemNameElement = itemElement.Q<Label>(_itemName);
            ItemTypeElement = itemElement.Q<Label>(_itemType);
            CheckMarkElement = itemElement.Q(_checkMark);
            CheckMarkElement.style.display = DisplayStyle.None;
        }

        public void SetData(TemplateContainer itemElement)
        {
            if (itemElement == null)
                return;

            ItemNameElement.text = ItemData.Name;
            ItemTypeElement.text = ItemData.Path;
        
            IconElement.style.backgroundImage = new StyleBackground((Texture2D)EditorGUIUtility.IconContent("BuildSettings.Lumin On").image);
        }

        public void RegisterButtonCallbacks()
        {
            IconElement?.RegisterCallback<ClickEvent>(ClickItem);
        }

        private void ClickItem(ClickEvent evt)
        {
            ToggleCheckItem();

            ItemClicked?.Invoke(this);
        }

        public void CheckItem(bool state)
        {
            if (_checkMark == null) return;

            CheckMarkElement.style.display = (state)
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        private void ToggleCheckItem()
        {
            if (CheckMarkElement == null) return;

            bool state = CheckMarkElement.style.display == DisplayStyle.None;
            CheckItem(state);
        }
    }
}