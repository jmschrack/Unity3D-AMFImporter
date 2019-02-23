using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class SkinnedmeshConverter : EditorWindow
{

    GameObject root;

    [MenuItem("Tools/Convert To Skinned mesh")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        SkinnedmeshConverter window = (SkinnedmeshConverter)EditorWindow.GetWindow(typeof(SkinnedmeshConverter));
        window.Show();
    }

    void OnGUI(){
        root=(GameObject)EditorGUILayout.ObjectField("Root node",root,typeof(GameObject),true);
        if(GUILayout.Button("Convert")){
            convertToSkinnedMesh(root);
        }
        if(GUILayout.Button("Merge&Convert")){
            ConvertToSkin(MergeAllMeshes(root.GetComponentsInChildren<MeshFilter>()));
        }
    }
    Mesh MergeAllMeshes(MeshFilter[] meshFilters){
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }
        Mesh masterMesh = new Mesh();
        masterMesh.CombineMeshes(combine);
        return masterMesh;
    }
    void ConvertToSkin(Mesh mesh){
        GameObject topObject = new GameObject(root.name+"_rigged");
        GameObject modelObject = new GameObject(root.name+"_model");
        modelObject.transform.parent=topObject.transform;
        GameObject rigRoot = new GameObject("Root");
        rigRoot.transform.parent=topObject.transform;
        List<Transform> bones = new List<Transform>();
        for(int i=0;i<mesh.boneWeights.Length;i++){
            GetTransform(bones,mesh.boneWeights[i].boneIndex0,mesh.boneWeights[i].boneIndex0+"Bone",Vector3.zero);
            if(mesh.boneWeights[i].weight1>0)
                GetTransform(bones,mesh.boneWeights[i].boneIndex1,mesh.boneWeights[i].boneIndex1+"Bone",Vector3.zero);
            if(mesh.boneWeights[i].weight2>0)
                GetTransform(bones,mesh.boneWeights[i].boneIndex2,mesh.boneWeights[i].boneIndex2+"Bone",Vector3.zero);
        }
        Matrix4x4[] bindPoses = new Matrix4x4[bones.Count];
        for(int i=0;i<bones.Count;i++){
            Transform t=GetTransform(bones,i,i+"EmptyBone",Vector3.zero);
            t.parent=rigRoot.transform;
            bindPoses[i]=bones[i].worldToLocalMatrix*rigRoot.transform.localToWorldMatrix;
            //bindPoses[1] = bones[1].worldToLocalMatrix * transform.localToWorldMatrix;
        }
        mesh.bindposes=bindPoses;
        SkinnedMeshRenderer smr = modelObject.AddComponent<SkinnedMeshRenderer>();
        smr.sharedMesh=mesh;
        smr.bones=bones.ToArray();
        smr.rootBone=rigRoot.transform;
    }

    void convertToSkinnedMesh(GameObject root){
        GameObject topNode = new GameObject(root.name+"_skinned");
        GameObject rigRoot = new GameObject("Root");
        rigRoot.transform.parent=topNode.transform;
        List<Transform> bones = new List<Transform>();
        List<SkinnedMeshRenderer> smrs = new List<SkinnedMeshRenderer>();
        MeshFilter[] meshFilters= root.GetComponentsInChildren<MeshFilter>();

        for(int i=0;i<meshFilters.Length;i++){
            if(meshFilters[i].sharedMesh.boneWeights.Length==0){
                //not skinned. Just copy
                GameObject temp = new GameObject(meshFilters[i].gameObject.name);
                temp.transform.parent=topNode.transform;
                MeshFilter mf = temp.AddComponent<MeshFilter>();
                mf.sharedMesh=meshFilters[i].sharedMesh;
                MeshRenderer mr = temp.AddComponent<MeshRenderer>();
                mr.sharedMaterials=meshFilters[i].GetComponent<MeshRenderer>().sharedMaterials;
            }else{
                GameObject temp = new GameObject(meshFilters[i].transform.parent.name+"_model");
                temp.transform.parent=topNode.transform;
                SkinnedMeshRenderer smr = temp.AddComponent<SkinnedMeshRenderer>();
                smr.sharedMaterials=meshFilters[i].GetComponent<MeshRenderer>().sharedMaterials;
                smr.rootBone=rigRoot.transform;
                //
                for(int b =0;b<meshFilters[i].sharedMesh.boneWeights.Length;b++){
                    GetTransform(bones,meshFilters[i].sharedMesh.boneWeights[b].boneIndex0,meshFilters[i].sharedMesh.boneWeights[b].boneIndex0+"_Bone",Vector3.zero);
                    if(meshFilters[i].sharedMesh.boneWeights[i].weight1>0)
                        GetTransform(bones,meshFilters[i].sharedMesh.boneWeights[b].boneIndex1,meshFilters[i].sharedMesh.boneWeights[b].boneIndex1+"_Bone",Vector3.zero);
                    if(meshFilters[i].sharedMesh.boneWeights[i].weight2>0)
                        GetTransform(bones,meshFilters[i].sharedMesh.boneWeights[b].boneIndex2,meshFilters[i].sharedMesh.boneWeights[b].boneIndex2+"_Bone",Vector3.zero);
                    //bindPoses[i]=bones[i].worldToLocalMatrix*rigRoot.transform.localToWorldMatrix;
                }
                smr.sharedMesh=meshFilters[i].sharedMesh;
                smrs.Add(smr);
            }
        }
        List<Matrix4x4> bindPoses= new List<Matrix4x4>();
        //assign parents and fill in missing bones
        for(int i=0;i<bones.Count;i++){
            Transform t = GetTransform(bones,i,i+"_EmptyBone",Vector3.zero);
            t.parent=rigRoot.transform;
            bindPoses.Add(t.worldToLocalMatrix*rigRoot.transform.localToWorldMatrix);
        }
        
        //Assign bindboses
        foreach(SkinnedMeshRenderer smr in smrs){
           smr.sharedMesh.bindposes=bindPoses.ToArray();
           smr.bones=bones.ToArray();
        }
    
    }


    Transform GetTransform(List<Transform> transforms, int index, string defName,Vector3 defPos){
        Transform t;
        if(!(index<transforms.Count)||transforms[index]==null){
            t= new GameObject(defName).transform;
            t.position=defPos;
            InsertTransform(transforms,index,t);
        }
        return transforms[index];
    }

    void InsertTransform(List<Transform> transforms,int index,Transform t){
        while(index>=transforms.Count){
            transforms.Add(null);
        }
        transforms[index]=t;
    }

   
}
