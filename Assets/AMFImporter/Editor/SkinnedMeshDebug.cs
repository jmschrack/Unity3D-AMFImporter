using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class SkinnedMeshDebug : EditorWindow
{
    SkinnedMeshRenderer skinnedMeshRenderer;
    Mesh _mesh;
    Mesh  miniMesh;
    Vector2 scrollPos=Vector2.zero;

    /*
    Conversion Stuff
    
     */
    GameObject parentObject;



    public Mesh mesh{
        get{return _mesh;}
        set{
            if(value!=_mesh){
                AnalyzeBones(value);
            }
            _mesh=value;
        }
    }
    List<BoneWeight> boneWeights;
    int lowerIndex=999999,upperIndex=-1;
    [MenuItem("Tools/Skinned Mesh Debug")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        SkinnedMeshDebug window = (SkinnedMeshDebug)EditorWindow.GetWindow(typeof(SkinnedMeshDebug));
        window.Show();
    }
    bool showConvert;
    void OnGUI(){
        skinnedMeshRenderer=(SkinnedMeshRenderer)EditorGUILayout.ObjectField("Mesh",skinnedMeshRenderer,typeof(SkinnedMeshRenderer),true);
        if(skinnedMeshRenderer!=null)
            mesh= skinnedMeshRenderer.sharedMesh;
        /*if(mesh!=null)
            EditorGUILayout.LabelField("VertCount:"+mesh.vertexCount); */
        scrollPos=EditorGUILayout.BeginScrollView(scrollPos);
        if(boneWeights!=null){
            EditorGUILayout.LabelField("BoneCount:"+boneWeights.Count);
            EditorGUILayout.LabelField("lower index:"+lowerIndex);
            EditorGUILayout.LabelField("Upper Index:"+upperIndex);   
            //EditorGUILayout.LabelField("SkinnedBonesCount:"+skinnedMeshRenderer.bones.Length);
            List<string> nodes = new List<string>();
            bool bone2=false,bone3=false;
            
            foreach(BoneWeight bw in boneWeights){
                bone2=bone2||bw.weight2>0;
                bone3=bone3||bw.weight3>0;
                if(bone2&&!nodes.Contains(skinnedMeshRenderer.bones[bw.boneIndex2].name)){
                    nodes.Add(skinnedMeshRenderer.bones[bw.boneIndex2].name);
                    EditorGUILayout.LabelField(bw.boneIndex2+":"+skinnedMeshRenderer.bones[bw.boneIndex2].name);
                }
            }
            foreach(Transform t in skinnedMeshRenderer.bones){
                EditorGUILayout.LabelField(t.name);
            }
            EditorGUILayout.LabelField("Bone2?"+bone2);
            EditorGUILayout.LabelField("Bone3?"+bone3);
            //EditorGUILayout.LabelField("Unique Bones:"+uniqueBones.Count);
        }
        EditorGUILayout.EndScrollView();
        showConvert=EditorGUILayout.Foldout(showConvert,"Convert to SkinnedMesh");
        if(showConvert){
            parentObject=(GameObject)EditorGUILayout.ObjectField("Parent Object",parentObject,typeof(GameObject),true);
            if(GUILayout.Button("Convert to Rig")){
                convertToSkinnedMesh(parentObject);
            }
            miniMesh = (Mesh)EditorGUILayout.ObjectField("Single Mesh",miniMesh,typeof(Mesh),false);
            if(GUILayout.Button("Convert Mesh")){
                miniConvertSkinMesh(miniMesh);
            }
        }
    }

    void AnalyzeBones(Mesh mesh){
        if(mesh==null){
            boneWeights=null;
            return;
        }
            
        boneWeights= new List<BoneWeight>();
        lowerIndex=int.MaxValue;
        upperIndex=-1;
        BoneWeight[] bw = mesh.boneWeights;
        boneWeights.AddRange(bw);
        for(int i=0;i<bw.Length;i++){
            
            if(bw[i].weight0>0){
                lowerIndex=Mathf.Min(lowerIndex,bw[i].boneIndex0);
                upperIndex=Mathf.Max(upperIndex,bw[i].boneIndex0);
            }
            if(bw[i].weight1>0){
                lowerIndex=Mathf.Min(lowerIndex,bw[i].boneIndex1);
                upperIndex=Mathf.Max(upperIndex,bw[i].boneIndex1);
            }
            if(bw[i].weight2>0){
                lowerIndex=Mathf.Min(lowerIndex,bw[i].boneIndex2);
                upperIndex=Mathf.Max(upperIndex,bw[i].boneIndex2);
            }
            if(bw[i].weight3>0){
                lowerIndex=Mathf.Min(lowerIndex,bw[i].boneIndex3);
                upperIndex=Mathf.Max(upperIndex,bw[i].boneIndex3);
            }
                
            
        }

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

    void miniConvertSkinMesh(Mesh m){
        GameObject newSkin = new GameObject("NewSkin");
        SkinnedMeshRenderer smr = newSkin.AddComponent<SkinnedMeshRenderer>();
        List<Transform> transforms = new List<Transform>();
        GetTransform(transforms,0,"Root",Vector3.zero).parent=newSkin.transform;
        m.bindposes=new Matrix4x4[m.boneWeights.Length];
        for(int b=0;b<m.boneWeights.Length;b++){
            Transform t=GetTransform(transforms,m.boneWeights[b].boneIndex0,m.name,Vector3.zero);
            t.parent=newSkin.transform;
            if(m.boneWeights[b].weight1>0){
                t=GetTransform(transforms,m.boneWeights[b].boneIndex1,m.name,Vector3.zero);
                t.parent=newSkin.transform;
            }
            if(m.boneWeights[b].weight2>0){
                t=GetTransform(transforms,m.boneWeights[b].boneIndex2,m.name,Vector3.zero);
                t.parent=newSkin.transform;
            }
            if(m.boneWeights[b].weight3>0){
                t=GetTransform(transforms,m.boneWeights[b].boneIndex3,m.name,Vector3.zero);
                t.parent=newSkin.transform;
            }
            m.bindposes[b]=Matrix4x4.identity;
            
        }
        for(int i=0;i<transforms.Count;i++){
            if(transforms[i]==null){
                GameObject temp = new GameObject("Empty"+i);
                temp.transform.parent=newSkin.transform;
                transforms[i]=temp.transform;
            }
        }
        
        smr.rootBone=GetTransform(transforms,0,"Root",Vector3.zero);
        
        smr.bones=transforms.ToArray();
        m.RecalculateBounds();
        smr.sharedMesh=m;
        smr.localBounds=m.bounds; 
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