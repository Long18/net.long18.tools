using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Long18.Editor.Tools.Editor.SOBrowser
{
    public abstract class SOBrowserEditor
    {
        public SOBrowser Browser { get; set; }
        protected UnityEditor.Editor CachedEditor;

        public virtual void SetTargetObjects(Object[] objects)
        {
        }

        public virtual void RenderInspector()
        {
        }

        /// <summary>
        /// This method is called when user clicks on "Import" button in SOBrowser window
        ///  - directory is the directory of the selected file 
        /// </summary>
        /// <param name="directory">Directory of input file</param>
        /// <param name="callback">callback for adding new ScriptableObject to ListView</param>
        public virtual void ImportBatchData(string directory, Action<ScriptableObject> callback)
        {
        }

        public bool CreateDataFolder { get; protected set; }

        public string DefaultStoragePath { get; protected set; }

        public GenericMenu ContextMenu { get; protected set; }
    }

    public abstract class SOBrowserEditor<T> : SOBrowserEditor where T : Object
    {
        public T TargetObject { get; set; }

        public override void SetTargetObjects(Object[] objects)
        {
            if (objects == null || objects.Length <= 0) TargetObject = null;
            else TargetObject = (T)objects[0];

            UnityEditor.Editor.CreateCachedEditor(objects, null, ref CachedEditor);
            if (CachedEditor != null) CachedEditor.ResetTarget();
        }

        public override void RenderInspector()
        {
            if (TargetObject == null) return;
            CustomInspector(CachedEditor.serializedObject);
        }

        private void DrawDefaultInspector() => CachedEditor.OnInspectorGUI();

        protected virtual void CustomInspector(SerializedObject objects) => DrawDefaultInspector();

        public override void ImportBatchData(string directory, Action<ScriptableObject> callback)
        {
        }
    }
}