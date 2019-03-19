using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;
public struct partPath
{
    public string name;
    public string path;
}

public class SVCreateClip : EditorWindow {

    GameObject root;
    List<string> xmlFiles = new List<string>();
    int FrameRate = 30;
    List<partPath> parts = new List<partPath>();

    [MenuItem("Window/Windom/SV Create Clip")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SVCreateClip));
    }

    void createPathData(string tail, ref GameObject GO)
    {
        partPath n = new partPath();
        n.name = GO.name;
        n.path = tail + GO.name;
        parts.Add(n);
        int childCount = GO.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject pGO = GO.transform.GetChild(i).gameObject;
            createPathData(n.path + "/", ref pGO);
        }
    }

    void OnGUI()
    {
        root = (GameObject)EditorGUILayout.ObjectField(root, typeof(GameObject), true);
        FrameRate = int.Parse(EditorGUILayout.TextField("Frame Rate", FrameRate.ToString()));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Anime File", GUILayout.Width(150)))
        {
            xmlFiles.Add("");
        }
        if (GUILayout.Button("Find All Files", GUILayout.Width(150)))
        {
            for (int i = 0; i < xmlFiles.Count; i++)
                xmlFiles[i] = EditorUtility.OpenFilePanel("Find File", "", "xml");
        }
        if (GUILayout.Button("Clear All", GUILayout.Width(150)))
        {
            xmlFiles.Clear();
        }
        GUILayout.EndHorizontal();

        for (int i = 0; i < xmlFiles.Count; i++)
        {
            xmlFiles[i] = EditorGUILayout.TextField("Animation #" + i.ToString(), xmlFiles[i]);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Find File", GUILayout.Width(200)))
            {
                xmlFiles[i] = EditorUtility.OpenFilePanel("Find File", "", "xml");
            }
            if (GUILayout.Button("Remove", GUILayout.Width(200)))
            {
                xmlFiles.RemoveAt(i);
            }
            GUILayout.EndHorizontal();
        }

       
        
        if (GUILayout.Button("Build", GUILayout.Width(200)))
        {
            //generate path data

            int childCount = root.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                GameObject pGO = root.transform.GetChild(i).gameObject;
                createPathData("", ref pGO);
            }
            foreach (string f in xmlFiles)
            {
                AnimationClip nClip = constructAnimationClip(f);
                AssetDatabase.CreateAsset(nClip, "Assets/" + nClip.name + ".anim");
                AssetDatabase.SaveAssets();
            }
        }

        if (GUILayout.Button("Mass Build", GUILayout.Width(200)))
        {
            //generate path data

            int childCount = root.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                GameObject pGO = root.transform.GetChild(i).gameObject;
                createPathData("", ref pGO);
            }

            FileInfo inf = new FileInfo(xmlFiles[0]);
            DirectoryInfo D = inf.Directory;
            foreach (FileInfo f in D.GetFiles())
            {
                if (f.Name.Contains("Anime"))
                {
                    AnimationClip nClip = constructAnimationClip(f.FullName);

                    if (nClip.name != "")
                    {
                        try
                        {
                            AssetDatabase.CreateAsset(nClip, "Assets/" + nClip.name + ".anim");
                            AssetDatabase.SaveAssets();
                        } catch
                        { }

                    }
                }
            }
        }

    }

    partPath getPart(string name)
    {
        for (int i = 0; i < parts.Count; i++)
        {
            if (name == parts[i].name)
            {
                return parts[i];
            }
        }

        partPath dummy = new partPath();
        dummy.name = "Does Not Exist!";
        return dummy;
    }

    AnimationClip constructAnimationClip(string filename)
    {
        AnimationClip nClip = new AnimationClip();

        Debug.Log("loading");
        XmlDocument doc = new XmlDocument();
        doc.Load(filename);
        XmlNode AnimeName = doc.SelectSingleNode("AnimeName");
        nClip.name = AnimeName.Attributes["Name"].Value;
        if (nClip.name != "")
        {
            foreach (XmlNode node in AnimeName.ChildNodes)
        {
            if (node.Name == "BoneData")
            {
                
                string curvetype = "";
                XmlNode BoneData = node;
                
                foreach (XmlNode Bone in BoneData.ChildNodes)
                {
                    partPath prt = getPart(Bone.Attributes["Text"].Value.Replace(".x",""));
                    if (prt.name != "Does Not Exist!")
                    {
                        
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

                        XmlNodeList childs = Bone.ChildNodes;
                        for (int i = 0; i < childs.Count; i++)
                        {
                            switch (childs[i].Name)
                            {
                                case "RotateKey":
                                    curvetype = "RotateKey";
                                    break;
                                case "ScaleKey":
                                    curvetype = "Scalekey";
                                    break;
                                case "PosKey":
                                    curvetype = "PosKey";
                                    break;
                                case "Time":
                                    switch (curvetype)
                                    {
                                        case "RotateKey":
                                            Keyframe RP = new Keyframe();
                                            RP.time = float.Parse(childs[i].Attributes["Value"].Value) / FrameRate;
                                            if (RP.time < 0)
                                                RP.time = 0;


                                            XmlNode Rota = childs[i].ChildNodes[0];
                                            RP.value = float.Parse(Rota.Attributes["x"].Value);
                                            RotX.AddKey(RP);
                                            RP.value = float.Parse(Rota.Attributes["y"].Value);
                                            RotY.AddKey(RP);
                                            RP.value = float.Parse(Rota.Attributes["z"].Value);
                                            RotZ.AddKey(RP);
                                            RP.value = float.Parse(Rota.Attributes["w"].Value);
                                            RotW.AddKey(RP);
                                            break;
                                        case "ScaleKey":
                                            Keyframe SP = new Keyframe();
                                            SP.time = float.Parse(childs[i].Attributes["Value"].Value) / FrameRate;
                                            if (SP.time < 0)
                                                SP.time = 0;
                                            XmlNode Scale = childs[i].ChildNodes[0];
                                            SP.value = float.Parse(Scale.Attributes["x"].Value);
                                            ScaleX.AddKey(SP);
                                            SP.value = float.Parse(Scale.Attributes["y"].Value);
                                            ScaleY.AddKey(SP);
                                            SP.value = float.Parse(Scale.Attributes["z"].Value);
                                            ScaleZ.AddKey(SP);
                                            break;
                                        case "PosKey":
                                            Keyframe PP = new Keyframe();
                                            PP.time = float.Parse(childs[i].Attributes["Value"].Value) / FrameRate;
                                            if (PP.time < 0)
                                                PP.time = 0;
                                            XmlNode Pos = childs[i].ChildNodes[0];
                                            PP.value = float.Parse(Pos.Attributes["x"].Value);
                                            PosX.AddKey(PP);
                                            PP.value = float.Parse(Pos.Attributes["y"].Value);
                                            PosY.AddKey(PP);
                                            PP.value = float.Parse(Pos.Attributes["z"].Value);
                                            PosZ.AddKey(PP);
                                            break;
                                    }

                                    break;
                            }
                        }

                        //load curves into clip
                        nClip.SetCurve(prt.path, typeof(Transform), "localRotation.x", RotX);
                        nClip.SetCurve(prt.path, typeof(Transform), "localRotation.y", RotY);
                        nClip.SetCurve(prt.path, typeof(Transform), "localRotation.z", RotZ);
                        nClip.SetCurve(prt.path, typeof(Transform), "localRotation.w", RotW);

                        nClip.SetCurve(prt.path, typeof(Transform), "localScale.x", ScaleX);
                        nClip.SetCurve(prt.path, typeof(Transform), "localScale.y", ScaleY);
                        nClip.SetCurve(prt.path, typeof(Transform), "localScale.z", ScaleZ);

                        nClip.SetCurve(prt.path, typeof(Transform), "localPosition.x", PosX);
                        nClip.SetCurve(prt.path, typeof(Transform), "localPosition.y", PosY);
                        nClip.SetCurve(prt.path, typeof(Transform), "localPosition.z", PosZ);
                    }
                }
            }
        }
        }

        return nClip;
    }
}
