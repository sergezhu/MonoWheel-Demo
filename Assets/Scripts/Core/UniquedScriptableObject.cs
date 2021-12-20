using System;
using System.Linq;
using Attributes;
using UnityEngine;

namespace Core
{
    public abstract class UniquedScriptableObject : ScriptableObject 
    {
        [SerializeField][GUIReadOnly]
        private string _id;

        public string ID => _id;


        private void OnValidate()
        {
#if UNITY_EDITOR
            if (_id == "")
                GenerateID();
#endif
        }
    
#if UNITY_EDITOR
        public void CheckDuplicatesAndGenerateID()
        {
            var allSoList = EditorTools.EditorResourcesProvider.GetAllSOAssets<ScriptableObject>();
            var baseSoList = allSoList.Where(obj => obj is UniquedScriptableObject).ToList();
            var sameIDObjectsCount = baseSoList.Count(obj => ((UniquedScriptableObject) obj).ID == _id);
        
            if (sameIDObjectsCount > 1 || string.IsNullOrEmpty(_id))
                GenerateID();
        
        }

        [ContextMenu("🙌 GenerateID")]
        private void GenerateID()
        {
            //_id = UnityEditor.GUID.Generate().ToString();
            _id = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
