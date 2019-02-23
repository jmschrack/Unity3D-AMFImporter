using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiearchyVisualizer : MonoBehaviour
{
    public int maxDepth=30;
   void OnDrawGizmos(){
       RecursiveDive(transform,0,maxDepth);
   }

   void RecursiveDive(Transform t,int depth,int maxDepth){
       RayToParent(t);
       for(int i=0;i<t.childCount;i++){
            if(depth<maxDepth)
                RecursiveDive(t.GetChild(i),depth+1,maxDepth);
            else
                RayToParent(t.GetChild(i));
       }
   }

   void RayToParent(Transform t){
        if(t.parent!=null&&!t.Equals(transform))
            Gizmos.DrawLine(t.position,t.parent.position);
   }
}
