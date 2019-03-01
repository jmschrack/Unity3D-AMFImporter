using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public static class MergeSkinnedMeshes 
{
    /**
     */
    public static Mesh Merge(Mesh m1, Mesh m2,List<Transform> bones,Transform[] bones2){
        
        Mesh newMesh = new Mesh();
        
        List<Vector3> vertices = new List<Vector3>();
        vertices.AddRange(m1.vertices);
        vertices.AddRange(m2.vertices);
        newMesh.vertices=vertices.ToArray();
        List<Vector2> uvs = new List<Vector2>();
        uvs.AddRange(m1.uv);
        uvs.AddRange(m2.uv);
        newMesh.uv=uvs.ToArray();
        List<BoneWeight> bw = new List<BoneWeight>();
        bw.AddRange(m1.boneWeights);
        
        //Remap bones as necessary
        List<Matrix4x4> bindposes= new List<Matrix4x4>();
        bindposes.AddRange(m1.bindposes);
        int[] remap = new int[bones2.Length];
        for(int i=0;i<remap.Length;i++){
            remap[i]=bones.IndexOf(bones2[i]);
            if(remap[i]==-1){
                remap[i]=bones.Count;
                bones.Add(bones2[i]);

            }
        }
        for(int i=0;i<m2.boneWeights.Length;i++){
            BoneWeight bwtemp = m2.boneWeights[i];
            bwtemp.boneIndex0=remap[bwtemp.boneIndex0];
            if(bwtemp.boneIndex1>=0)
                bwtemp.boneIndex1=remap[bwtemp.boneIndex1];
            if(bwtemp.boneIndex2>=0)
                bwtemp.boneIndex2=remap[bwtemp.boneIndex2];
            if(bwtemp.boneIndex3>=0)
                bwtemp.boneIndex3=remap[bwtemp.boneIndex3];
            bw.Add(bwtemp);
        }
        
        newMesh.boneWeights=bw.ToArray();
        newMesh.bindposes=m1.bindposes;
        newMesh.subMeshCount=m1.subMeshCount+m2.subMeshCount;
        for(int i=0;i<m1.subMeshCount;i++){
            int[] tris=m1.GetTriangles(i);
            newMesh.SetTriangles(tris,i);
        }
        
        int offset=m1.vertexCount;
        for(int i=0;i<m2.subMeshCount;i++){
            int[] tris=m2.GetTriangles(i);
            //tris.ForEach(index => index+offset);
            for(int t=0;t<tris.Length;t++){
                tris[t]+=offset;
            }
            newMesh.SetTriangles(tris,m1.subMeshCount+i);
        }
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
        return newMesh;
    }

    public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action){
        foreach(T item in enumeration){ action(item);}
    }
}
