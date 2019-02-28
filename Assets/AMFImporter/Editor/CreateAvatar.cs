using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AdjutantSharp;
public class CreateAvatar : EditorWindow
{
    bool _requiredOnly;
    bool showBoneFields;
    public GameObject topObject;
    public List<string> boneNames;
    public List<Transform> matchingBones;
    Vector2 scrollPosition;
    Vector2 scrollPos2;
    HumanBone[] humanBones;
    AdjutantSharp.AvatarSetupTool.SkeletonBone[] skeletonBones;
    
    [MenuItem("Tools/Create Avatar Wizard")]
    static void CreateWizard()
    {
        //ScriptableWizard.DisplayWizard<CreateAvatar>("Create Light", "Create");
        CreateAvatar window = (CreateAvatar)EditorWindow.GetWindow(typeof(CreateAvatar));
        window.Show();
        //If you don't want to use the secondary button simply leave it out:
        //ScriptableWizard.DisplayWizard<WizardCreateLight>("Create Light", "Create");
    }

   void OnGUI(){
       _requiredOnly=EditorGUILayout.Toggle("Required bones Only",_requiredOnly);
       topObject=(GameObject)EditorGUILayout.ObjectField("Upper most GameObject",topObject,typeof(GameObject),true);
       if(boneNames==null){
           SetupBones();
       }
       showBoneFields=EditorGUILayout.Foldout(showBoneFields,"BoneFields");
       if(showBoneFields){
           scrollPosition=EditorGUILayout.BeginScrollView(scrollPosition);
            for(int i=0;i<boneNames.Count;i++){
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(boneNames[i]);
                while(i>=matchingBones.Count)
                    matchingBones.Add(null);
                matchingBones[i]=(Transform)EditorGUILayout.ObjectField(matchingBones[i],typeof(Transform),true);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
       }
       
        if(GUILayout.Button("Build")){
            Avatar avatar=Build(boneNames,matchingBones,topObject);
            Debug.Log("Avatar is valid:"+avatar.isValid);
            Animator anim = topObject.GetComponent<Animator>();
            if(anim!=null)
                anim.avatar=avatar;
            AssetDatabase.CreateAsset(avatar,"Assets\\"+topObject.name+".asset");
        }
            
        /* if(GUILayout.Button("AutoMap")){
            HumanDescription hd = new HumanDescription();
            skeletonBones=null;
            bool hasTranslationDOF;
            List<string> reports=AvatarSetupTool.SetupHumanSkeleton(topObject,ref hd.human,out skeletonBones,out hasTranslationDOF);
            hd.hasTranslationDoF=hasTranslationDOF;
            hd.skeleton=new SkeletonBone[skeletonBones.Length];
            System.Array.Copy(System.Array.ConvertAll(skeletonBones,(p => (SkeletonBone) p)),hd.skeleton,hd.skeleton.Length);

            Debug.Log("Report size:"+reports.Count);
            Debug.Log("SkeleBones:"+skeletonBones.Length);
            Debug.Log("HumanBones:"+humanBones.Length); 
            
        }
        scrollPos2=EditorGUILayout.BeginScrollView(scrollPos2);
        if(skeletonBones!=null){
            foreach(HumanBone hb in humanBones){
                EditorGUILayout.LabelField(hb.boneName);
            }
        }
        
        EditorGUILayout.EndScrollView(); */


   }

   void SetupBones(){
    boneNames=new List<string>();
    string[] boneName = HumanTrait.BoneName;
    for (int i = 0; i < HumanTrait.BoneCount; ++i)
    {
        if ((_requiredOnly&&HumanTrait.RequiredBone(i))||!_requiredOnly)
            boneNames.Add(boneName[i]);
    }
    matchingBones= new List<Transform>(boneNames.Count);
   }

   public static Avatar Build(List<string> boneNames,List<Transform> matchingBones,GameObject topObject){
        HumanBone[] humanBones = new HumanBone[boneNames.Count];
        List<Transform> bones = new List<Transform>();
        for(int i=0;i<boneNames.Count;i++){
            if(matchingBones[i]==null)
                continue;
            humanBones[i]=new HumanBone();
            humanBones[i].boneName=matchingBones[i].name;
            humanBones[i].humanName=boneNames[i];
            humanBones[i].limit.useDefaultValues=true;
            AddParentRecursive(bones,matchingBones[i],topObject.transform);
        }

        HumanDescription hd = new HumanDescription();
        hd.human=humanBones;
        SkinnedMeshRenderer smr=topObject.GetComponentInChildren<SkinnedMeshRenderer>();
        
        //Transform[] bones=new Transform[smr.bones.Length+1];  
        //System.Array.Copy(smr.bones,bones,smr.bones.Length);
        //bones[smr.bones.Length]=smr.bones[0].parent;



        SkeletonBone[] skeleton = new SkeletonBone[bones.Count];
        for(int i=0;i<bones.Count;i++){
            skeleton[i]= new SkeletonBone();
            skeleton[i].name=bones[i].name;
            skeleton[i].position=bones[i].localPosition;
            skeleton[i].rotation=bones[i].localRotation;
            skeleton[i].scale=bones[i].localScale;
        }
        hd.skeleton=skeleton;
        Avatar avatar=AvatarBuilder.BuildHumanAvatar(topObject,hd);
        return avatar;

        
   }
   public static Avatar Build(Dictionary<int,Transform> matches,GameObject topObject){
       List<string> boneNames = new List<string>();
       List<Transform> matchingBones = new List<Transform>();
        foreach(int key in matches.Keys){
            boneNames.Add(HumanTrait.BoneName[key]);
            matchingBones.Add(matches[key]);
        }
        return Build(boneNames,matchingBones,topObject);
   }

   static void AddParentRecursive(List<Transform> t,Transform startAt,Transform stopAt){
        if(!t.Contains(startAt))
            t.Add(startAt);
        if(startAt.Equals(stopAt))
            return;
        else
            AddParentRecursive(t,startAt.parent,stopAt);
    
   }
}
