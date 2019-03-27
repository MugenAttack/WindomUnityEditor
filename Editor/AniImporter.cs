using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


public class AniImporter : EditorWindow
{
    string filename;
    hod structure = new hod();
    List<hod> hods = new List<hod>();
    List<bool> hodChecks = new List<bool>();
    Vector2 scroll = new Vector2();
    string animationName = "";
    float timeBetween = 0.12f;
    string tbConvert = "";

    [MenuItem("Window/Windom/Ani Importer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AniImporter));
    }

    void OnGUI()
    {

        filename = EditorGUILayout.TextField("File", filename);
        //removeExt = GUILayout.Toggle(removeExt, "Remove Extension");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Find File", GUILayout.Width(200)))
        {
            filename = EditorUtility.OpenFilePanel("Find File", "", "ani");
        }

        if (GUILayout.Button("Load File", GUILayout.Width(200)))
        {
            loadAni();
        }
        GUILayout.EndHorizontal();

        scroll = GUILayout.BeginScrollView(scroll, false, true);
        for (int i = 0; i < hods.Count; i++)
        {
            GUILayout.BeginHorizontal();
            hodChecks[i] = GUILayout.Toggle(hodChecks[i], hods[i].name);

            string textAdd = "";
            if (hodChecks[i])
                textAdd = " (X)";

            if (GUILayout.Button("Preview" + textAdd, GUILayout.Width(100)))
            {
                GameObject GO;
                string rootObject = "";
                for (int j = 0; j < structure.parts.Length; j++)
                {
                    if (j == 0)
                    {
                        rootObject = structure.parts[j].part_name;
                        Debug.Log(rootObject);
                        GO = GameObject.Find(rootObject);
                    }
                    else
                    {
                        Debug.Log(rootObject + "/" + structure.parts[j].part_path);
                        GO = GameObject.Find(rootObject + "/" + structure.parts[j].part_path);
                    }

                    GO.transform.localPosition = hods[i].parts[j].localPosition;
                    GO.transform.localRotation = hods[i].parts[j].localRotation;
                    GO.transform.localScale = hods[i].parts[j].localScale;
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply Structure", GUILayout.Width(200)))
        {
            GameObject GO;
            string rootObject = "";
            for (int j = 0; j < structure.parts.Length; j++)
            {
                if (j == 0)
                {
                    rootObject = structure.parts[j].part_name;
                    //Debug.Log(rootObject);
                    GO = GameObject.Find(rootObject);
                }
                else
                {
                    //Debug.Log(rootObject + "/" + structure.parts[j].part_path);
                    GO = GameObject.Find(rootObject + "/" + structure.parts[j].part_path);
                }

                GO.transform.localPosition = structure.parts[j].localPosition;
                GO.transform.localRotation = structure.parts[j].localRotation;
                GO.transform.localScale = structure.parts[j].localScale;
            }
        }

        if (GUILayout.Button("Build Structure", GUILayout.Width(200)))
        {
            buildStructure();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Animation Name");
        animationName = GUILayout.TextField(animationName);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Time Between Frames");
        tbConvert = GUILayout.TextField(tbConvert);
        float.TryParse(tbConvert, out timeBetween);
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Build Animation", GUILayout.Width(200)))
        {
            BuildAnimation();
        }
    }

    void loadAni()
    {
        
        BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open));
        string signature = new string(br.ReadChars(3));
        if (signature == "AN2")
        {
            br.BaseStream.Seek(256, SeekOrigin.Current);
            structure.loadhod(ref br);
        }

        //search for other hod files and load them
        while (br.BaseStream.Position != br.BaseStream.Length)
        {
            byte hCheck = br.ReadByte();
            if (hCheck == 'H')
            {
                hCheck = br.ReadByte();
                if (hCheck == 'D')
                {
                    hCheck = br.ReadByte();
                    if (hCheck == '2')
                    {
                        
                        br.BaseStream.Seek(-3, SeekOrigin.Current);
                        long hodAddress = br.BaseStream.Position;
                        string name = "";
                        br.BaseStream.Seek(-1, SeekOrigin.Current);
                        byte[] c = br.ReadBytes(1);
                        while (c[0] != 0x00)
                        {
                            name = new string(System.Text.Encoding.ASCII.GetChars(c)) + name;
                            br.BaseStream.Seek(-2, SeekOrigin.Current);
                            c = br.ReadBytes(1);
                        }

                        hod n = new hod();
                        n.name = name;
                        br.BaseStream.Seek(hodAddress, SeekOrigin.Begin);
                        n.loadhod(ref br);
                        hods.Add(n);
                        hodChecks.Add(false);
                    }
                }
            }
        }
        br.Close();
        
        buildPaths();
    }

    void buildPaths()
    { 
        for (int i = 0; i < structure.parts.Length; i++)
        {
            if (i == 0)
            {
                structure.parts[i].part_path = "";
            }
            else
            {
                //find next level higher in tree.
                for (int j = i - 1; j >= 0; j--)
                {
                    if (structure.parts[i].level - 1 == structure.parts[j].level)
                    {
                        if (j == 0)
                        {
                            structure.parts[i].part_path = structure.parts[i].part_name;
                        }
                        else
                        {
                            structure.parts[i].part_path = structure.parts[j].part_path + "/" + structure.parts[i].part_name;
                        }
                        break;
                    }
                }
            }
            Debug.Log(structure.parts[i].part_path);
        }
        
    }
    
    void buildStructure()
    {
        List<GameObject> GOs = new List<GameObject>();

        for (int i = 0; i < structure.parts.Length; i++)
        {
            var part = GameObject.Find(structure.parts[i].part_name);
            if (part == null)
                part = new GameObject(structure.parts[i].part_name);

            GOs.Add(part);
            if (i == 0)
            {
                GOs[i].transform.localPosition = structure.parts[i].localPosition;
                GOs[i].transform.localRotation = structure.parts[i].localRotation;
                GOs[i].transform.localScale = structure.parts[i].localScale;
            }
            else
            {
                //find next level higher in tree.
                for (int j = i - 1; j >= 0; j--)
                {
                    if (structure.parts[i].level - 1 == structure.parts[j].level)
                    {
                        if (j == 0)
                        {
                            GOs[i].transform.SetParent(GOs[0].transform);
                            GOs[i].transform.localPosition = structure.parts[i].localPosition;
                            GOs[i].transform.localRotation = structure.parts[i].localRotation;
                            GOs[i].transform.localScale = structure.parts[i].localScale;
                        }
                        else
                        {
                            GOs[i].transform.SetParent(GOs[j].transform);
                            GOs[i].transform.localPosition = structure.parts[i].localPosition;
                            GOs[i].transform.localRotation = structure.parts[i].localRotation;
                            GOs[i].transform.localScale = structure.parts[i].localScale;
                        }
                        break;
                    }
                }
            }
        }
    }

    void BuildAnimation()
    {
        AnimationClip nClip = new AnimationClip();
        nClip.name = animationName;
        for (int i = 0; i < structure.parts.Length; i++)
        {
            float time = 0.0f;
            AnimationCurve RotX = new AnimationCurve();
            AnimationCurve RotY = new AnimationCurve();
            AnimationCurve RotZ = new AnimationCurve();
            AnimationCurve RotW = new AnimationCurve();

            AnimationCurve ScaleX = new AnimationCurve();
            AnimationCurve ScaleY = new AnimationCurve();
            AnimationCurve ScaleZ = new AnimationCurve();

            AnimationCurve PosX = new AnimationCurve();
            AnimationCurve PosY = new AnimationCurve();
            AnimationCurve PosZ = new AnimationCurve();
            
            //add to curves from hod file
            for (int h = 0; h < hods.Count; h++)
            {
                if (hodChecks[h])
                {
                    RotX.AddKey(time, hods[h].parts[i].localRotation.x);
                    RotY.AddKey(time, hods[h].parts[i].localRotation.y);
                    RotZ.AddKey(time, hods[h].parts[i].localRotation.z);
                    RotW.AddKey(time, hods[h].parts[i].localRotation.w);

                    ScaleX.AddKey(time, hods[h].parts[i].localScale.x);
                    ScaleY.AddKey(time, hods[h].parts[i].localScale.y);
                    ScaleZ.AddKey(time, hods[h].parts[i].localScale.z);

                    PosX.AddKey(time, hods[h].parts[i].localPosition.x);
                    PosY.AddKey(time, hods[h].parts[i].localPosition.y);
                    PosZ.AddKey(time, hods[h].parts[i].localPosition.z);

                    time += timeBetween;
                }
            }

            //load curves into clip
            nClip.SetCurve(structure.parts[i].part_path, typeof(Transform), "localRotation.x", RotX);
            nClip.SetCurve(structure.parts[i].part_path, typeof(Transform), "localRotation.y", RotY);
            nClip.SetCurve(structure.parts[i].part_path, typeof(Transform), "localRotation.z", RotZ);
            nClip.SetCurve(structure.parts[i].part_path, typeof(Transform), "localRotation.w", RotW);

            nClip.SetCurve(structure.parts[i].part_path, typeof(Transform), "localScale.x", ScaleX);
            nClip.SetCurve(structure.parts[i].part_path, typeof(Transform), "localScale.y", ScaleY);
            nClip.SetCurve(structure.parts[i].part_path, typeof(Transform), "localScale.z", ScaleZ);

            nClip.SetCurve(structure.parts[i].part_path, typeof(Transform), "localPosition.x", PosX);
            nClip.SetCurve(structure.parts[i].part_path, typeof(Transform), "localPosition.y", PosY);
            nClip.SetCurve(structure.parts[i].part_path, typeof(Transform), "localPosition.z", PosZ);
        }

        if (nClip.name != "")
        {
            try
            {
                AssetDatabase.CreateAsset(nClip, "Assets/" + nClip.name + ".anim");
                AssetDatabase.SaveAssets();
            }
            catch
            {
                Debug.Log("Error in Creating Clip");
            }
        }
    }
}