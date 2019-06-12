using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MeshFilter))]
public class MeshFilterInspector : Editor
{
    public override void OnInspectorGUI(){
        DrawDefaultInspector();
        MeshFilter mf = target as MeshFilter;
        Mesh m = mf.sharedMesh;
        EditorGUILayout.LabelField("Verts:"+m.vertexCount);
        EditorGUILayout.LabelField("Submeshes:"+m.subMeshCount);
        EditorGUILayout.LabelField("Tris:"+m.triangles.Length/3);
        EditorGUILayout.LabelField("Bounds:"+m.bounds.ToString());
        EditorGUILayout.LabelField("WorldCenter:"+mf.transform.TransformPoint(m.bounds.center));
        /* for(int i=0;i<m.subMeshCount;i++){
            EditorGUILayout.LabelField(i+":"+m.GetTriangles(i).Length/3);
        } */
    }
}
