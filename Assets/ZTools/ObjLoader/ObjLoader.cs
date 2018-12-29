using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ObjLoader : MonoBehaviour {

    public string path;
    public static bool splitByMaterial = false;
    public static string[] searchPaths = new string[] { "", "%FileName%_Textures" + Path.DirectorySeparatorChar };

    struct OBJFace {
        public string materialName;
        public string meshName;
        public int[] indexes;
    }

    // Use this for initialization
    void Start() {
        path = Application.dataPath + "/ZTools/ObjLoader/Engine/AreoEngine-06.obj";
        StartCoroutine(LoadOBJFile(path));
    }

    // Update is called once per frame
    void Update() {

    }

    IEnumerator LoadOBJFile(string fn) {
        string meshName = Path.GetFileNameWithoutExtension(fn);

        bool hasNormals = false;
        //OBJ LISTS
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        //UMESH LISTS
        List<Vector3> uvertices = new List<Vector3>();
        List<Vector3> unormals = new List<Vector3>();
        List<Vector2> uuvs = new List<Vector2>();
        //MESH CONSTRUCTION
        List<string> materialNames = new List<string>();
        List<string> objectNames = new List<string>();
        Dictionary<string, int> hashtable = new Dictionary<string, int>();
        List<OBJFace> faceList = new List<OBJFace>();
        string cmaterial = "";
        string cmesh = "default";
        //CACHE
        Material[] materialCache = null;
        //save this info for later
        FileInfo OBJFileInfo = new FileInfo(fn);

        string[] lines = File.ReadAllLines(fn);
        string ln;
        string l;
        string[] cmps;
        string data;
        int[] indexes1;



        string felement;
        int vertexIndex = -1;
        int normalIndex = -1;
        int uvIndex = -1;
        string[] elementComps;

        for (int xl = 0; xl < lines.Length; xl++)
        //foreach (string ln in File.ReadAllLines(fn))
        {
            ln = lines[xl];
            if (ln.Length > 0 && ln[0] != '#')
            {
                l = ln.Trim().Replace("  ", " ");
                cmps = l.Split(' ');
                data = l.Remove(0, l.IndexOf(' ') + 1);
                if (cmps[0] == "mtllib")
                {
                    //load cache
                    string pth = OBJGetFilePath(data, OBJFileInfo.Directory.FullName + Path.DirectorySeparatorChar, meshName);
                    if (pth != null)
                        materialCache = LoadMTLFile(pth);
                }
                else if ((cmps[0] == "g" || cmps[0] == "o") && splitByMaterial == false)
                {
                    cmesh = data;
                    if (!objectNames.Contains(cmesh))
                    {
                        objectNames.Add(cmesh);
                    }
                }
                else if (cmps[0] == "usemtl")
                {
                    cmaterial = data;
                    if (!materialNames.Contains(cmaterial))
                    {
                        materialNames.Add(cmaterial);
                    }

                    if (splitByMaterial)
                    {
                        if (!objectNames.Contains(cmaterial))
                        {
                            objectNames.Add(cmaterial);
                        }
                    }
                }
                else if (cmps[0] == "v")
                {
                    //VERTEX
                    vertices.Add(ParseVectorFromCMPSY(cmps, true));
                }
                else if (cmps[0] == "vn")
                {
                    //VERTEX NORMAL
                    normals.Add(ParseVectorFromCMPSY(cmps, true));
                }
                else if (cmps[0] == "vt")
                {
                    //VERTEX UV
                    uvs.Add(ParseVectorFromCMPSY2(cmps, true));
                }
                else if (cmps[0] == "f")
                {
                    indexes1 = new int[cmps.Length - 1];
                    for (int i = 1; i < cmps.Length; i++)
                    {
                        felement = cmps[i];
                        vertexIndex = -1;
                        normalIndex = -1;
                        uvIndex = -1;
                        if (felement.Contains("//"))
                        {
                            //doubleslash, no UVS.
                            elementComps = felement.Split('/');
                            vertexIndex = int.Parse(elementComps[0]) - 1;
                            normalIndex = int.Parse(elementComps[2]) - 1;
                        }
                        else if (felement.Count(x => x == '/') == 2)
                        {
                            //contains everything
                            elementComps = felement.Split('/');
                            vertexIndex = int.Parse(elementComps[0]) - 1;
                            uvIndex = int.Parse(elementComps[1]) - 1;
                            normalIndex = int.Parse(elementComps[2]) - 1;
                        }
                        else if (!felement.Contains("/"))
                        {
                            //just vertex inedx
                            vertexIndex = int.Parse(felement) - 1;
                        }
                        else
                        {
                            //vertex and uv
                            elementComps = felement.Split('/');
                            vertexIndex = int.Parse(elementComps[0]) - 1;
                            uvIndex = int.Parse(elementComps[1]) - 1;
                        }
                        string hashEntry = vertexIndex + "|" + normalIndex + "|" + uvIndex;
                        if (hashtable.ContainsKey(hashEntry))
                        {
                            indexes1[i - 1] = hashtable[hashEntry];
                        }
                        else
                        {
                            //create a new hash entry
                            indexes1[i - 1] = hashtable.Count;
                            hashtable[hashEntry] = hashtable.Count;
                            uvertices.Add(vertices[vertexIndex]);
                            if (normalIndex < 0 || (normalIndex > (normals.Count - 1)))
                            {
                                unormals.Add(Vector3.zero);
                            }
                            else
                            {
                                hasNormals = true;
                                unormals.Add(normals[normalIndex]);
                            }
                            if (uvIndex < 0 || (uvIndex > (uvs.Count - 1)))
                            {
                                uuvs.Add(Vector2.zero);
                            }
                            else
                            {
                                uuvs.Add(uvs[uvIndex]);
                            }

                        }
                    }
                    if (indexes1.Length < 5 && indexes1.Length >= 3)
                    {
                        OBJFace f1 = new OBJFace();
                        f1.materialName = cmaterial;
                        f1.indexes = new int[] { indexes1[0], indexes1[2], indexes1[1] };//modified by kfir for right2left
                        f1.meshName = (splitByMaterial) ? cmaterial : cmesh;
                        faceList.Add(f1);
                        if (indexes1.Length > 3)
                        {

                            OBJFace f2 = new OBJFace();
                            f2.materialName = cmaterial;
                            f2.meshName = (splitByMaterial) ? cmaterial : cmesh;
                            f2.indexes = new int[] { indexes1[2], indexes1[0], indexes1[3] };//modified by kfir for right2left
                            faceList.Add(f2);
                        }
                    }
                }
            }
        }
        if (objectNames.Count == 0)
            objectNames.Add("default");
        //build objects
        GameObject parentObject = new GameObject(meshName);

        for (int obji = 0; obji < objectNames.Count; obji++)
        //foreach (string obj in objectNames)
        {
            string obj = objectNames[obji];
            GameObject subObject = new GameObject(obj);
            subObject.transform.parent = parentObject.transform;
            subObject.transform.localScale = new Vector3(1, 1, 1);
            //Debug.LogWarning("subObject : " + subObject.name);
            //Create mesh
            Mesh m = new Mesh();
            m.name = obj;
            //LISTS FOR REORDERING
            List<Vector3> processedVertices = new List<Vector3>();
            List<Vector3> processedNormals = new List<Vector3>();
            List<Vector2> processedUVs = new List<Vector2>();
            List<int[]> processedIndexes = new List<int[]>();
            Dictionary<int, int> remapTable = new Dictionary<int, int>();
            //POPULATE MESH
            List<string> meshMaterialNames = new List<string>();
            OBJFace[] ofaces = faceList.Where(x => x.meshName == obj).ToArray();
            foreach (string mn in materialNames)
            {
                OBJFace[] faces = ofaces.Where(x => x.materialName == mn).ToArray();
                if (faces.Length > 0)
                {
                    //int[] indexes = new int[0];
                    List<int> indexes = new List<int>(); ;
                    for (int face_i = 0; face_i < faces.Length; face_i++)
                    //foreach (OBJFace f in faces)
                    {
                        OBJFace f = faces[face_i];
                        indexes.AddRange(f.indexes);
                        //int l = indexes.Length;
                        //System.Array.Resize(ref indexes, l + f.indexes.Length);
                        //System.Array.Copy(f.indexes, 0, indexes, l, f.indexes.Length);
                    }
                    meshMaterialNames.Add(mn);
                    if (m.subMeshCount != meshMaterialNames.Count)
                        m.subMeshCount = meshMaterialNames.Count;

                    for (int i = 0; i < indexes.Count; i++)
                    {
                        int idx = indexes[i];
                        //build remap table
                        if (remapTable.ContainsKey(idx))
                        {
                            //ezpz
                            indexes[i] = remapTable[idx];
                        }
                        else
                        {
                            processedVertices.Add(uvertices[idx]);
                            processedNormals.Add(unormals[idx]);
                            processedUVs.Add(uuvs[idx]);
                            remapTable[idx] = processedVertices.Count - 1;
                            indexes[i] = remapTable[idx];
                        }
                    }
                    processedIndexes.Add(indexes.ToArray());
                }
                else
                {

                }
            }
            //apply stuff
            m.vertices = processedVertices.ToArray();
            m.normals = processedNormals.ToArray();
            m.uv = processedUVs.ToArray();

            for (int i = 0; i < processedIndexes.Count; i++)
            {
                m.SetTriangles(processedIndexes[i], i);
            }

            if (!hasNormals)
            {
                m.RecalculateNormals();
            }
            m.RecalculateBounds();
            MeshFilter mf = subObject.AddComponent<MeshFilter>();
            MeshRenderer mr = subObject.AddComponent<MeshRenderer>();
            Material[] processedMaterials = new Material[meshMaterialNames.Count];
            for (int i = 0; i < meshMaterialNames.Count; i++)
            {
                if (materialCache == null)
                {
                    processedMaterials[i] = new Material(Shader.Find("Standard"));
                }
                else
                {
                    Material mfn = Array.Find(materialCache, x => x.name == meshMaterialNames[i]); ;
                    if (mfn == null)
                    {
                        processedMaterials[i] = new Material(Shader.Find("Standard"));
                    }
                    else
                    {
                        processedMaterials[i] = mfn;
                    }
                }
                processedMaterials[i].name = meshMaterialNames[i];
            }
            mr.materials = processedMaterials;
            mf.mesh = m;
        }
        //return parentObject;
        yield return null;
    }

    #region
    public static Vector3 ParseVectorFromCMPS(string[] cmps, bool z_mirror) {
        float x = float.Parse(cmps[1]);
        float y = float.Parse(cmps[2]);
        if (cmps.Length == 4)
        {
            float z = float.Parse(cmps[3]);
            if (z_mirror)
            {
                z *= -1;//modified by kfir for right2left
            }
            return new Vector3(x, y, z);
        }
        return new Vector2(x, y);
    }
    public static Vector3 ParseVectorFromCMPSY(string[] cmps, bool z_mirror) {
        float x;
        float.TryParse(cmps[1], out x);
        float y;
        float.TryParse(cmps[2], out y);
        if (cmps.Length == 4)
        {
            float z = float.Parse(cmps[3]);
            if (z_mirror)
            {
                z *= -1;//modified by kfir for right2left
            }
            return new Vector3(x, y, z);
        }
        return new Vector2(x, y);
    }
    public static Vector3 ParseVectorFromCMPSY2(string[] cmps, bool z_mirror) {
        float x;
        float.TryParse(cmps[1], out x);
        float y;
        float.TryParse(cmps[2], out y);
        //if (cmps.Length == 4)
        //{
        //    float z = float.Parse(cmps[3]);
        //    if (z_mirror)
        //    {
        //        z *= -1;//modified by kfir for right2left
        //    }
        //    return new Vector3(x, y, z);
        //}
        return new Vector3(x, y, 0f);
    }

    public static Material[] LoadMTLFile(string fn) {
        Material currentMaterial = null;
        List<Material> matlList = new List<Material>();
        FileInfo mtlFileInfo = new FileInfo(fn);
        string baseFileName = Path.GetFileNameWithoutExtension(fn);
        string mtlFileDirectory = mtlFileInfo.Directory.FullName + Path.DirectorySeparatorChar;
        foreach (string ln in File.ReadAllLines(fn))
        {
            string l = ln.Trim().Replace("  ", " ");
            string[] cmps = l.Split(' ');
            string data = l.Remove(0, l.IndexOf(' ') + 1);

            if (cmps[0] == "newmtl")
            {
                if (currentMaterial != null)
                {
                    matlList.Add(currentMaterial);
                }
                Shader sh = Shader.Find("Standard");

                currentMaterial = new Material(sh);

                currentMaterial.SetFloat("_Mode", 2);
                //currentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                //currentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                ////currentMaterial.SetInt("_ZWrite", 0);
                ////currentMaterial.DisableKeyword("_ALPHATEST_ON");
                //currentMaterial.EnableKeyword("_ALPHABLEND_ON");
                // currentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");

                currentMaterial.renderQueue = 3000;
                currentMaterial.name = data;
            }
            else if (cmps[0] == "Kd")
            {
                currentMaterial.SetColor("_Color", ParseColorFromCMPS(cmps));
            }
            else if (cmps[0] == "map_Kd")
            {
                //TEXTURE
                string fpth = OBJGetFilePath(data, mtlFileDirectory, baseFileName);
                if (fpth != null)
                    currentMaterial.SetTexture("_MainTex", LoadTexture(fpth));
            }
            else if (cmps[0] == "map_Bump")
            {
                //TEXTURE
                string fpth = OBJGetFilePath(data, mtlFileDirectory, baseFileName);
                if (fpth != null)
                {
                    currentMaterial.SetTexture("_BumpMap", LoadTexture(fpth, true));
                    currentMaterial.EnableKeyword("_NORMALMAP");
                }
            }
            else if (cmps[0] == "Ks")
            {
                currentMaterial.SetColor("_SpecColor", ParseColorFromCMPS(cmps));
            }
            else if (cmps[0] == "Ka")
            {
                currentMaterial.SetColor("_EmissionColor", ParseColorFromCMPS(cmps, 0.05f));
                currentMaterial.EnableKeyword("_EMISSION");
            }
            else if (cmps[0] == "d")
            {
                float visibility = float.Parse(cmps[1]);
                if (visibility < 1)
                {
                    Color temp = currentMaterial.color;

                    temp.a = visibility;
                    currentMaterial.SetColor("_Color", temp);
                    //TRANSPARENCY ENABLER
                    currentMaterial.SetFloat("_Mode", 3);
                    currentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    currentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    currentMaterial.SetInt("_ZWrite", 1);
                    currentMaterial.DisableKeyword("_ALPHATEST_ON");
                    currentMaterial.EnableKeyword("_ALPHABLEND_ON");
                    currentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    currentMaterial.renderQueue = 3000;
                }
            }
            else if (cmps[0] == "Ns")
            {
                float Ns = float.Parse(cmps[1]);
                Ns = (Ns / 1000);
                currentMaterial.SetFloat("_Glossiness", Ns);
            }
        }
        if (currentMaterial != null)
        {
            matlList.Add(currentMaterial);
        }
        return matlList.ToArray();
    }
    public static Color ParseColorFromCMPS(string[] cmps, float scalar = 1.0f) {
        float Kr = float.Parse(cmps[1]) * scalar;
        float Kg = float.Parse(cmps[2]) * scalar;
        float Kb = float.Parse(cmps[3]) * scalar;
        return new Color(Kr, Kg, Kb);
    }
    public static void SetNormalMap(ref Texture2D tex) {
        Color[] pixels = tex.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            Color temp = pixels[i];
            temp.r = pixels[i].g;
            temp.a = pixels[i].r;
            pixels[i] = temp;
        }
        tex.SetPixels(pixels);
    }
    public static Texture2D LoadTexture(string fn, bool normalMap = false) {
        if (!File.Exists(fn))
            return null;
        string ext = Path.GetExtension(fn).ToLower();
        if (ext == ".png" || ext == ".jpg")
        {
            Texture2D t2d = new Texture2D(1, 1);
            t2d.LoadImage(File.ReadAllBytes(fn));
            if (normalMap)
                SetNormalMap(ref t2d);
            return t2d;
        }
        else if (ext == ".dds")
        {
            Texture2D returnTex = LoadDDSManual(fn);
            if (normalMap)
                SetNormalMap(ref returnTex);
            return returnTex;
        }
        else if (ext == ".tga")
        {
            Texture2D returnTex = LoadTGA(fn);
            if (normalMap)
                SetNormalMap(ref returnTex);
            return returnTex;
        }
        else
        {
            //Debug.Log("texture not supported : " + fn);
        }
        return null;
    }
    public static string OBJGetFilePath(string path, string basePath, string fileName) {
        foreach (string sp in searchPaths)
        {
            string s = sp.Replace("%FileName%", fileName);
            if (File.Exists(basePath + s + path))
            {
                return basePath + s + path;
            }
            else if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
    public static Texture2D LoadTGA(Stream TGAStream) {

        using (BinaryReader r = new BinaryReader(TGAStream))
        {
            // Skip some header info we don't care about.
            // Even if we did care, we have to move the stream seek point to the beginning,
            // as the previous method in the workflow left it at the end.
            r.BaseStream.Seek(12, SeekOrigin.Begin);

            short width = r.ReadInt16();
            short height = r.ReadInt16();
            int bitDepth = r.ReadByte();

            // Skip a byte of header information we don't care about.
            r.BaseStream.Seek(1, SeekOrigin.Current);
            Texture2D tex = new Texture2D(width, height);
            Color32[] pulledColors = new Color32[width * height];

            if (bitDepth == 32)
            {
                for (int i = 0; i < width * height; i++)
                {
                    byte red = r.ReadByte();
                    byte green = r.ReadByte();
                    byte blue = r.ReadByte();
                    byte alpha = r.ReadByte();
                    pulledColors[i] = new Color32(blue, green, red, alpha);
                }
            }
            else if (bitDepth == 24)
            {
                for (int i = 0; i < width * height; i++)
                {
                    byte red = r.ReadByte();
                    byte green = r.ReadByte();
                    byte blue = r.ReadByte();

                    pulledColors[i] = new Color32(blue, green, red, 1);
                }
            }
            else
            {
                throw new Exception("TGA texture had non 32/24 bit depth.");
            }
            tex.SetPixels32(pulledColors);
            tex.Apply();
            return tex;
        }
    }
    public static Texture2D LoadTGA(string fileName) {
        using (var imageFile = File.OpenRead(fileName))
        {
            return LoadTGA(imageFile);
        }
    }
    public static Texture2D LoadDDSManual(string ddsPath) {
        try
        {
            byte[] ddsBytes = File.ReadAllBytes(ddsPath);
            byte ddsSizeCheck = ddsBytes[4];
            if (ddsSizeCheck != 124)
                throw new System.Exception("Invalid DDS DXTn texture. Unable to read"); //this header byte should be 124 for DDS image files
            int height = ddsBytes[13] * 256 + ddsBytes[12];
            int width = ddsBytes[17] * 256 + ddsBytes[16];
            byte DXTType = ddsBytes[87];
            TextureFormat textureFormat = TextureFormat.DXT5;
            if (DXTType == 49)
            {
                textureFormat = TextureFormat.DXT1;
                //	Debug.Log ("DXT1");
            }
            if (DXTType == 53)
            {
                textureFormat = TextureFormat.DXT5;
                //	Debug.Log ("DXT5");
            }
            int DDS_HEADER_SIZE = 128;
            byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
            Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

            System.IO.FileInfo finf = new System.IO.FileInfo(ddsPath);
            Texture2D texture = new Texture2D(width, height, textureFormat, false);
            texture.LoadRawTextureData(dxtBytes);
            texture.Apply();
            texture.name = finf.Name;
            return (texture);
        }
        catch (System.Exception ex)
        {
            //Debug.LogError("Error: Could not load DDS");
            return new Texture2D(8, 8);
        }
    }
    #endregion
}
