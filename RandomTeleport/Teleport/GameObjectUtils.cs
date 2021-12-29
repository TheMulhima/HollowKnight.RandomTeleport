using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RandomTeleport1_4
{
    
    public static class GameObjectUtils
    {
        public static GameObject GetGameObjectByName(this Scene scene, string name,bool useBaseName = false)
        {
            GameObject foundGo;
            foreach (var go in scene.GetRootGameObjects()){
                if(go.GetName(useBaseName) == name){
                    return go;
                }
                foundGo = go.FindGameObjectInChildren(name,useBaseName);
                if(foundGo != null){
                    return foundGo;
                }
            }
            return null;
        }
        public static string GetName(this GameObject go,bool useBaseName = false){
            string ret = go.name;
            if(useBaseName){
                ret = ret.ToLower();
                ret.Replace("(clone)", "");
                ret = ret.Trim();
                ret.Replace("cln", "");
                ret = ret.Trim();
                ret = Regex.Replace(ret, @"\([0-9+]+\)", "");
                ret = ret.Trim();
                ret = Regex.Replace(ret, @"[0-9+]+$", "");
                ret = ret.Trim();
                ret.Replace("(clone)", "");
                ret = ret.Trim();
            }
            return ret;
        }
        
        public static GameObject FindGameObjectInChildren( this GameObject gameObject, string name ,bool useBaseName = false)
        {
            if( gameObject == null ){ return null; }

            foreach( var t in gameObject.GetComponentsInChildren<Transform>( true ) )
            {
                if( t.GetName(useBaseName) == name ) { return t.gameObject; }
            }
            return null;
        }
        
        public static string GetName(this Transform t,bool useBaseName = false){
            return t.gameObject.GetName(useBaseName);
        }
        
        public static string GetPath(this GameObject go,bool useBaseName = false){
            string path = go.GetName(useBaseName);
            GameObject currObj = go;
            while(currObj.transform.parent != null && currObj.transform.parent.gameObject != null){
                currObj = currObj.transform.parent.gameObject;
                path = currObj.GetName(useBaseName) + "/" + path; 
            }
            return path;
        }
    }
}