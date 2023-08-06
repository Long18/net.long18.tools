using System;
using Object = UnityEngine.Object;

namespace Long18.SOBrowser
{
    [Serializable]
    public class AssetEntry
    {
        public string Path { get; protected set; } = string.Empty;
        public string RPath { get; protected set; } = string.Empty;
        public string Name { get; protected set; } = string.Empty;
        public Object Asset { get; protected set; } = default;
        public int MatchAmount { get; set; } = 0;
        public bool Visible { get; set; } = true;
        public bool Selected { get; set; } = false;

        public AssetEntry(string path, string name, Object asset)
        {
            Path = path;
            RPath = ReverseString(path);
            Name = name;
            Asset = asset;
        }

        private static string ReverseString(string s)
        {
            char[] charArray = s.ToCharArray();

            Array.Reverse(charArray);

            return new string(charArray);
        }
    }
}