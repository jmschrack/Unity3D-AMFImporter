using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
namespace AdjutantSharp{
    [System.Serializable]
    public class AMF 
    {
        public string header;
        public float version;
        public string modelName;
        [SerializeField]
        public List<AMF_MarkerGroup> markerGroups;
        public List<AMF_Node> nodes;

        public List<ShaderInfo> shaderInfos;
        public List<AMF_RegionInfo> regionInfo;
    }
    [System.Serializable]
    public struct AMF_RegionInfo{
        public string name;
        public List<AMF_Permutations> permutations;
        public AMF_RegionInfo(string name,List<AMF_Permutations> permutations){
            this.name=name;
            this.permutations=permutations;
        }
    }
    [System.Serializable]
    public struct AMF_Node
            {
                public string name;
                public short parentIndex;
                public short childIndex;
                public short siblingIndex;
                
                
                
                public Vector3 pos;
                public Quaternion rot;

                public AMF_Node(string name, short parentIndex, short childIndex, short siblingIndex, Vector3 pos, Quaternion rot) : this()
                {
                    this.name = name;
                    this.parentIndex = parentIndex;
                    this.childIndex = childIndex;
                    this.siblingIndex = siblingIndex;
                    this.pos = pos;
                    this.rot = rot;
                }

                public AMF_Node(BinaryReader reader,int index){
                    this.name = index.ToString().PadLeft(3, '0') +reader.ReadCString();
                    this.parentIndex = reader.ReadInt16();
                    this.childIndex = reader.ReadInt16();
                    this.siblingIndex = reader.ReadInt16();
                    this.pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    this.rot = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                }
            }

            /* public struct RGBA
            {
                public byte r,g,b,a;
                public RGBA(BinaryReader reader)
                {
                    r = reader.ReadByte();
                    g = reader.ReadByte();
                    b = reader.ReadByte();
                    a = reader.ReadByte();

                }
                public override string ToString()
                {
                    return "(" + r + "," + g + "," + b + "," + a + ")";
                }
                public Color32 ToColor32(){
                    return new Color32(r,g,b,a);
                }
                
            } */
    [System.Serializable]
    public struct AMF_MarkerGroup{
        public string name;
        public List<AMF_Marker> markers;
    }
    [System.Serializable]
    public struct AMF_Marker
    {
        public byte regionIndex;
        public byte permutationIndex;
        public short nodeIndex;
        
        public Vector3 position;
        public Quaternion orientation;
        public AMF_Marker(BinaryReader reader)
        {
            this.regionIndex = reader.ReadByte();
            this.permutationIndex = reader.ReadByte();
            this.nodeIndex = reader.ReadInt16();
            this.position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            this.orientation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),reader.ReadSingle());
        }

    }
    [System.Serializable]
     public class ShaderInfo
        {
            public string sName;
        }
        public class RegularShader : ShaderInfo
        {
            
            public List<string> paths;
            public List<Vector2> uvTiles;
            public bool isTransparent;
            public bool ccOnly;
            public List<Color32> tints;
            

            public RegularShader(string name,BinaryReader reader,bool parseTints,bool debug=false)
            {
                this.sName = name;
                paths = new List<string>();
                uvTiles = new List<Vector2>();
                tints = new List<Color32>();
                //copy uv tiles
                for (int j = 0; j < 8; j++)
                {
                    paths.Add(reader.ReadCString().Trim());
                    if (!paths[j].Equals("null"))
                    {
                        uvTiles.Add(new Vector2(reader.ReadSingle(), reader.ReadSingle()));

                    }
                    else
                    {
                        uvTiles.Add(Vector2.zero);
                    }
                    if (debug)
                    {
                        Console.WriteLine("+path:" + paths[j]+" | ["+uvTiles[j].x+","+uvTiles[j].y+"]");
                    }
                }
                //copy tints
                for (int j = 0; j < 4; j++)
                {
                    if (parseTints)
                    {
                        tints.Add(reader.ReadColor32());
                    }
                    else
                    {
                        tints.Add(reader.ReadColor32());
                    }

                }

                isTransparent = reader.ReadBoolean();
                ccOnly = reader.ReadBoolean();
            }


            public bool Equals(RegularShader s)
            {
                if (!this.sName.Equals(s.sName))
                    return false;
                if (paths.Count != s.paths.Count)
                    return false;
                for(int i = 0; i < paths.Count; i++)
                {
                    if (!paths[i].Equals(s.paths[i]))
                        return false;
                }

                return true;
            }
        }
        [System.Serializable]
        public class TerrainShader : ShaderInfo
        {
            public string blendPath;
            public Vector2 blendTile;

            public List<string> baseMaps;
            public List<Vector2> baseTiles;

            public List<string> bumpMaps;
            public List<Vector2> bumpTiles;

            public List<string> detMaps;
            public List<Vector2> detTiles;

            public TerrainShader(string name,BinaryReader reader,bool debug=false)
            {
                this.sName = name;
                baseMaps = new List<string>();
                bumpMaps = new List<string>();
                detMaps = new List<string>();

                baseTiles = new List<Vector2>();
                bumpTiles = new List<Vector2>();
                detTiles = new List<Vector2>();

                blendPath = reader.ReadCString();
                if (!blendPath.Equals("null"))
                {
                    blendTile = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                }
                else
                {
                    blendTile = Vector2.zero;
                }
                sbyte baseCount = reader.ReadSByte();
                sbyte bumpCount = reader.ReadSByte();
                sbyte detCount = reader.ReadSByte();
                for(int i=0; i < baseCount; i++)
                {
                    baseMaps.Add(reader.ReadCString());
                    baseTiles.Add(new Vector2(reader.ReadSingle(), reader.ReadSingle()));
                }
                for(int i = 0; i < bumpCount; i++)
                {
                    bumpMaps.Add(reader.ReadCString());
                    bumpTiles.Add(new Vector2(reader.ReadSingle(), reader.ReadSingle()));
                }
                for(int i = 0; i < detCount; i++)
                {
                    detMaps.Add(reader.ReadCString());
                    detTiles.Add(new Vector2(reader.ReadSingle(), reader.ReadSingle()));
                }
            }
        }

         //AKA submesh
        public struct AMF_Mesh
        {
            public short shaderIndex;
            public int startingFace;
            public int faceCount;
            public AMF_Mesh(short shaderIndex,int startingFace,int faceCount)
            {
                this.shaderIndex = shaderIndex;
                this.startingFace = startingFace;
                this.faceCount = faceCount;
            }
            public AMF_Mesh(BinaryReader reader)
            {
                shaderIndex = reader.ReadInt16();//+1 pretty sure 3ds max counts at 1
                startingFace = reader.ReadInt32();//+1
                faceCount = reader.ReadInt32();
                
            }
        }
        public struct AMF_Vertex
        {
            public Vector3 pos;
            public Vector3 norm;
            public Vector3 tex;
            public List<int> indices;
            public List<float> weights;
            public Matrix4x4 tmat;

            public AMF_Vertex(BinaryReader reader, Matrix4x4 tmat,int vFormat, int cFormat)
            {
                this.tmat=tmat;
                if (cFormat == 1)
                {
                    Debug.LogError("Spacer data encountered?");
                    pos = reader.ReadVector3Short();
                    norm = Vector3.zero;
                    //spacer data it looks like
                    reader.ReadInt32();
                    
                    tex = tmat.MultiplyPoint(new Vector3(reader.ReadInt16(), reader.ReadInt16(), 0));
                }
                else
                {
                    pos = reader.ReadVector3();
                    norm = reader.ReadVector3();
                    tex =new Vector3(reader.ReadSingle(), reader.ReadSingle(),0f);
                }
                //list indices
                indices = new List<int>();
                //list weights
                weights = new List<float>();
                if (vFormat == 1)
                {
                    //starting counting at 1? hissssssss...
                    int iCount = 1;
                    int i1 = (reader.ReadByte()) + 1;
                    int i2 = (reader.ReadByte()) + 1;
                    int i3 = 0, i4 = 0;
                    if (i2 != 256)
                    {
                        iCount++;
                        i3 = (reader.ReadByte()) + 1;
                        if (i3 != 256)
                        {
                            iCount++;
                            i4 = (reader.ReadByte()) + 1;
                            if (i4 != 256)
                            {
                                iCount++;
                            }
                        }
                    }
                    float w1 = reader.ReadSingle();
                    indices.Add(i1);
                    weights.Add(w1);
                    if (iCount > 1)
                    {
                        indices.Add(i2);
                        weights.Add(reader.ReadSingle());
                    }
                    if (iCount > 2)
                    {
                        indices.Add(i3);
                        weights.Add(reader.ReadSingle());
                    }
                    if (iCount > 3)
                    {
                        indices.Add(i4);
                        weights.Add(reader.ReadSingle());
                    }
                }

                if (vFormat == 2)
                {
                    int i1 = (reader.ReadByte()) + 1;
                    indices.Add(i1);
                    weights.Add(1.0f);
                    int i2 = (reader.ReadByte()) + 1;
                    if (i2 != 256)
                    {
                        indices.Add(i2);
                        weights.Add(1.0f);
                        int i3 = (reader.ReadByte()) + 1;
                        if (i3 != 256)
                        {
                            indices.Add(i3);
                            weights.Add(1.0f);
                            int i4 = (reader.ReadByte()) + 1;
                            if (i4 != 256)
                            {
                                indices.Add(i4);
                                weights.Add(1.0f);
                            }
                        }
                    }
                }
            }
            public bool HasBoneWeight(){
                return weights.Count>0;
            }
            public BoneWeight GetBoneWeight(){
                BoneWeight bw = new BoneWeight();
                for(int b = 0;b<weights.Count;b++){
                    switch(b){
                        case 0:
                            bw.weight0=weights[b];
                            bw.boneIndex0=indices[b];
                        break;
                        case 1:
                            bw.weight1=weights[b];
                            bw.boneIndex1=indices[b];
                        break;
                        case 2:
                            bw.weight2=weights[b];
                            bw.boneIndex2=indices[b];
                        break;
                        case 3:
                            bw.weight3=weights[b];
                            bw.boneIndex3=indices[b];
                        break;
                    }
                }
                return bw;
            }
        }
        [System.Serializable]
        public struct AMF_Permutations
        {
            //public string name;
            //public short vertexFormat;
            //public short compressionFormat;
            //public byte nodeIndex;
            //vertices header
            //faces header
            //sections header
            public float mult;
            public string pName;
            public int vFormat;
            public byte nIndex;
            public List<AMF_Vertex> vertices;
            public List<Vector3Int> faces;
            public List<AMF_Mesh> meshes;
            public long vAddress;
            public long fAddress;
            public Matrix4x4 matrix4x4;
            public List<Vector2> bounds;
            public int cFormat;

            public AMF_Permutations(string pName, int vFormat, byte nIndex, List<AMF_Vertex> vertices, List<Vector3Int> faces, List<AMF_Mesh> meshes, long vAddress, long fAddress, float mult, Matrix4x4 matrix4x4,List<Vector2>  bounds,int cFormat) : this()
            {
                this.pName = pName;
                this.vFormat = vFormat;
                this.nIndex = nIndex;
                this.vertices = vertices;
                this.faces = faces;
                this.meshes = meshes;
                this.vAddress = vAddress;
                this.fAddress = fAddress;
                this.mult = mult;
                this.matrix4x4 = matrix4x4;
                this.bounds=bounds;
                this.cFormat=cFormat;
            }

            public void DebugCheck(){
                int submeshTotal=0;
                Debug.Log("Checking Mesh:"+pName);
                for(int i=0;i<meshes.Count;i++){
                    submeshTotal+=meshes[i].faceCount;
                    if(meshes[i].faceCount%3!=0){
                        Debug.LogErrorFormat("Bad Submesh [{0}]:{1} Index:{2}",i,meshes[i].faceCount,meshes[i].startingFace);
                    }
                }
                if(submeshTotal!=faces.Count){
                    Debug.LogErrorFormat("Submeshes don't add up! {0} vs {1}",submeshTotal,faces.Count);
                }
                if(faces.Count%3!=0){
                    Debug.LogErrorFormat("Invalid total tris:{0}",faces.Count);
                }else{
                    Debug.LogFormat("Correct Total tris:{0}",faces.Count);
                }
                
            }
            //transformation matrix
        }
}
