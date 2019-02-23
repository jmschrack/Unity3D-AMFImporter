using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class MeshHacking : EditorWindow
{
    MeshFilter meshFilter;
    Vector4 r0=new Vector4(1,0);
    Vector4 r1 = new Vector4(0,1);
    Vector4 r2 = new Vector4(0,0,1);
    Vector4 r3 = new Vector4(0,0,0,1);
    // Add menu named "My Window" to the Window menu
    [MenuItem("Tools/Mesh hacking")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MeshHacking window = (MeshHacking)EditorWindow.GetWindow(typeof(MeshHacking));
        window.Show();
    }
    

    void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        meshFilter = (MeshFilter)EditorGUILayout.ObjectField("Target Mesh",meshFilter,typeof(MeshFilter),true);
        r0=EditorGUILayout.Vector4Field("",r0);
        r1=EditorGUILayout.Vector4Field("",r1);
        r2=EditorGUILayout.Vector4Field("",r2);
        r3=EditorGUILayout.Vector4Field("",r3);
        GUI.enabled=meshFilter!=null;
        if(GUILayout.Button("Flip Mesh")){
            FlipMesh(meshFilter.sharedMesh);
        }
        if(GUILayout.Button("Flip UVs")){
            FlipUV(meshFilter.sharedMesh);
        }
        GUI.enabled=true;
        Matrix4x4 matr = Matrix4x4.identity;
        matr=matr.Convert3DSMatrixToUnity();
        EditorGUILayout.Vector4Field("",matr.GetRow(0));
        EditorGUILayout.Vector4Field("",matr.GetRow(1));
        EditorGUILayout.Vector4Field("",matr.GetRow(2));
        EditorGUILayout.Vector4Field("",matr.GetRow(3));
    }
    void FlipUV(Mesh mesh){
        Vector2[] uvs=mesh.uv;
        for(int i=0;i<uvs.Length;i++){
            uvs[i]=new Vector2(1-uvs[i].x,1-uvs[i].y);
        }
        mesh.uv=uvs;
    }

    void FlipMesh(Mesh mesh){
        Vector3[] verts = mesh.vertices;
        Matrix4x4 flipMat= Matrix4x4.identity;
        flipMat.SetRow(0,r0);
        flipMat.SetRow(1,r1);
        flipMat.SetRow(2,r2);
        flipMat.SetRow(3,r3);

        Matrix4x4 secondFlip = Matrix4x4.identity;
        secondFlip.SetRow(0,new Vector4(-1,0));
        for(int i=0;i<verts.Length;i++){
            verts[i]= flipMat.MultiplyPoint3x4(verts[i]);//secondFlip.MultiplyPoint(flipMat.MultiplyPoint3x4(verts[i]));//new Vector3(-verts[i].x,verts[i].z,-verts[i].y);
        }
        mesh.vertices=verts;
        for(int i=0;i<mesh.subMeshCount;i++){
            int[] tris=mesh.GetTriangles(i);
            for(int t=0;t<tris.Length;t+=3){
                int temp = tris[t];
                tris[t]=tris[t+2];
                tris[t+2]=temp;
            }
            mesh.SetTriangles(tris,i);
        }

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
