using UnityEngine;
using AdjutantSharp;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif 
public class AMFShaderInfo : ScriptableObject{
    readonly string _RegularShader="Standard";
    readonly string _TerrainShader="Adjutant/MultiBlend";
    public enum ShaderInfoType{
        Regular,Terrain
    }

    /*
    Textures
    0 - Diffuse
    1 - Detail
    2 - ColorChange
    3 - Normal
    4 - Detail Normal
    5 - Emission
    6 - ???
    7 - ???
     */

     /*
    Colors
    0 - Main Color
    1 - Emission Color
    2 - Detail Color
     */
    public string sName;
    public ShaderInfoType shaderType;
    public string[] paths;
    public Vector2[] tiles;
    public string[] detPaths;
    public Vector2[] detTiles;
    public bool ccOnly;
    public bool isTransparent;
    public Color32[] tints;

    /*
    Terrain shader values
     */
    public string blendPath;
    public Vector2 blendTile;
    public string[] bumpmaps;
    public Vector2[] bumpTiles;

    public string workingDir;

    public void SaveData(AdjutantSharp.ShaderInfo s){
        
        sName=s.sName;
        if(s.GetType()==typeof(RegularShader)){
            shaderType=ShaderInfoType.Regular;
            SaveData((RegularShader)s);
        }else{
            shaderType=ShaderInfoType.Terrain;
            SaveData((TerrainShader)s);
        }
    }
    private void SaveData(RegularShader s){
        paths=s.paths.ToArray();
        CleanPaths(paths);
        ccOnly=s.ccOnly;
        isTransparent=s.isTransparent;
        tints=s.tints.ToArray();
        tiles=s.uvTiles.ToArray();
    }

    void CleanPaths(string[] tbc){
        for(int i=0;i<tbc.Length;i++){
            if(!tbc[i].Contains("null"))
                tbc[i]=tbc[i].Replace('\\','/')+".tif";
        }
    }
    private void SaveData(TerrainShader s){
        paths=s.baseMaps.ToArray();
        CleanPaths(paths);
        tiles=s.baseTiles.ToArray();
        detPaths=s.detMaps.ToArray();
        CleanPaths(detPaths);
        detTiles=s.detTiles.ToArray();
        bumpmaps=s.bumpMaps.ToArray();
        CleanPaths(bumpmaps);
        bumpTiles=s.bumpTiles.ToArray();
        blendPath=s.blendPath.Replace('\\','/')+".tif";
        blendTile=s.blendTile;
    }

    #if UNITY_EDITOR
    /*
    
    Calls to AssetDatabase require the UnityEditor namespace. Which won't compile out on builds.
    So you can't call this at runtime
     */
     public Material CreateMaterial(){
         if(workingDir==null){
            workingDir=AssetDatabase.GetAssetPath(this);
            workingDir=workingDir.Substring(0,workingDir.LastIndexOf("/")+1);
        }
        Material m;
        if(shaderType==ShaderInfoType.Terrain){
            m= new Material(Shader.Find(_TerrainShader));
            SetupTerrainMaterial(m);
        }else{
            m=new Material(Shader.Find(_RegularShader));
            SetupRegularMaterial(m);
        }
        return m;
     }
    public void SetupMaterial(Material m){
        if(workingDir==null||workingDir.Equals("")){
            workingDir=AssetDatabase.GetAssetPath(this);
            workingDir=workingDir.Substring(0,workingDir.LastIndexOf("/")+1);
        }
        if(shaderType==ShaderInfoType.Terrain){
            SetupTerrainMaterial(m);
        }else{
            SetupRegularMaterial(m);
        }
    }
    void SetupRegularMaterial(Material m){
        m.shader=Shader.Find(_RegularShader);
        //setup textures
        for(int i=0;i<paths.Length;i++){
            if(paths[i]==null)
                continue;
            TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(workingDir+paths[i]);
            if(ti==null)
                continue;
            Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(workingDir+paths[i],typeof(Texture2D));

            switch(i){
                case 0:
                    m.SetTexture("_MainTex",tex);
                    if(ti.DoesSourceTextureHaveAlpha()){
                        if(isTransparent){
                            m.SetFloat("_Mode",2f);
                            m.SetFloat("_SmoothnessTextureChannel",0f);
                            m.DisableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
                        }else{
                            m.SetFloat("_SmoothnessTextureChannel",1f);
                            m.EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
                        }
                    }
                    if(tiles!=null&&i<tiles.Length)
                        m.SetTextureScale("_MainTex",tiles[i]);
                    break;
                case 1:
                    m.SetTexture("_DetailAlbedoMap",tex);
                    if(tiles!=null&&i<tiles.Length)
                        m.SetTextureScale("_DetailAlbedoMap",tiles[i]);
                    m.EnableKeyword("___ _DETAIL_MULX2");

                break;
                
                case 3:
                    m.SetTexture("_BumpMap",tex);
                    if(tiles!=null&&i<tiles.Length)
                        m.SetTextureScale("_BumpMap",tiles[i]);
                    m.EnableKeyword("_NORMALMAP");
                    break;
                case 4:
                    m.SetTexture("_DetailNormalMap",tex);
                    if(tiles!=null&&i<tiles.Length)
                        m.SetTextureScale("_DetailNormalMap",tiles[i]);
                    m.EnableKeyword("_DETAIL_MULX2");
                    break;
                case 5:
                    
                    
                    m.SetFloat("_EmissionScaleUI", 1);
                    m.SetColor("_EmissionColor",Color.white);
                    m.SetTexture("_EmissionMap",tex);
                    if(tiles!=null&&i<tiles.Length)
                        m.SetTextureScale("_EmissionMap",tiles[i]);
                    m.EnableKeyword("_EMISSION");
                    break;
                    
                default:
                    Debug.LogErrorFormat("{0} has Texture in unimplemented slot {1}! {2}",sName,i,paths[i]);
                break;
            }
        }
        Color nullColor = new Color(0,0,0,0);
        Color tempColor;
        //setup colors
        for(int i=0;i<tints.Length;i++){
            tempColor=tints[i];
            if(tints[i].r==0&&tints[i].a==0&&i==0)
                tempColor=Color.white;

            switch(i){
                case 0:
                    m.SetColor("_Color",tempColor);
                    break;
                case 1:
                    m.SetColor("_EmissionColor",tempColor);
                    break;
                case 2:
                    Debug.LogWarningFormat("{0} has a detail tint. Currently not implemented.",sName);
                    break;
            }
        }
    }
    void SetupTerrainMaterial(Material m){
        
        
        m.shader=Shader.Find(_TerrainShader);
        //m.SetFloat("_Intensity",1f);
        Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(workingDir+blendPath,typeof(Texture2D));
        if(tex!=null){
            m.SetTexture("_BlendTex",tex);
            m.SetTextureScale("_BlendTex",blendTile);
        }else{
            Debug.LogErrorFormat("{0} was declared as a Terrain shader, but has no blend map! {1}",sName,blendPath);
        }
        
        /*
        We use the R channel as our fallback names
         */
        string texName="_MainTex";
        tex = (Texture2D)AssetDatabase.LoadAssetAtPath(workingDir+paths[0],typeof(Texture2D));
        if(tex!=null){
            m.SetTexture(texName,tex);
            m.SetTextureScale(texName,tiles[0]);
            m.EnableKeyword("_ENABLE_R_ON");
        }
        if(bumpmaps.Length>0){
            tex = (Texture2D)AssetDatabase.LoadAssetAtPath(workingDir+bumpmaps[0],typeof(Texture2D));
            if(tex!=null){
                m.SetTexture("_BumpMap",tex);
                m.SetTextureScale("_BumpMap",bumpTiles[0]);
            }
        }
        if(detPaths.Length>0){
            tex = (Texture2D)AssetDatabase.LoadAssetAtPath(workingDir+detPaths[0],typeof(Texture2D));
            if(tex!=null){
                m.SetTexture("_DetailAlbedoMap",tex);
                m.SetTextureScale("_DetailAlbedoMap",detTiles[0]);
            }
        }
        string keyword="";
        for(int i=1;i<paths.Length;i++){
            texName=null;
            switch(i){
                /* case 0:
                    texName="_MainTex";
                    break; */
                case 1:
                    texName="_GTex";
                    keyword="_ENABLE_G_ON";
                    break;
                case 2:
                    texName="_BTex";
                    keyword="_ENABLE_B_ON";
                    break;
                case 3:
                    texName="_ATex";
                    keyword="_ENABLE_A_ON"; 
                    break;
                default:
                    texName=null;
                    break;
            }
            if(texName==null)
                continue;
            tex = (Texture2D)AssetDatabase.LoadAssetAtPath(workingDir+paths[i],typeof(Texture2D));
            if(tex!=null){
                m.SetTexture(texName,tex);
                m.SetTextureScale(texName,tiles[i]);
                m.EnableKeyword(keyword);
            }
                
            
            if(i<bumpmaps.Length){
                tex = (Texture2D)AssetDatabase.LoadAssetAtPath(workingDir+bumpmaps[i],typeof(Texture2D));
                if(tex!=null){
                    
                    m.SetTexture(texName+"Bump",tex);
                    m.SetTextureScale(texName+"Bump",bumpTiles[i]);
                }
            }
            if(i<detPaths.Length){
                tex = (Texture2D)AssetDatabase.LoadAssetAtPath(workingDir+detPaths[i],typeof(Texture2D));
                if(tex!=null){
                    m.SetTexture(texName+"Det",tex);
                    m.SetTextureScale(texName+"Det",detTiles[i]);
                }else{
                    Debug.LogErrorFormat("TerrainShader setup: couldn't find texture@ {0}",workingDir+detPaths[i]);
                }
            }else{
                Debug.LogWarningFormat("TerrainShader setup: not enough details {0} vs {1}",i,detPaths.Length);
            }
        }
            

    }

    public void CompressBlendMapChannels(){
        if(shaderType!=ShaderInfoType.Terrain)
            return;
        Texture2D tex = (Texture2D) AssetDatabase.LoadAssetAtPath(workingDir+blendPath,typeof(Texture2D));
        TextureChannelInfo channelInfo = GetChannelInfo(tex);
        if(channelInfo.channelsFound==4)
            return;
        int[] remap = {3,3,3,3};
        int searchStart=0;
        for(int i=0;i<remap.Length;i++){
            for(int j=searchStart;j<channelInfo.hasChannel.Length;j++){
                if(channelInfo.hasChannel[j]){
                    remap[i]=j;
                    searchStart=j+1;
                    break;
                }
            }
        }

        Color[] pixels=tex.GetPixels();
        float[] temp;
        for(int i=0;i<pixels.Length;i++){
            temp=pixels[i].ToArray();
            pixels[i]=new Color(temp[remap[0]],temp[remap[1]],temp[remap[2]],temp[remap[3]]);
        }
        tex.SetPixels(pixels);
        tex.Apply();
        AssetDatabase.SaveAssets();
    }
    #endif
    TextureChannelInfo GetChannelInfo(Texture2D texture){
        TextureChannelInfo channelInfo;
        Color[] pixels=texture.GetPixels();
        channelInfo.hasChannel= new bool[4];
        channelInfo.hasChannel[0]=false;
        channelInfo.hasChannel[1]=false;
        channelInfo.hasChannel[2]=false;
        channelInfo.hasChannel[3]=false;
        channelInfo.channelsFound=0;
        for(int i=0;i<pixels.Length;i++){
            channelInfo.hasChannel[0]=channelInfo.hasChannel[0]||(pixels[i].r!=0);
            channelInfo.hasChannel[1]=channelInfo.hasChannel[1]||(pixels[i].g!=0);
            channelInfo.hasChannel[2]=channelInfo.hasChannel[2]||(pixels[i].b!=0);
            channelInfo.hasChannel[3]=channelInfo.hasChannel[3]||(pixels[i].a!=0);
        }
        for(int i=0;i<channelInfo.hasChannel.Length;i++)
            channelInfo.channelsFound+=(channelInfo.hasChannel[i]?1:0);
        
        return channelInfo;
    }
    int GoodPathsCount(){
        int c=0;
        for(int i=0;i<paths.Length;i++){
            if(paths[i].Contains("null"))
                c++;
        }
        return c;
    }
    
    struct TextureChannelInfo{
        public bool[] hasChannel;
        public int channelsFound;
    }
}