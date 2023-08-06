using System.Collections.Generic;
using Long18.Editor.Tools.Editor.SOBrowser.VisualElement.Utilities;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;
using Object = UnityEngine.Object;

namespace Long18.Editor.Tools.Editor.SOBrowser
{
    [CustomEditor(typeof(AssetEntry))]
    internal class CardTableEditor : UnityEditor.Editor
    {
        private SerializedProperty _transitions;

        private List<Object> _states = new();

        private bool[] _toggles;

        private AddTransitionHelper _addTransitionHelper;

        private UnityEditor.Editor _cachedEditor;
        private bool _isDisplay;

        private void OnEnable()
        {
            _addTransitionHelper = new AddTransitionHelper(this);
            Undo.undoRedoPerformed += OnUndoRedo;
            Reset();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            _addTransitionHelper.Dispose();
        }


        internal void Reset()
        {
            _toggles = new bool[_states.Count];
        }

        private void OnUndoRedo()
        {
            if (!serializedObject.UpdateIfRequiredOrScript()) return;
            Reset();
        }

        public override void OnInspectorGUI()
        {
            if (!_isDisplay)
            {
                ShowDataTableGUI();
            }
            else
            {
                StateEditorGUI();
            }
        }

        private void StateEditorGUI()
        {
            Separator();

            if (GUILayout.Button(EditorGUIUtility.IconContent("scrollleft"), GUILayout.Width(35), GUILayout.Height(20)))
            {
                _isDisplay = false;
            }

            Separator();
            HelpBox(
                "Edit the Actions that a State performs per frame. The order represent the order of execution.",
                MessageType.Info);
            Separator();

            LabelField(_cachedEditor.target.name, EditorStyles.boldLabel);
            Separator();
            _cachedEditor.OnInspectorGUI();
        }

        private void ShowDataTableGUI()
        {
            Separator();
            EditorGUILayout.HelpBox(
                "Click on any data to see the Data it contains, or click the Pencil/Wrench icon to see its Actions.",
                MessageType.Info);
            Separator();

            serializedObject.UpdateIfRequiredOrScript();

            for (int i = 0; i < _states.Count; i++)
            {
                var stateRect = BeginVertical(ContentStyle.WithPaddingAndMargins);
                EditorGUI.DrawRect(stateRect, ContentStyle.LightGray);

                var headerRect = BeginHorizontal();
                {
                    BeginVertical();
                    string label = "test";

                    if (i == 0) label += "(Initial)";

                    headerRect.height = EditorGUIUtility.singleLineHeight;
                    GUILayoutUtility.GetRect(headerRect.width, headerRect.height);
                    headerRect.x += 5;

                    // Toggle
                    {
                        var toggleRect = headerRect;
                        toggleRect.width -= 140;
                        _toggles[i] = EditorGUI.BeginFoldoutHeaderGroup(toggleRect,
                            foldout: _toggles[i],
                            content: label,
                            style: ContentStyle.StateListStyle);
                    }

                    Separator();
                    EndVertical();

                    // Header
                    {
                        bool Button(Rect position, string icon) =>
                            GUI.Button(position, EditorGUIUtility.IconContent(icon));

                        var buttonRect = new Rect(
                            x: headerRect.width - 105,
                            y: headerRect.y,
                            width: 35,
                            height: 20);

                        if (Button(buttonRect, "Toolbar Plus"))
                        {
                            if (_cachedEditor == null)
                            {
                                _cachedEditor = CreateEditor(_states[i]);
                            }
                        }
                    }
                }
            }

            var rect = BeginHorizontal();
            Space(rect.width - 55);

            _addTransitionHelper.Display(rect);

            EndHorizontal();
        }
    }
}