using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using System;
namespace AdjutantSharp{


public static class AMFHelperExt 
{
    public static Color32 ReadColor32(this BinaryReader reader){
        return new Color32(reader.ReadByte(),reader.ReadByte(),reader.ReadByte(),reader.ReadByte());
    }

    public static Vector3 ReadVector3(this BinaryReader reader)
    {
        return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }
    public static Vector3 ReadVector3Short(this BinaryReader reader)
    {
        return new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
    }

    public static Vector2 ReadVector2(this BinaryReader reader)
    {
        return new Vector2(reader.ReadSingle(), reader.ReadSingle());
    }
    
    public static Vector3Int ReadVector3Int(this BinaryReader reader){
        return new Vector3Int(reader.ReadInt32(),reader.ReadInt32(),reader.ReadInt32());
    }
    public static Vector4 ReadVector4(this BinaryReader reader,float w){
        Vector3 temp = reader.ReadVector3();
        return new Vector4(temp.x,temp.y,temp.z,w);
    }
    
    
    
    /* public static uint ReadBEUInt32(this BinaryReader reader){
        byte[] b = new byte[4];
        b[0]=reader.ReadByte();
        b[1]=reader.ReadByte();
        b[2]=reader.ReadByte();
        b[3]=reader.ReadByte();
        System.BitConverter.
        System.BitConverter.ToUInt32(b,0);
    } */
    
    public static UInt16 ReverseBytes(this UInt16 value)
    {
        return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
    }
    public static UInt32 ReverseBytes(this UInt32 value)
    {
        return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 | (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
    }
    public static UInt64 ReverseBytes(this UInt64 value)
    {
        return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
    }

    public static Matrix4x4 ReadMatrix4x4(this BinaryReader reader, bool rows=false){
        Vector4 v0 = reader.ReadVector4(0);
        Vector4 v1 = reader.ReadVector4(0);
        Vector4 v2 = reader.ReadVector4(0);
        Vector4 v3 = reader.ReadVector4(0);
        Matrix4x4 matr = new Matrix4x4();
        if(rows){
            matr.SetRow(0,v0);
            matr.SetRow(1,v1);
            matr.SetRow(2,v2);
            matr.SetRow(3,v3);
        }else{
            matr.SetColumn(0,v0);
            matr.SetColumn(1,v1);
            matr.SetColumn(2,v2);
            matr.SetColumn(3,v3);
        }
        //we have to pad an empty column for 3dsMax's fake-ass 4x3 matrix
        matr.m33=1;
        return matr;
    }

    /* public static Matrix4x4 ConvertHandedness(this Matrix4x4 matrix){
        Matrix4x4 flip = Matrix4x4.identity;
        flip.SetRow(0,flip.GetRow(2));
        flip.SetRow(2,new Vector4(1,0f));
        return matrix*flip;
    } */
    public static bool IsBad(this Vector3 vector){
        if(float.IsNaN(vector.x)||float.IsInfinity(vector.x)||float.IsNegativeInfinity(vector.x)||float.IsPositiveInfinity(vector.x))
            return true;
        if(float.IsNaN(vector.y)||float.IsInfinity(vector.y)||float.IsNegativeInfinity(vector.y)||float.IsPositiveInfinity(vector.y))
            return true;
        if(float.IsNaN(vector.z)||float.IsInfinity(vector.z)||float.IsNegativeInfinity(vector.z)||float.IsPositiveInfinity(vector.z))
            return true;

        return false;
    }

    public static Quaternion ExtractRotation(this Matrix4x4 m)
        {
            /* // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
            Quaternion q = new Quaternion();
            q.w =Mathf.Sqrt(Mathf.Max(0,1+ m[0,0]+ m[1,1]+ m[2,2]))/2;
            q.x =Mathf.Sqrt(Mathf.Max(0,1+ m[0,0]- m[1,1]- m[2,2]))/2;
            q.y =Mathf.Sqrt(Mathf.Max(0,1- m[0,0]+ m[1,1]- m[2,2]))/2;
            q.z =Mathf.Sqrt(Mathf.Max(0,1- m[0,0]- m[1,1]+ m[2,2]))/2;
            q.x *=Mathf.Sign(q.x *(m[2,1]- m[1,2]));
            q.y *=Mathf.Sign(q.y *(m[0,2]- m[2,0]));
            q.z *=Mathf.Sign(q.z *(m[1,0]- m[0,1])); */
            float x = Mathf.Atan2(m.m32,m.m33);
            float y=Mathf.Atan2(-m.m31,Mathf.Sqrt((m.m32*m.m32)+(m.m33*m.m33)));
            float z =Mathf.Atan2(m.m21,m.m11);
            //return Quaternion.LookRotation(m.GetColumn(2),m.GetColumn(1));
             //m.transpose.rotation;
             return Quaternion.Euler(-x,z,y);
        }
    public static Quaternion GetRotation(this Matrix4x4 m){
        return Quaternion.LookRotation(m.GetColumn(2),m.GetColumn(1));
    }
 
    public static Vector3 ExtractPosition(this Matrix4x4 matrix)
    {
        /* Vector3 temp=matrix.GetRow(3);
        return new Vector3(-temp.x,temp.z,temp.y); */
        return matrix.GetColumn(3);
    }
 
    public static Vector3 FlipHands(this Vector3 vector){
       
        return Matrix4x4.identity.convertRHStoLHS().MultiplyPoint3x4(vector);
    }
    public static Vector3 ExtractScale(this Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = ((Vector3)matrix.GetColumn(0)).magnitude;
        scale.y = ((Vector3)matrix.GetColumn(1)).magnitude;
        scale.z = ((Vector3)matrix.GetColumn(2)).magnitude;
        return scale;
    }

    public static Matrix4x4 Convert3DSMatrixToUnity(this Matrix4x4 matrix){
        Matrix4x4 identityConversion = new Matrix4x4(new Vector4(-1,0),new Vector4(0,0,1),new Vector4(0,-1),new Vector4(0,0,0,-1));
        Matrix4x4 topRowNegate = new Matrix4x4(new Vector4(0,0,0,-1),new Vector4(0,0,1),new Vector4(0,1),new Vector4(1,0));
        Matrix4x4 rowReverse= new Matrix4x4(new Vector4(0,0,0,1),new Vector4(0,0,1),new Vector4(0,1),new Vector4(1,0));

        matrix=matrix*identityConversion;
        matrix=topRowNegate*matrix;
        matrix=rowReverse*matrix;
        //rowReverse*topRowNegate*matrix*identityConversion
        //Unity Saftey Check.
        matrix.m33=1;
        return matrix.transpose;
    }
    public static float[] ToArray(this Color c){
        float[] ret = new float[4];
        ret[0]=c.r;
        ret[1]=c.g;
        ret[2]=c.b;
        ret[3]=c.a;
        return ret;
    }
    public static void FlipNormals(this Mesh mesh){
        for(int i=0;i<mesh.subMeshCount;i++){
            int[] tris=mesh.GetTriangles(i);
            for(int t=0;t<tris.Length;t+=3){
                int temp = tris[t];
                tris[t]=tris[t+2];
                tris[t+2]=temp;
            }
            mesh.SetTriangles(tris,i);
        }
    }
    public static Matrix4x4 convertRHStoLHS( this Matrix4x4 model ) {
        return new Matrix4x4(model.GetRow(0),model.GetRow(2),model.GetRow(1),model.GetRow(3));
    /* mat 4 newModel;
    newModel.x = model.x;   // Where X is X-Axis
    newModel.y = model.z;   //       Y is Y-Axis & Z is Z-Axis
    newModel.z = -model.y;  //       Z is Z-Axis & Y is Y-Axis

    return newModel; */
}

    public static bool Approx(this Vector3 v1, Vector3 v2){
        return Mathf.Approximately(v1.x,v2.x)&&Mathf.Approximately(v1.y,v2.y)&&Mathf.Approximately(v1.z,v2.z);
    }
    public static bool Approx(this Quaternion q1, Quaternion q2){
        return (q1.Equals(q2) || (q1 == q2));
    }

    public static SkinnedMeshRenderer MergeRiggedMeshes(SkinnedMeshRenderer smr1, SkinnedMeshRenderer smr2){
        Mesh nmesh = new Mesh();
        //List<Vector3>
        return null;
    }

   

    public static Mesh ExtractSubmesh(this Mesh m, int submeshIndex,bool collapseVertices=true){
        Mesh submesh = new Mesh();
        submesh.name=m.name+"_"+submeshIndex;
        int[] tris=m.GetTriangles(submeshIndex);
        int[] remap = new int[m.vertexCount];
        for(int i=0;i<remap.Length;i++){remap[i]=-1;}
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        Vector3[] vertices = m.vertices;
        Vector2[] uvcoords = m.uv;
        //add verts, check remap if necessary
        //redo tris list
        for(int i=0;i<tris.Length;i++){
            if(remap[tris[i]]==-1){
                remap[tris[i]]=verts.Count;
                verts.Add(vertices[tris[i]]);
                uvs.Add(uvcoords[tris[i]]);
            }
            tris[i]=remap[tris[i]];
        }
        submesh.vertices=verts.ToArray();
        submesh.uv=uvs.ToArray();
        submesh.SetTriangles(tris,0);
        submesh.RecalculateNormals();
        
        return submesh;
    }
}
}