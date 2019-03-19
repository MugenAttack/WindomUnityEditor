using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class SVLoadStructure : EditorWindow {

    string filename;
    bool removeExt = true;
    [MenuItem("Window/Windom/SVLoadStructure")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SVLoadStructure));

    }

    void OnGUI()
    {

        filename = EditorGUILayout.TextField("File", filename);
        

        if (GUILayout.Button("Find File", GUILayout.Width(200)))
        {
            filename = EditorUtility.OpenFilePanel("Find File", "", "xml");
        }

        removeExt = GUILayout.Toggle(removeExt, "Remove Extension");
        if (GUILayout.Button("Build", GUILayout.Width(200)))
        {
            BpBoneData[] data = BoneProperty.Read(filename);
            Matrix4x4[] pMatrix = new Matrix4x4[data.Length];
            List<GameObject> parts = new List<GameObject>();

            //create objects
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].ParentBoneIdx != -1)
                {
                    pMatrix[i] = pMatrix[data[i].ParentBoneIdx] * data[i].TransMat.transpose;
                }
                else
                {
                    pMatrix[i] = data[i].TransMat.transpose;
                }

                //find part and add it do parts if exists
                //if it doesn't exist create it
                
                if (removeExt)
                {
                    string[] split = data[i].Name.Split('.');
                    data[i].Name = split[0];
                }

                var part = GameObject.Find(data[i].Name);
                if (part == null)
                    part = new GameObject(data[i].Name);
                Debug.Log(data[i].Name);
                parts.Add(part);

                part.transform.position = Utils.GetPosition(pMatrix[i]);
                part.transform.rotation = Utils.GetRotation(pMatrix[i]);
                part.transform.localScale = Utils.GetScale(pMatrix[i]);

                if (data[i].ParentBoneIdx != -1)
                    part.transform.SetParent(parts[data[i].ParentBoneIdx].transform, true);

            }
        }

    }
}
