using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AdjutantSharp;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(AMFMaterialHelper))]
public class AMFMaterialHelperUI : Editor{
    bool showHax;
    int index;
    int offset;
    public override void OnInspectorGUI(){
        DrawDefaultInspector();
        if(GUILayout.Button("Setup Materials")){
            (target as AMFMaterialHelper).SetupMaterial();
        }
        AMFMaterialHelper amf =target as AMFMaterialHelper;
        showHax=EditorGUILayout.Foldout(showHax,"Helper Hacks");
        if(showHax){
            index=EditorGUILayout.IntField("Index:",index);
            AMFShaderInfo si = amf.shaderSettings[index];
            EditorGUILayout.LabelField(si.sName+":"+si.shaderType.ToString());
            if(GUILayout.Button("Toggle Shader Type")){
                if(si.shaderType==AMFShaderInfo.ShaderInfoType.Regular)
                    si.shaderType=AMFShaderInfo.ShaderInfoType.Terrain;
                else
                    si.shaderType=AMFShaderInfo.ShaderInfoType.Regular;
                si.SetupMaterial(amf.GetComponent<MeshRenderer>().sharedMaterials[index]);
            }
            if(si.shaderType==AMFShaderInfo.ShaderInfoType.Terrain){
                
               if(GUILayout.Button("Compress Blendmap")){
                   si.CompressBlendMapChannels();
               }
                
            }
        }
    }
}
#endif
public class AMFMaterialHelper : MonoBehaviour
{
    
    public List<AMFShaderInfo> shaderSettings;
   
    public void SetupMaterial(){
        MeshRenderer mr = GetComponent<MeshRenderer>();
        Material[] m = mr.sharedMaterials;       
        for(int i=0;i<m.Length;i++){
            shaderSettings[i].SetupMaterial(m[i]);
        }
        mr.sharedMaterials=m;
    }
}
