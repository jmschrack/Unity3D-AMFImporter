using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
namespace AdjutantSharp{

public class AMFInspector : BaseAssetImporterTabUI
{
    Vector2 scrollPos;
    bool showMarkerGroup,showRegions,showMaterials,showNodes;
    
    

    public AMFInspector(ScriptedImporterEditor panelContainer)
            : base(panelContainer)
        {
        }
    internal override void OnEnable(){
        
        
        //markerGroups=(List<AMF_MarkerGroup>)(amf.FindPropertyRelative("markerGroups").objectReferenceValue);
        //object mg=amf.FindPropertyRelative("markerGroups").objectReferenceValue;
        //Debug.Log(mg.GetType());
    }
    public override void OnInspectorGUI(){
       // EditorGUI.BeginProperty(position, label, property);
        // EditorGUI.PropertyField
        AMFImporter importer=target as AMFImporter;
        if(importer.amf==null||importer.amf.version==0){
            EditorGUILayout.LabelField("AMF not cached. Try reimporting?");
            return;
        }
        EditorGUILayout.LabelField("Name:"+(importer.amf.modelName));
        EditorGUILayout.LabelField("AMF Version:"+importer.amf.version);
        
        scrollPos=EditorGUILayout.BeginScrollView(scrollPos);
        showMarkerGroup=EditorGUILayout.Foldout(showMarkerGroup,string.Format("MarkerGroup [{0}]",importer.amf.markerGroups.Count));
        if(showMarkerGroup){
            foreach(AMF_MarkerGroup markerGroup in importer.amf.markerGroups){
                foreach(AMF_Marker marker in markerGroup.markers){
                    EditorGUILayout.LabelField(string.Format("{0} R{1}:P{2}:N{3} {4}",markerGroup.name,marker.regionIndex,marker.permutationIndex,marker.nodeIndex,marker.position));
                }
            }
        } 
        showRegions=EditorGUILayout.Foldout(showRegions,string.Format("Regions [{0}]",importer.amf.regionInfo.Count));
        if(showRegions){
            foreach(AMF_RegionInfo region in importer.amf.regionInfo){
                foreach(AMF_Permutations perm in region.permutations){
                    EditorGUILayout.LabelField(string.Format("{0}:{1}",region.name,perm.pName));
                }
            }
        }
        showNodes=EditorGUILayout.Foldout(showNodes,string.Format("Nodes [{0}]",importer.amf.nodes.Count));
        if(showNodes){
            foreach(AMF_Node node in importer.amf.nodes){
                EditorGUILayout.LabelField(node.name);
            }
        }
        showMaterials=EditorGUILayout.Foldout(showMaterials,string.Format("Materials [{0}]",importer.amf.shaderInfos.Count));
        if(showMaterials){
            foreach(ShaderInfo si in importer.amf.shaderInfos){
                EditorGUILayout.LabelField(si.sName);
            }
        }
        EditorGUILayout.EndScrollView();
    }
}
}