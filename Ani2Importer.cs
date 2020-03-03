using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Ani2Importer : EditorWindow
{

    Ani2 aniFile = new Ani2();
    bool[] animChecks;
    string[] partPaths;
    Vector2 scroll = new Vector2();
    string filename = "";
    bool isLoaded = false;

    [MenuItem("Window/Windom/Ani2 Importer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(Ani2Importer));
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
            aniFile.load(filename);
            animChecks = new bool[aniFile.animations.Count];
            isLoaded = true;
            buildPaths();
        }
        GUILayout.EndHorizontal();
        if (isLoaded)
        {
            scroll = GUILayout.BeginScrollView(scroll, false, true);
            for (int i = 0; i < aniFile.animations.Count; i++)
            {
                GUILayout.BeginHorizontal();
                animChecks[i] = GUILayout.Toggle(animChecks[i], i.ToString() + " - " + aniFile.animations[i].name);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Build Structure", GUILayout.Width(200)))
            {
                BuildStructure();
            }


            if (GUILayout.Button("Build Animation", GUILayout.Width(200)))
            {
                BuildAnimations();
            }
            GUILayout.EndHorizontal();
        }
    }

    void buildPaths()
    {
        partPaths = new string[aniFile.structure.parts.Count];
        for (int i = 0; i < aniFile.structure.parts.Count; i++)
        {
            if (i == 0)
            {
                partPaths[i] = "";
            }
            else
            {
                //find next level higher in tree.
                for (int j = i - 1; j >= 0; j--)
                {
                    if (aniFile.structure.parts[i].treeDepth - 1 == aniFile.structure.parts[j].treeDepth)
                    {
                        if (j == 0)
                        {
                            partPaths[i] = aniFile.structure.parts[i].name.Replace(".x", "");
                        }
                        else
                        {
                            partPaths[i] = partPaths[j] + "/" + aniFile.structure.parts[i].name.Replace(".x", "");
                        }
                        break;
                    }
                }
            }
            //Debug.Log(partPaths[i]);
        }

    }
    void BuildStructure()
    {
        List<GameObject> GOs = new List<GameObject>();

        for (int i = 0; i < aniFile.structure.parts.Count; i++)
        {
            var part = GameObject.Find(aniFile.structure.parts[i].name.Replace(".x", ""));
            if (part == null)
                part = new GameObject(aniFile.structure.parts[i].name.Replace(".x", ""));

            GOs.Add(part);
            if (i == 0)
            {
                GOs[i].transform.localPosition = aniFile.structure.parts[i].position;
                GOs[i].transform.localRotation = aniFile.structure.parts[i].rotation;
                GOs[i].transform.localScale = aniFile.structure.parts[i].scale;
            }
            else
            {
                //find next level higher in tree.
                for (int j = i - 1; j >= 0; j--)
                {
                    if (aniFile.structure.parts[i].treeDepth - 1 == aniFile.structure.parts[j].treeDepth)
                    {
                        if (j == 0)
                        {
                            GOs[i].transform.SetParent(GOs[0].transform);
                            GOs[i].transform.localPosition = aniFile.structure.parts[i].position;
                            GOs[i].transform.localRotation = aniFile.structure.parts[i].rotation;
                            GOs[i].transform.localScale = aniFile.structure.parts[i].scale;
                        }
                        else
                        {
                            GOs[i].transform.SetParent(GOs[j].transform);
                            GOs[i].transform.localPosition = aniFile.structure.parts[i].position;
                            GOs[i].transform.localRotation = aniFile.structure.parts[i].rotation;
                            GOs[i].transform.localScale = aniFile.structure.parts[i].scale;
                        }
                        break;
                    }
                }
            }
        }
    }

    void BuildAnimations()
    {
        for (int a = 0; a < aniFile.animations.Count; a++)
        {
            if (animChecks[a])
            {
                AnimationClip nClip = new AnimationClip();
                nClip.name = a.ToString() + " - " + aniFile.animations[a].name;
                //calculate frame length
                float timeBetween = 0.0f;
                for (int s = 0; s < aniFile.animations[a].scripts.Count; s++)
                    timeBetween += aniFile.animations[a].scripts[s].time * 2;

                timeBetween = timeBetween / (aniFile.animations[a].frames.Count - 1);

                for (int i = 0; i < partPaths.Length; i++)
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
                    for (int h = 0; h < aniFile.animations[a].frames.Count; h++)
                    {
                        
                        RotX.AddKey(time, aniFile.animations[a].frames[h].parts[i].rotation.x);
                        RotY.AddKey(time, aniFile.animations[a].frames[h].parts[i].rotation.y);
                        RotZ.AddKey(time, aniFile.animations[a].frames[h].parts[i].rotation.z);
                        RotW.AddKey(time, aniFile.animations[a].frames[h].parts[i].rotation.w);

                        ScaleX.AddKey(time, aniFile.animations[a].frames[h].parts[i].scale.x);
                        ScaleY.AddKey(time, aniFile.animations[a].frames[h].parts[i].scale.y);
                        ScaleZ.AddKey(time, aniFile.animations[a].frames[h].parts[i].scale.z);

                        PosX.AddKey(time, aniFile.animations[a].frames[h].parts[i].position.x);
                        PosY.AddKey(time, aniFile.animations[a].frames[h].parts[i].position.y);
                        PosZ.AddKey(time, aniFile.animations[a].frames[h].parts[i].position.z);

                        time += timeBetween;
                        
                    }

                    //load curves into clip
                    nClip.SetCurve(partPaths[i], typeof(Transform), "localRotation.x", RotX);
                    nClip.SetCurve(partPaths[i], typeof(Transform), "localRotation.y", RotY);
                    nClip.SetCurve(partPaths[i], typeof(Transform), "localRotation.z", RotZ);
                    nClip.SetCurve(partPaths[i], typeof(Transform), "localRotation.w", RotW);

                    nClip.SetCurve(partPaths[i], typeof(Transform), "localScale.x", ScaleX);
                    nClip.SetCurve(partPaths[i], typeof(Transform), "localScale.y", ScaleY);
                    nClip.SetCurve(partPaths[i], typeof(Transform), "localScale.z", ScaleZ);

                    nClip.SetCurve(partPaths[i], typeof(Transform), "localPosition.x", PosX);
                    nClip.SetCurve(partPaths[i], typeof(Transform), "localPosition.y", PosY);
                    nClip.SetCurve(partPaths[i], typeof(Transform), "localPosition.z", PosZ);
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
    }
}

