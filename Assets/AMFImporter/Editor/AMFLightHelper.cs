using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class AMFLightHelper : EditorWindow
{
    public GameObject root;
    public Material searchFor;
    public GameObject lightPrefab;

    [MenuItem("Tools/Light Helper")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        AMFLightHelper window = (AMFLightHelper)EditorWindow.GetWindow(typeof(AMFLightHelper));
        window.Show();
    }

    void OnGUI(){
        root=(GameObject)EditorGUILayout.ObjectField("Root Node",root,typeof(GameObject),true);
        searchFor=(Material)EditorGUILayout.ObjectField("Material to look for",searchFor,typeof(Material),true);
        lightPrefab=(GameObject)EditorGUILayout.ObjectField("Light Prefab",lightPrefab,typeof(GameObject),true);
        if(GUILayout.Button("Add Lights")){
            AddLights();
        }
        if(GUILayout.Button("Find mats")){
            List<GameObject> g=FindMats();
            Selection.objects=g.ToArray();
        }
    }

    List<GameObject> FindMats(){
        MeshRenderer[] mrs = root.GetComponentsInChildren<MeshRenderer>();
        List<GameObject> gos = new List<GameObject>();
        List<Material> mats;
        for(int i=0;i<mrs.Length;i++){
            mats = new List<Material>();
            mrs[i].GetSharedMaterials(mats);
            if(mats.Contains(searchFor)){
                gos.Add(mrs[i].gameObject);
            }
        }
        return gos;
    }
    void AddLights(){
        MeshRenderer[] mrs = root.GetComponentsInChildren<MeshRenderer>();
        List<Material> mats;
        for(int i=0;i<mrs.Length;i++){
            mats = new List<Material>();
            mrs[i].GetSharedMaterials(mats);
            if(mats.Contains(searchFor)){
                GameObject temp=GameObject.Instantiate(lightPrefab);
                temp.transform.SetParent(mrs[i].gameObject.transform,true);

                temp.transform.localPosition=mrs[i].GetComponent<MeshFilter>().sharedMesh.bounds.center;
            }
        }

    }

}
