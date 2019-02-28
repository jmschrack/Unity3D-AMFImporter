using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(AvatarInspector))]
public class AvatarInspectorUI: Editor{

    public override void OnInspectorGUI(){
        Avatar avatar=((AvatarInspector)target).GetComponent<Animator>().avatar;
        if(avatar==null)
            return;
        EditorGUILayout.LabelField(avatar.name);
        EditorGUILayout.LabelField("Valid:"+avatar.isValid);
        EditorGUILayout.LabelField("IsHuman:"+avatar.isHuman);
    }
}
#endif
public class AvatarInspector : MonoBehaviour
{
    
}
