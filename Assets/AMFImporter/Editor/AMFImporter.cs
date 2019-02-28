using UnityEditor.Experimental.AssetImporters;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using AdjutantSharp;
[ScriptedImporter(2,"amf")]
public class AMFImporter : ScriptedImporter
{
    readonly string RegularShaderName="Standard";
    public float globalScale=1f;
    public string m_FileScaleUnit="in.";
    public bool m_UseFileScale=false;
    public float m_FileScaleFactor=0.0254f;
    public bool mergeSkinnedMeshes=false;
    public ModelImporterNormals normalImportMode;
    //Model Rigging
    public Vector3 rigOffset = Vector3.up;
    public Vector3 rigEulerRoot= new Vector3(0,-90,-90);
    public enum RigType{
        None=0,
        Generic=1,
        Humanoid=2
    }
    public RigType rigType=RigType.None;
    public bool copyAvatar=false;
    public Avatar m_LastHumanDescriptionAvatarSource;
    public bool createDuplicateInstances;

    public bool GenerateLightmapUVs=false;
    public bool CreateSkinnedMeshes{
        get{return rigType!=RigType.None;}
    }
    
    public bool GenerateMeshCollidersOnClusters;

    UnwrapParam uvSettings = new UnwrapParam();
    public float angleError=8f;
    public float areaError=15f;
    public float hardAngle=88f;
    public float packMargin=4f;
    
    public override void OnImportAsset(AssetImportContext ctx){
        Debug.Log("Attempting to import AMF:"+ctx.assetPath);
        
        EditorUtility.DisplayProgressBar("Parsing"+ctx.assetPath,"Parsing...",0);
        AMF amf=ParseAMF( ctx.assetPath);
        
        string workingDir=ctx.assetPath.Substring(0,ctx.assetPath.LastIndexOf("/")+1);
        /*
        Setup materials first
         */
        Dictionary<string ,Material > mats = new Dictionary<string, Material>();
        Dictionary<string,AMFShaderInfo> matsHelpers = new Dictionary<string,AMFShaderInfo>();
        System.IO.Directory.CreateDirectory(workingDir+"Materials/");
        //System.IO.Directory.CreateDirectory(workingDir+"MaterialHelpers/");
        AMFShaderInfo asi;
        float totalMats=amf.shaderInfos.Count;
        float matsComplete=0;
        foreach(AdjutantSharp.ShaderInfo si in amf.shaderInfos){
            EditorUtility.DisplayProgressBar("Setting up Materials...",si.sName,(matsComplete/totalMats));
            asi = (AMFShaderInfo)AMFShaderInfo.CreateInstance(typeof(AMFShaderInfo));
            asi.name=si.sName;
            asi.SaveData(si);
            if(!mats.ContainsKey(si.sName)){
                string path=workingDir+"Materials/"+si.sName+".mat";
                Material material=(Material)AssetDatabase.LoadAssetAtPath(workingDir+"Materials/"+si.sName+".mat",typeof(Material));
                
                if(material==null){
                    asi.workingDir=workingDir;
                    material=asi.CreateMaterial();
                    /* if(si.GetType()==typeof(RegularShader)){
                        material=SetupRegularMaterial((RegularShader)si,workingDir);
                    }else{
                        material=SetupTerrainMaterial((TerrainShader)si,workingDir);
                    } */
                    
                    AssetDatabase.CreateAsset(material,workingDir+"Materials/"+si.sName+".mat");
                }
                
                
                
                mats.Add(si.sName,material);
                matsHelpers.Add(si.sName,asi);
                ctx.AddObjectToAsset("MaterialHelper-"+asi.sName,asi);
                ctx.DependsOnSourceAsset(workingDir+"Materials/"+si.sName+".mat");
                /* if(material!=null)
                    ctx.AddObjectToAsset(material.name,material); */
            }
            matsComplete++;
        }
        
        /*
        Create Meshes
        */
        
        GameObject root = new GameObject(amf.modelName);
        ctx.AddObjectToAsset(amf.modelName,root);
        ctx.SetMainObject(root);
        Dictionary<long,Mesh> meshList =ConvertMeshes(amf,mats,matsHelpers,root);
        EditorUtility.DisplayProgressBar("Parsing "+ctx.assetPath,"[5/5] Finishing up...",(5f/5f));
        foreach(Mesh m in meshList.Values){
            ctx.AddObjectToAsset(m.name,m);
        } 
        if(CreateSkinnedMeshes){
            Avatar a = root.GetComponent<Animator>().avatar;
            ctx.AddObjectToAsset(a.name,a);
        }
        
        
        Debug.Log("AMF import complete");
        EditorUtility.ClearProgressBar();
    }
    /*
    
    Convert Meshes
     */
    public Dictionary<long,Mesh> ConvertMeshes(AMF amf,Dictionary<string,Material> mats,Dictionary<string,AMFShaderInfo> matHelpers,GameObject root){
        //List<Mesh> meshList = new List<Mesh>();
        Dictionary<long,Mesh> meshCache = new Dictionary<long,Mesh>();
        float meshComplete=0;
        float totalMeshCount=0;

        List<Transform> nodes=null;
        if(CreateSkinnedMeshes){
            nodes=CreateRigging(amf);
            nodes[0].parent=root.transform;
            Animator anim = root.AddComponent<Animator>();
            //Transform rigRoot=root.GetComponentInChildren<SkinnedMeshRenderer>().rootBone;
            if(copyAvatar){
                //ctx.AddObjectToAsset(m_LastHumanDescriptionAvatarSource.name,m_LastHumanDescriptionAvatarSource);
                anim.avatar=m_LastHumanDescriptionAvatarSource;
            }else{
                //EditorUtility.DisplayProgressBar("Parsing "+ctx.assetPath,"Creating Avatar",(5f/5f));
                List<string> reports = new List<string>();
                HumanDescription hd = new HumanDescription();
                AvatarSetupTool.SkeletonBone[] skeletonBones;
                bool hasTranslationDOF;
                reports=AvatarSetupTool.SetupHumanSkeleton(nodes[0].parent.gameObject,ref hd.human,out skeletonBones,out hasTranslationDOF);
                hd.hasTranslationDoF=hasTranslationDOF;
                SkeletonBone[] sb = new SkeletonBone[skeletonBones.Length+1];
                Array.Copy(Array.ConvertAll(skeletonBones,(p => (SkeletonBone) p)),sb,sb.Length-1);
                sb[sb.Length-1].name=nodes[0].parent.name;
                sb[sb.Length-1].position=nodes[0].parent.localPosition;
                sb[sb.Length-1].rotation=nodes[0].parent.localRotation;
                sb[sb.Length-1].scale=nodes[0].parent.localScale;
                hd.skeleton=sb;
                Avatar a;
                if(rigType==RigType.Humanoid)
                    a=AvatarBuilder.BuildHumanAvatar(nodes[0].parent.gameObject,hd);
                else
                    a=AvatarBuilder.BuildGenericAvatar(nodes[0].parent.gameObject,nodes[0].parent.name);
                a.name=root.name+"Avatar";
                //ctx.AddObjectToAsset(a.name,a);
                anim.avatar=a;
            }
            
        }


        foreach(AMF_RegionInfo ri in amf.regionInfo)
            totalMeshCount+=ri.permutations.Count;

        for(int ri=0;ri<amf.regionInfo.Count;ri++){
            GameObject riNode = new GameObject(amf.regionInfo[ri].name);
            GameObjectUtility.SetParentAndAlign(riNode,root);
            foreach(AMF_Permutations perm in amf.regionInfo[ri].permutations){
                EditorUtility.DisplayProgressBar("Creating Meshes",perm.pName,(meshComplete/totalMeshCount));
                Mesh temp;
                if(createDuplicateInstances||!meshCache.ContainsKey(perm.vAddress)){
                    temp = ConvertInstanceToMesh(perm);
                    //we have to flip the normals due to the coordinate system translation
                    temp.FlipNormals();
                    temp.RecalculateNormals();
                    if(GenerateLightmapUVs){
                        EditorUtility.DisplayProgressBar("Generating Lightmap UVs",perm.pName,(meshComplete/totalMeshCount));
                        uvSettings.angleError=angleError;
                        uvSettings.areaError=areaError;
                        uvSettings.hardAngle=hardAngle;
                        uvSettings.packMargin=packMargin;
                        Unwrapping.GenerateSecondaryUVSet(temp,uvSettings);
                    }
                    meshCache.Add(perm.vAddress,temp);
                    
                }else{
                    temp=meshCache[perm.vAddress];
                }
                GameObject meshNode = new GameObject(perm.pName);
                Matrix4x4 matr = Matrix4x4.identity;
                if(!float.IsNaN(perm.mult)){
                    Matrix4x4 scalerM = new Matrix4x4();
                    scalerM.SetRow(0,new Vector4(100f*m_FileScaleFactor,0));
                    scalerM.SetRow(1,new Vector4(0,100f*m_FileScaleFactor));
                    scalerM.SetRow(2,new Vector4(0,0,100f*m_FileScaleFactor));
                    scalerM.SetRow(3,new Vector4(0,0,0,1));
                    matr.SetRow(0,new Vector4(perm.mult,0));
                    matr.SetRow(1,new Vector4(0,perm.mult));
                    matr.SetRow(2,new Vector4(0,0,perm.mult));
                    matr.SetRow(3,new Vector4(0,0,0,1));
                    matr*=perm.matrix4x4;
                    matr*=scalerM;
                    Matrix4x4 unityMatr=matr.Convert3DSMatrixToUnity();
                    meshNode.transform.localScale=unityMatr.ExtractScale();
                    meshNode.transform.localRotation=unityMatr.GetRotation();
                    meshNode.transform.localPosition=unityMatr.ExtractPosition();
                }else{
                    meshNode.transform.localScale=new Vector3(m_FileScaleFactor,m_FileScaleFactor,m_FileScaleFactor);
                }
                
                //GameObjectUtility.SetParentAndAlign(meshNode,riNode);
                
                //meshNode.transform.localToWorldMatrix=matr;
                
                
                Renderer mr;
                if(temp.boneWeights.Length>0&&CreateSkinnedMeshes){
                    mr=meshNode.AddComponent<SkinnedMeshRenderer>();
                    meshNode.transform.localRotation=Quaternion.Euler(0,90,0);
                    Matrix4x4[] bindPoses = new Matrix4x4[nodes.Count];
                    for(int m =0;m<bindPoses.Length;m++){
                        bindPoses[m]=nodes[m].worldToLocalMatrix*meshNode.transform.localToWorldMatrix;
                    }
                    temp.bindposes=bindPoses;
                    ((SkinnedMeshRenderer)mr).sharedMesh=temp;
                    ((SkinnedMeshRenderer)mr).bones=nodes.ToArray();
                    ((SkinnedMeshRenderer)mr).rootBone=nodes[0];

                }else{
                    MeshFilter mf =meshNode.AddComponent<MeshFilter>();
                    mf.sharedMesh=temp;
                    mr = meshNode.AddComponent<MeshRenderer>();
                }
                
                
                meshNode.transform.parent=riNode.transform;
                if(GenerateMeshCollidersOnClusters&&amf.regionInfo[ri].name.Equals("Clusters")){
                    MeshCollider mc = meshNode.AddComponent<MeshCollider>();
                    mc.sharedMesh=temp;
                }
                Material[] materials = new Material[temp.subMeshCount];
                List<AMFShaderInfo> si = new List<AMFShaderInfo>();
                for(int i=0;i<materials.Length;i++){
                    materials[i]=mats[amf.shaderInfos[perm.meshes[i].shaderIndex].sName];
                    si.Add(matHelpers[amf.shaderInfos[perm.meshes[i].shaderIndex].sName]) ;
                    //si[i].SaveData(amf.shaderInfos[perm.meshes[i].shaderIndex]);
                    
                }
                mr.sharedMaterials=materials;
                AMFMaterialHelper mh = meshNode.AddComponent<AMFMaterialHelper>();
                mh.shaderSettings=si;
                
                //mh.SaveData(amf.shaderInfos[perm.meshes[0].shaderIndex]);
                //Debug.LogFormat("Transform: Pos:{0} Rot:{1} Scale:{2}",perm.matrix4x4.ExtractPosition(),perm.matrix4x4.ExtractRotation(),perm.matrix4x4.ExtractScale());
                meshComplete++;
            }
        }
        
        return meshCache;
    }

    Mesh ConvertInstanceToMesh(AMF_Permutations perm){
        Mesh temp= new Mesh();
        temp.name=perm.pName;
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs=new List<Vector2>();
        //List<int> badIndex = new List<int>();
        //int[] identity=new int[perm.vertices.Count];
        List<BoneWeight> bones = new List<BoneWeight>();

        //Matrix4x4 texMatrix=Matrix4x4.identity;
        for(int i=0;i<perm.vertices.Count;i++){
            Vector3 pos = perm.vertices[i].pos;
            Matrix4x4 flip=Matrix4x4.identity;
            flip.SetRow(0,new Vector4(-1,0));
            if(float.IsNaN(perm.mult)){
                flip.SetRow(1,new Vector4(0,0,1));
                flip.SetRow(2,new Vector4(0,-1));
            }
            verts.Add(flip.MultiplyPoint3x4(pos));
            uvs.Add(perm.vertices[i].tex);
            //texMatrix=perm.vertices[i].tmat;
            if(perm.vertices[i].HasBoneWeight()){
                bones.Add(perm.vertices[i].GetBoneWeight());
            }
        }
        temp.SetVertices(verts);
        temp.SetUVs(0,uvs);
        temp.boneWeights=bones.ToArray();
        //Triangles
        List<int> tris;
        int faceTotal=0;
        temp.subMeshCount=perm.meshes.Count;
        
        for(int s=0;s<perm.meshes.Count;s++){
            AMF_Mesh sinfo=perm.meshes[s];
            faceTotal+=sinfo.faceCount;
            tris=new List<int>();
            for(int f=0;f<sinfo.faceCount;f++){
                Vector3Int face=perm.faces[f+sinfo.startingFace];
                tris.Add(face.x);
                tris.Add(face.y);
                tris.Add(face.z);
            }
            temp.SetTriangles(tris,s);
        }

        return temp;

    }
    /*
    
    Create Bone Hierarchy
     */
    List<Transform> CreateRigging(AMF amf){
        List<Transform> bones= new List<Transform>();
        
        //first past to instantiate references
        for(int i=0;i<amf.nodes.Count;i++){
            bones.Add(new GameObject(amf.nodes[i].name).transform);
        }
        
        //second pass to setup hierarchy
        for(int i=0;i<amf.nodes.Count;i++){
            if(amf.nodes[i].parentIndex>-1){
                bones[i].parent=bones[amf.nodes[i].parentIndex];
                Matrix4x4 flip=Matrix4x4.identity;
                flip.SetRow(0,new Vector4(-1,0));
                bones[i].localPosition=flip.MultiplyPoint3x4(amf.nodes[i].pos);//new Vector3(amf.nodes[i].pos.x,amf.nodes[i].pos.y,-amf.nodes[i].pos.z);
                bones[i].localRotation=new Quaternion(amf.nodes[i].rot.x,-amf.nodes[i].rot.y,-amf.nodes[i].rot.z,amf.nodes[i].rot.w);//amf.nodes[i].rot;//
            }
        }
        //third pass to reorder siblings.
        for(int i=0;i<amf.nodes.Count;i++){
            if(amf.nodes[i].siblingIndex>-1){
                bones[i].parent.SetSiblingIndex(amf.nodes[i].siblingIndex);
            }
        }
        bones.Insert(0,new GameObject("Root").transform);
        bones[1].parent=bones[0];
        bones[0].localScale=new Vector3(m_FileScaleFactor*globalScale,m_FileScaleFactor*globalScale,m_FileScaleFactor*globalScale);
        bones[0].localRotation=Quaternion.Euler(rigEulerRoot);//0,-90f,-90f);
        bones[0].localPosition=rigOffset;
        //bones[0].localPosition=Vector3.up;
        return bones;
    }

    


/*
-------------
Big ol parser code. 
Maybe I should split it out? Probably not worth the effort
Translated from the original MaxScript import
--------------
 */




    
        public static AMF ParseAMF(string path)
        {
            AMF amf = new AMF();
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {

                Debug.Log("File length:" + reader.BaseStream.Length);

                readHeader(reader, amf);
                Debug.Log(amf.header + " " + amf.version + " " + amf.modelName);
                //tvRegions.Nodes.Clear()

                EditorUtility.DisplayProgressBar("Parsing "+path,"[0/5] Parsing nodes...",0);
                readNodes(reader, amf);
                EditorUtility.DisplayProgressBar("Parsing "+path,"[1/5] Parsing Markers...",(1f/5f));
                readMarkers(reader, amf);
                EditorUtility.DisplayProgressBar("Parsing "+path,"[2/5] Parsing Regions...",(2f/5f));
                readRegions(reader, amf);
                EditorUtility.DisplayProgressBar("Parsing "+path,"[3/5] Parsing Shaders...",(3f/5f));
                readShaders(reader, amf);

                //loadRegions(tvRegions)
            }
            return amf;
        }

        static void readHeader(BinaryReader reader,AMF amf)
        {
            amf.header = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(4));
            amf.version = reader.ReadSingle();
            amf.modelName = reader.ReadCString();
        }
        static void readNodes(BinaryReader reader, AMF amf,bool skip=false)
        {
            int count = reader.ReadInt32();
            Debug.Log("Node count:" + count);
            long address = (long)reader.ReadInt32();
            long fPos = reader.BaseStream.Position;
            List<AMF_Node> nodes = new List<AMF_Node>();
            if (count > 0&&!skip)
            {
                reader.BaseStream.Seek(address, SeekOrigin.Begin);
                for (int i = 0; i < count; i++)
                {
                    nodes.Add(new AMF_Node(reader,i));
                }
                reader.BaseStream.Seek(fPos, SeekOrigin.Begin);
            }
            amf.nodes=nodes;
        }
        static void readMarkers(BinaryReader reader, AMF amf,bool skip=false)
        {
            int groupCount = reader.ReadInt32();
            Debug.Log("Groups Of Markers:[" + groupCount + "]");
            long groupAddress = reader.ReadInt32();
            long fPos = reader.BaseStream.Position;
            List<List<AMF_Marker>> markerGroup = new List<List<AMF_Marker>>();
            if (groupCount > 0&&!skip)
            {
                reader.BaseStream.Seek(groupAddress, SeekOrigin.Begin);
                for(int i = 0; i < groupCount; i++)
                {
                    string groupName = "#" + reader.ReadCString();
                    long markerCount = reader.ReadInt32();
                    long markerAddress = reader.ReadInt32();
                    long cPos = reader.BaseStream.Position;
                    List<AMF_Marker> markers = new List<AMF_Marker>();
                    if (markerCount > 0)
                    {
                        reader.BaseStream.Seek(markerAddress, SeekOrigin.Begin);
                        for(int j = 0; j < markerCount; j++)
                        {
                            markers.Add(new AMF_Marker(reader));
                        }
                        
                    }
                    markerGroup.Add(markers);
                    reader.BaseStream.Seek(cPos, SeekOrigin.Begin);
                }
                reader.BaseStream.Seek(fPos, SeekOrigin.Begin);
            }
            
        }
        static void readRegions(BinaryReader reader, AMF amf,bool skip=false)
        {
            int regionCount = reader.ReadInt32();
            Debug.Log("Region Count:" + regionCount);
            long regionAddress = reader.ReadInt32();
            long fPos = reader.BaseStream.Position;
            amf.regionInfo=  new List<AMF_RegionInfo>();
            //List <List<AMF_Permutations>> 
            if (regionCount > 0&&!skip)
            {
                reader.BaseStream.Seek(regionAddress, SeekOrigin.Begin);
                for(int i = 0; i < regionCount; i++)
                {
                    List<AMF_Permutations> permutations = new List<AMF_Permutations>();
                    string regionName = reader.ReadCString();
                    int pCount = reader.ReadInt32();
                    long pAddress = reader.ReadInt32();
                    long pPos = reader.BaseStream.Position;
                    if (pCount > 0)
                    {
                        reader.BaseStream.Seek(pAddress, SeekOrigin.Begin);
                        for(int j = 0; j < pCount; j++)
                        {
                            string pName = reader.ReadCString();
                            
                            sbyte vTemp = reader.ReadSByte();
                            byte nIndex = reader.ReadByte();
                            int vCount = reader.ReadInt32();
                            long vAddress = reader.ReadInt32();
                            int fCount = reader.ReadInt32();
                            long fAddress = reader.ReadInt32();
                            int sCount = reader.ReadInt32();
                            long sAddress = reader.ReadInt32();
                            //Debug.LogFormat("vTemp {0}",vTemp.ToString());
                            int vFormat = vTemp & 15;
                            int cFormat = (vTemp & 240) >> 4;
                            float mult=0f;
                            //Debug.LogFormat("Parse Permutation: {0} - {1}v-{2}f-{3}s",pName,vCount,fCount,sCount);

                            //transform = matrix3 1      ???
                            Matrix4x4 trnsfm = Matrix4x4.identity;
                            
                            if (amf.version > 0.1)
                            {
                                mult = reader.ReadSingle();
                                //NANI?!
                                if (!float.IsNaN(mult))
                                {
                                    trnsfm=reader.ReadMatrix4x4(true);//new Matrix4x4(reader.ReadVector4(0f),reader.ReadVector4(0f),reader.ReadVector4(0f),reader.ReadVector4(0f));
                                    //transform = new Matrix4x4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 1.0f, reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 1.0f, reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 1.0f, reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 1.0f);
                                    //3DS uses a 4x3 so we need to pad an extra column for the identity
                                    trnsfm.m33=1;
                                }
                            }
                            long xPos = reader.BaseStream.Position;
                            bool vDone = false;
                            bool fDone = false;
                            List<AMF_Vertex> vertices = new List<AMF_Vertex>();
                            Matrix4x4 vMat = Matrix4x4.identity;
                            List<Vector3Int> faces=null;
                            foreach(AMF_Permutations permX in permutations)
                            {
                                //do some weird copy verts thing
                                if (permX.vAddress == vAddress && !vDone)
                                {
                                    //not really a copy, it's actually just caclulating vMat
                                    //but I'm trying to preserve the original script as much
                                    //as possible before refactoring
                                    vMat = copyVertices(reader,permX.vertices, cFormat);
                                    vertices=permX.vertices;
                                    vDone = true;
                                }
                                if (permX.fAddress == fAddress && !fDone)
                                {
                                    faces=permX.faces;
                                    fDone = true;
                                }
                                if (vDone && fDone)
                                    break;

                            }
                            List<Vector2> bounds = new List<Vector2>();
                            if (!vDone)
                            {
                                reader.BaseStream.Seek(vAddress, SeekOrigin.Begin);
                                vMat=readVertices(reader, vFormat, cFormat, vCount, vertices,bounds);
                                //verts list = readVertices vFormat,cFormat,vCount

                            }
                            
                            if (!fDone)
                            {
                                reader.BaseStream.Seek(fAddress, SeekOrigin.Begin);
                                //faces list = readFaces vCount,fCount
                                faces = readFaces(reader, vCount, fCount);
                            }
                            reader.BaseStream.Seek(sAddress, SeekOrigin.Begin);
                            List<AMF_Mesh> meshes = readMeshes(reader, sCount);
                            /* Debug.LogFormat("vMat:{0}",vMat);
                            Debug.LogFormat("trnsfm:{0}",trnsfm); */
                            
                            //meshes list = readMeshes sCount
                            //vMat*trnsfm
                            permutations.Add(new AMF_Permutations(pName,vFormat,nIndex,vertices,faces,meshes,vAddress,fAddress,mult,vMat*trnsfm,bounds,cFormat));
                            
                            //permutations[j].DebugCheck();
                            reader.BaseStream.Seek(xPos, SeekOrigin.Begin);
                        }
                    }
                    

                    amf.regionInfo.Add(new AMF_RegionInfo(regionName,permutations));
                    reader.BaseStream.Seek(pPos, SeekOrigin.Begin);
                }
            }
            reader.BaseStream.Seek(fPos, SeekOrigin.Begin);
        }
        static void readShaders(BinaryReader reader, AMF amf, bool skip = false, bool dumpNames = false)
        {
            //ShaderInfo
            amf.shaderInfos = new List<AdjutantSharp.ShaderInfo>();
            //ShaderTypes
            List<int> shaderTypes = new List<int>();
            long sCount = reader.ReadInt32();
            Debug.Log("Shader Count:" + sCount);
            long sAddress = reader.ReadInt32();
            long fPos = reader.BaseStream.Position;
            int debugCount = 0;
            if (sCount > 0)
            {
                reader.BaseStream.Seek(sAddress, SeekOrigin.Begin);
                for(int i = 0; i < sCount; i++)
                {
                    string sName = reader.ReadCString();

                    
                    int sType = 0;
                    if (sName.Substring(0, 1).Equals("*"))
                    {
                        sType = 1;
                        sName = sName.Substring(1);
                    } 
                    if (dumpNames)
                    {
                        if (sType == 0)
                            Console.Write("Regular");
                        else
                            Console.Write("Terrain");
                        Debug.Log("Shader:" + sName+" tints:"+(amf.version>1.1f));
                    }
                    shaderTypes.Add(sType);

                    if (sType == 0)
                    {
                        amf.shaderInfos.Add(new RegularShader(sName,reader, amf.version>1.1f,(dumpNames&&debugCount<1)));
                        debugCount++;
                    }
                    else
                    {
                        amf.shaderInfos.Add(new TerrainShader(sName,reader));
                        if (dumpNames&&amf.shaderInfos[i].sName.Equals("cust_wall"))
                        {
                            TerrainShader ts = (TerrainShader)amf.shaderInfos[i];
                            Debug.Log("cust_wall");
                            for(int maps = 0; maps < ts.baseMaps.Count; maps++)
                            {
                                Debug.Log("[" + maps + "]:" + ts.baseMaps[maps]+" ["+ts.baseTiles[maps].x+","+ ts.baseTiles[maps].y+"] "+ts.detMaps[maps]+" "+ts.detTiles[maps].ToString());
                            }
                            Debug.Log("Blend tile:"+ts.blendPath+" "+ts.blendTile.ToString());
                        }
                    }
                }
            }
            reader.BaseStream.Seek(fPos, SeekOrigin.Begin);
        }

        public static List<Vector3Int> readFaces(BinaryReader reader,int vLen, int fCount)
        {
            List<Vector3Int> faces = new List<Vector3Int>();
            if (fCount > 0)
            {
                for(int f = 0; f < fCount; f++)
                {
                    if (vLen > 65535)
                    {
                        faces.Add(reader.ReadVector3Int());
                    }
                    else
                    {
                        
                        faces.Add(new Vector3Int(reader.ReadUInt16(),reader.ReadUInt16(),reader.ReadUInt16()));
                    }
                }
            }
            return faces;
        }

        public static Matrix4x4 readVertices(BinaryReader reader,int vFormat, int cFormat, int vCount, List<AMF_Vertex> vertices,List<Vector2> bounds)
        {
            //list verts
            Matrix4x4 vmat = Matrix4x4.identity;
            Matrix4x4 tmat = Matrix4x4.identity;
            
            //List<AMF_Vertex> vertices = new List<AMF_Vertex>();
            if (vCount > 0)
            {
                if (cFormat > 0)
                {
                    Vector2 xbounds = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    Vector2 ybounds = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    Vector2 zbounds = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    Vector2 ubounds = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    Vector2 vbounds = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    bounds.Add(xbounds);
                    bounds.Add(ybounds);
                    bounds.Add(zbounds);
                    bounds.Add(ubounds);
                    bounds.Add(vbounds);
                    //vmatrix
                    vmat = getVMatrix(xbounds, ybounds, zbounds);
                    tmat = getTMatrix (ubounds, vbounds);
                }
                for(int v = 0; v < vCount; v++)
                {
                    vertices.Add(new AMF_Vertex(reader, tmat,vFormat, cFormat));
                }
            }
            return vmat;
            //return vertices and vmat somehow
        }

       public static Matrix4x4 copyVertices(BinaryReader reader,List<AMF_Vertex> vFrom,int cFormat)
       {
            //verts cleared?
            Matrix4x4 vmat = Matrix4x4.identity;
            Matrix4x4 tmat = Matrix4x4.identity;
            if(vFrom.Count>0&&cFormat>0){
                Vector2 xbounds = reader.ReadVector2();
                Vector2 ybounds=reader.ReadVector2();
                Vector2 zbounds = reader.ReadVector2();
                Vector2 ubounds=reader.ReadVector2();
                Vector2 vbounds=reader.ReadVector2();
                vmat=getVMatrix(xbounds,ybounds,zbounds);
                tmat=getTMatrix(ubounds,vbounds);
            }
            return vmat;
       }

        public static List<AMF_Mesh> readMeshes(BinaryReader reader,int sCount)
        {
            //meshes
            List<AMF_Mesh> meshes = new List<AMF_Mesh>();
            string debugTotal="";
            bool error=false;
            if (sCount > 0)
            {
                for(int s = 0; s < sCount; s++)
                {
                    
                    meshes.Add(new AMF_Mesh(reader));
                    debugTotal+=meshes[s].faceCount+",";
                    /* if(meshes[s].faceCount%3!=0){
                        Debug.LogErrorFormat("Parsed invalid submesh {0}/{1} : {2} "+(meshes[s].faceCount%3==1?"+1":"-1")+" Total:{3}",s+1,sCount,meshes[s].faceCount,debugTotal);
                        error=true;
                    } */
                }
               /*  if(error){
                    debugTotal=0;
                    foreach(AMF_Mesh am in meshes){
                        debugTotal+=am.faceCount;
                    }
                    Debug.LogErrorFormat("Submesh Error: Total faces {0}",debugTotal);

                } */
            }
           // Debug.Log("Parsed Mesh Faces:"+debugTotal);
            return meshes;
        }

        
        public static Matrix4x4 getVMatrix(Vector2 xbound, Vector2 ybounds, Vector2 zbounds)
        {
            //new Matrix4x4(1.0f / 65535.0f, 0, 0, 0, 0, 1.0f / 65535.0f, 0, 0f, 0, 0, 1.0f / 65535.0f, 0f, 0.5f, 0.5f, 0.5f, 0f);
            Matrix4x4 dMat = new Matrix4x4();
            dMat.SetRow(0,new Vector4(1.0f / 65535.0f,0));
            dMat.SetRow(1,new Vector4(0,1.0f/65535.0f));
            dMat.SetRow(2,new Vector4(0,0,1.0f/65535.0f));
            dMat.SetRow(3,new Vector4(0.5f,0.5f,0.5f,1));
            //new Matrix4x4(xbound.Y - xbound.X, 0, 0, 0f, 0, ybounds.Y - ybounds.X, 0, 0f, 0, 0, zbounds.Y - zbounds.X, 0f, xbound.X, ybounds.X, zbounds.X, 0f);
            Matrix4x4 bMat = new Matrix4x4();
            bMat.SetRow(0,new Vector4(xbound.y - xbound.x,0f));
            bMat.SetRow(1,new Vector4(0f,ybounds.y-ybounds.x));
            bMat.SetRow(2,new Vector4(0,0,zbounds.y-ybounds.x));
            bMat.SetRow(3,new Vector4(xbound.x,ybounds.x,zbounds.x,1));
            return dMat * bMat;
        }

        public static Matrix4x4 getTMatrix(Vector2 ubounds, Vector2 vbounds)
        {
            //new Matrix4x4(1.0f / 32767.0f, 0, 0, 0,0, 1.0f / 32767.0f,0, 0, 0, 0, 1.0f / 32767.0f,0, 0, 0, 0,0);
            Matrix4x4 dMat = new Matrix4x4();
            dMat.SetRow(0,new Vector4(1f/32767.0f,0f));
            dMat.SetRow(1,new Vector4(0,1f/32767.0f));
            dMat.SetRow(2,new Vector4(0,0,1f/32767.0f));
            dMat.SetRow(3,new Vector4(0,0,0,1));
            //new Matrix4x4(ubounds.Y - ubounds.X, 0, 0, 0f, 0, vbounds.Y - vbounds.X, 0, 0f, 0, 0, 0, 0, ubounds.X, vbounds.Y, 0, 0);
            Matrix4x4 bMat = new Matrix4x4(new Vector4(ubounds.y-ubounds.x,0f),new Vector4(0,vbounds.y-vbounds.x),Vector4.zero,new Vector4(ubounds.x,ubounds.y,0,1));
            return dMat * bMat;
        }

        
        
        
        
        //[StructLayout(LayoutKind.Sequential)]
        //public struct AMF
        //{
        //    public string header;
        //    public float version;
        //    public string modelName;
        //}
        static List<int> UnpackVector3Int(List<Vector3Int> v){
            List<int> o = new List<int>();
            foreach(Vector3Int vi in v){
                o.Add(vi.x);
                o.Add(vi.y);
                o.Add(vi.z);
            }
            return o;
        }
        
        
       

       
    }
