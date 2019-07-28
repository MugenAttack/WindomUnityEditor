using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public struct hod_part
{
    public int level;
    public int children;
    public string part_name;
    public string part_path;
    public Vector3 localPosition;
    public Vector3 localScale;
    public Quaternion localRotation;
}

public class hod
{
    public string name;
    public int type;
    public hod_part[] parts;
    
    public void loadhod(string filename)
    {
        BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open));
        loadhod(ref br);
        br.Close();
    }

    public void loadhod(ref BinaryReader br)
    {
        long startpoint = br.BaseStream.Position;
        string signature = new string(br.ReadChars(3));

        if (signature == "HD2")
        {
            type = br.ReadInt32();
            int count = br.ReadInt32();
            parts = new hod_part[count];
            for (int i = 0; i < count; i++)
            {
                if (type == 0)
                {
                    br.BaseStream.Seek(startpoint + 11 + (i * 399), SeekOrigin.Begin);
                    parts[i].level = br.ReadInt32();
                    parts[i].children = br.ReadInt32();
                    parts[i].part_name = new string(br.ReadChars(256));
                    string[] split = parts[i].part_name.Split(".".ToCharArray());
                    parts[i].part_name = split[0];
                    //Debug.Log(parts[i].part_name);
                    parts[i].localRotation.x = br.ReadSingle();
                    parts[i].localRotation.y = br.ReadSingle();
                    parts[i].localRotation.z = br.ReadSingle();
                    parts[i].localRotation.w = br.ReadSingle();
                    parts[i].localScale.x = br.ReadSingle();
                    parts[i].localScale.y = br.ReadSingle();
                    parts[i].localScale.z = br.ReadSingle();
                    parts[i].localPosition.x = br.ReadSingle();
                    parts[i].localPosition.y = br.ReadSingle();
                    parts[i].localPosition.z = br.ReadSingle();
                }
                else if (type == 1)
                {
                    br.BaseStream.Seek(startpoint + 11 + (i * 179), SeekOrigin.Begin);
                    parts[i].level = br.ReadInt32();
                    parts[i].children = br.ReadInt32();
                    parts[i].localRotation.x = br.ReadSingle();
                    parts[i].localRotation.y = br.ReadSingle();
                    parts[i].localRotation.z = br.ReadSingle();
                    parts[i].localRotation.w = br.ReadSingle();
                    parts[i].localScale.x = br.ReadSingle();
                    parts[i].localScale.y = br.ReadSingle();
                    parts[i].localScale.z = br.ReadSingle();
                    parts[i].localPosition.x = br.ReadSingle();
                    parts[i].localPosition.y = br.ReadSingle();
                    parts[i].localPosition.z = br.ReadSingle();
                }
            }
        }
    }

    

    public void constructPath()
    {
        for (int i = 0; i < parts.Length; i++)
        {

        }
    }
}
