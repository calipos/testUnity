using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
public class importFree : MonoBehaviour
{
    public string objPath = "";
    public Vector3[] vertexes;//顶点数
    public Vector3[] norms;
    public Vector2[] uvs;//uvs坐标
    public int[] triangles;//三角形索引
    public GameObject obj;
    MeshFilter mfilter_;
    SkinnedMeshRenderer skinRender_;
    MeshCollider collider_;
    Texture2D t2d;
    float angle = 0;
    // Start is called before the first frame update
    void Start()
    {
        while (true)
        {
            {
                OpenFileName openFileName = new OpenFileName();
                openFileName.structSize = Marshal.SizeOf(openFileName);
                //openFileName.filter = "Excel文件(*.xlsx)\0*.xlsx";
                openFileName.filter = "(*.*)\0*.*";
                openFileName.file = new string(new char[256]);
                openFileName.maxFile = openFileName.file.Length;
                openFileName.fileTitle = new string(new char[64]);
                openFileName.maxFileTitle = openFileName.fileTitle.Length;
                openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径
                openFileName.title = "窗口标题";
                openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

                if (LocalDialog.GetOpenFileName(openFileName))
                {
                    Debug.Log(openFileName.file);
                    Debug.Log(openFileName.fileTitle);
                }
                objPath = openFileName.file;
            }
            if (objPath.EndsWith(".obj"))
            {
                string pPath = System.IO.Path.GetDirectoryName(objPath); //获取文件路径
                string pName = System.IO.Path.GetFileName(objPath);
                Debug.Log(pPath);
                Debug.Log(pName);
                break;
            }
        }
        FileStream aFile = new FileStream(objPath, FileMode.Open);
        StreamReader reader = new StreamReader(aFile);
        string strLine = reader.ReadLine();
        List<Vector3> pts_=new List<Vector3>();
        List<int> faces_ = new List<int>();
        List<Vector2> uvs_ = new List<Vector2>();
        while (true)
        {
            strLine = reader.ReadLine();
            if (strLine == null)
            {
                break;
            }
            if (strLine.StartsWith("v "))
            {
                string[] sArray = Regex.Split(strLine, " ", RegexOptions.IgnoreCase);
                float x = float.Parse(sArray[1]);
                float y = float.Parse(sArray[2]);
                float z = float.Parse(sArray[3]);
                Vector3 thisPt = new Vector3(x, y, z);
                pts_.Add(thisPt);
            }
            else if (strLine.StartsWith("vt "))
            {
                string[] sArray = Regex.Split(strLine, " ", RegexOptions.IgnoreCase);
                double x = double.Parse(sArray[1]);
                double y = double.Parse(sArray[2]);
                Vector2 thisUv = new Vector2((float)x, (float)y);
                uvs_.Add(thisUv);
            }
            else if (strLine.StartsWith("f "))
            {
                int ptA = 0;
                int ptB = 0;
                int ptC = 0;
                string[] sArray = Regex.Split(strLine, " ", RegexOptions.IgnoreCase);
                {
                    string[] ptAndUvStrs = Regex.Split(sArray[1], "/", RegexOptions.IgnoreCase);
                    ptA = int.Parse(ptAndUvStrs[0]) - 1;
                }
                {
                    string[] ptAndUvStrs = Regex.Split(sArray[2], "/", RegexOptions.IgnoreCase);
                    ptB = int.Parse(ptAndUvStrs[0]) - 1;
                }
                {
                    string[] ptAndUvStrs = Regex.Split(sArray[3], "/", RegexOptions.IgnoreCase);
                    ptC = int.Parse(ptAndUvStrs[0]) - 1;
                }
                faces_.Add(ptA);
                faces_.Add(ptB);
                faces_.Add(ptC);
                faces_.Add(ptA);
                faces_.Add(ptC);
                faces_.Add(ptB);
            }
            else
            {
                continue;
            }
        }
        reader.Close();
        vertexes = pts_.ToArray();
        triangles = faces_.ToArray();
        uvs = uvs_.ToArray();

        t2d = Resources.Load("one-", typeof(Texture2D)) as Texture2D;
        obj = new GameObject();
        obj.name = "one-_obj";        
        mfilter_ = obj.AddComponent<MeshFilter>(); 
        skinRender_ = obj.AddComponent<SkinnedMeshRenderer>();
        collider_ = obj.AddComponent<MeshCollider>();


        Mesh mesh = mfilter_.mesh;
        mesh.vertices = vertexes;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        //mesh.RecalculateNormals();
        collider_.sharedMesh = mesh;
        skinRender_.material = new Material(Shader.Find("Diffuse"));
        skinRender_.material.mainTexture = t2d;

        skinRender_.sharedMesh = mesh;

        obj.transform.position = new Vector3(-1.98f, -31.5f, 25f);
        obj.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        Transform[] bones = new Transform[2];
        Matrix4x4[] bindPoses = new Matrix4x4[2];
        bones[0] = new GameObject("Lower").transform;
        bones[0].parent = obj.transform;
        bones[0].localRotation = Quaternion.identity;
        bones[0].localPosition = obj.GetComponent<MeshFilter>().mesh.vertices[1]; 
        bindPoses[0] = bones[0].worldToLocalMatrix * obj.transform.localToWorldMatrix;
        bones[1] = new GameObject("Upper").transform;
        bones[1].parent = bones[0];
        bones[1].localRotation = Quaternion.identity;
        bones[1].localPosition =  obj.GetComponent<MeshFilter>().mesh.vertices[vertexes.Length-2]; 
        bindPoses[1] = bones[1].worldToLocalMatrix * obj.transform.localToWorldMatrix;
        mesh.bindposes = bindPoses;
        BoneWeight[] weights = new BoneWeight[vertexes.Length];
        for (int i = 0; i < weights.Length; i++)
        {
            if (i < 3)
            {
                weights[i].boneIndex0 = 0;
                weights[i].weight0 = 1;
            }
            else if (i >= weights.Length - 3)
            {
                weights[i].boneIndex0 = 1;
                weights[i].weight0 = 1;
            }
            else
            {
                weights[i].boneIndex0 = 0;
                weights[i].weight0 = 0.5f;
                weights[i].boneIndex1 = 1;
                weights[i].weight1 = 0.5f;
            }
        }
        mesh.boneWeights = weights;
        skinRender_.bones = bones;
        DynamicBone db = obj.AddComponent<DynamicBone>();
        db.m_Root = bones[0];
        db.m_Damping = 0.9f;
        db.m_Elasticity = 0;
        db.m_Stiffness = 0.6f;
        db.m_Inert = 0.3f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 5 == 0)
        {
            angle += 0.173f;
            obj.transform.RotateAround(new Vector3(0, 1, 0), Mathf.Sin(angle)*.1f);
        }

    }
}

