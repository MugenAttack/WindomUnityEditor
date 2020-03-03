using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


public class Ani2
{
    public hod2v0 structure;
    public List<animation> animations;

    public bool load(string filename)
    {
        BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read));

        string signature = new string(br.ReadChars(3));
        if (signature == "AN2")
        {
            animations = new List<animation>();
            Encoding ShiftJis = Encoding.GetEncoding("shift_jis");
            string robohod = ShiftJis.GetString(br.ReadBytes(256)).TrimEnd('\0');
            structure = new hod2v0(robohod);
            structure.loadFromBinary(ref br);

            int aCount = br.ReadInt32();
            for (int i = 0; i < aCount; i++)
            {
                animation aData = new animation();
                aData.loadFromAni(ref br, ref structure);
                animations.Add(aData);
            }
        }
        else if (signature == "ANI")
        {
            animations = new List<animation>();
            Encoding ShiftJis = Encoding.GetEncoding("shift_jis");
            string robohod = ShiftJis.GetString(br.ReadBytes(256)).TrimEnd('\0');
            hod1 oldStructure = new hod1(robohod);
            oldStructure.loadFromBinary(ref br);
            structure = oldStructure.convertToHod2v0();
            for (int i = 0; i < 200; i++)
            {
                animation aData = new animation();
                aData.loadFromAniOld(ref br);
                animations.Add(aData);
            }
        }
        br.Close();

        return true;
    }
}

public struct script
{
    public int unk;
    public float time;
    public string squirrel;
}

public class animation
{
    public string name;
    public string squirrelInit = "";
    public List<hod2v1> frames;
    public List<script> scripts;
    public void loadFromAni(ref BinaryReader br, ref hod2v0 structure)
    {
        frames = new List<hod2v1>();
        scripts = new List<script>();

        //load Name
        Encoding ShiftJis = Encoding.GetEncoding("shift_jis");
        name = ShiftJis.GetString(br.ReadBytes(256)).TrimEnd('\0');

        //Read Initial Script
        int textLength = br.ReadInt32();
        if (textLength != 0)
        {
            squirrelInit = ShiftJis.GetString(br.ReadBytes(textLength));
        }

        //Read Hod Files
        int hodCount = br.ReadInt32();
        for (int i = 0; i < hodCount; i++)
        {
            short nameLength = br.ReadInt16();
            hod2v1 nHod = new hod2v1(ShiftJis.GetString(br.ReadBytes(nameLength)));
            nHod.loadFromBinary(ref br, ref structure);
            frames.Add(nHod);
        }

        //Read Script Files
        int scriptCount = br.ReadInt32();
        for (int i = 0; i < scriptCount; i++)
        {
            script ns = new script();
            ns.unk = br.ReadInt32();
            ns.time = br.ReadSingle();
            textLength = br.ReadInt32();
            ns.squirrel = ShiftJis.GetString(br.ReadBytes(textLength));
            scripts.Add(ns);
        }
    }

    public void loadFromAniOld(ref BinaryReader br)
    {
        frames = new List<hod2v1>();
        scripts = new List<script>();

        //load Name
        Encoding ShiftJis = Encoding.GetEncoding("shift_jis");
        name = ShiftJis.GetString(br.ReadBytes(256)).TrimEnd('\0');

        //Read Hod Files
        int hodCount = br.ReadInt32();
        for (int i = 0; i < hodCount; i++)
        {
            hod1 nHod = new hod1(ShiftJis.GetString(br.ReadBytes(30)).TrimEnd('\0'));
            nHod.loadFromBinary(ref br);
            frames.Add(nHod.convertToHod2v1());
        }

        //Read Script Files
        int scriptCount = br.ReadInt32();
        for (int i = 0; i < scriptCount; i++)
        {
            script ns = new script();
            ns.unk = br.ReadInt32();
            ns.time = br.ReadSingle();
            int textLength = br.ReadInt32();
            ns.squirrel = ShiftJis.GetString(br.ReadBytes(textLength));
            scripts.Add(ns);
        }
    }
}

public struct hod2v0_Part
{
    public int treeDepth;
    public int childCount;
    public string name;
    public Quaternion rotation;
    public Vector3 scale;
    public Vector3 position;
    public byte flag;
    public Vector3 unk;
}

public class hod2v0
{
    public string filename;
    public List<hod2v0_Part> parts;
    public hod2v0(string name)
    {
        filename = name;
    }

    public bool loadFromBinary(ref BinaryReader br)
    {

        //data = br.ReadBytes(11 + (partCount * 399));
        string signature = new string(br.ReadChars(3));
        int version = br.ReadInt32();
        if (signature == "HD2" && version == 0)
        {
            parts = new List<hod2v0_Part>();
            int partCount = br.ReadInt32();
            for (int i = 0; i < partCount; i++)
            {
                hod2v0_Part nPart = new hod2v0_Part();
                nPart.treeDepth = br.ReadInt32();
                nPart.childCount = br.ReadInt32();
                nPart.name = ASCIIEncoding.ASCII.GetString(br.ReadBytes(256)).TrimEnd('\0');
                nPart.rotation = new Quaternion();
                nPart.rotation.x = br.ReadSingle();
                nPart.rotation.y = br.ReadSingle();
                nPart.rotation.z = br.ReadSingle();
                nPart.rotation.w = br.ReadSingle();
                nPart.scale = new Vector3();
                nPart.scale.x = br.ReadSingle();
                nPart.scale.y = br.ReadSingle();
                nPart.scale.z = br.ReadSingle();
                nPart.position = new Vector3();
                nPart.position.x = br.ReadSingle();
                nPart.position.y = br.ReadSingle();
                nPart.position.z = br.ReadSingle();
                nPart.flag = br.ReadByte();
                nPart.unk = new Vector3();
                nPart.unk.x = br.ReadSingle();
                nPart.unk.y = br.ReadSingle();
                nPart.unk.z = br.ReadSingle();
                br.BaseStream.Seek(82, SeekOrigin.Current);
                parts.Add(nPart);
            }
        }
        else
            return false;

        return true;
    }
}

public struct hod2v1_Part
{
    public string name;
    public int treeDepth;
    public int childCount;
    public Quaternion rotation;
    public Vector3 scale;
    public Vector3 position;
    public Quaternion unk1;
    public Quaternion unk2;
    public Quaternion unk3;
}

public class hod2v1
{
    public string filename;
    //public byte[] data;
    public List<hod2v1_Part> parts;
    public hod2v1(string name)
    {
        filename = name;
    }

    public bool loadFromBinary(ref BinaryReader br, ref hod2v0 structure)
    {

        //data = br.ReadBytes(11 + (partCount * 179));
        string signature = new string(br.ReadChars(3));
        int version = br.ReadInt32();
        if (signature == "HD2" && version == 1)
        {
            parts = new List<hod2v1_Part>();
            int partCount = br.ReadInt32();
            for (int i = 0; i < partCount; i++)
            {
                hod2v1_Part nPart = new hod2v1_Part();
                nPart.name = structure.parts[i].name;
                nPart.treeDepth = br.ReadInt32();
                nPart.childCount = br.ReadInt32();
                nPart.rotation = new Quaternion();
                nPart.rotation.x = br.ReadSingle();
                nPart.rotation.y = br.ReadSingle();
                nPart.rotation.z = br.ReadSingle();
                nPart.rotation.w = br.ReadSingle();
                nPart.scale = new Vector3();
                nPart.scale.x = br.ReadSingle();
                nPart.scale.y = br.ReadSingle();
                nPart.scale.z = br.ReadSingle();
                nPart.position = new Vector3();
                nPart.position.x = br.ReadSingle();
                nPart.position.y = br.ReadSingle();
                nPart.position.z = br.ReadSingle();
                nPart.unk1 = new Quaternion();
                nPart.unk1.x = br.ReadSingle();
                nPart.unk1.y = br.ReadSingle();
                nPart.unk1.z = br.ReadSingle();
                nPart.unk1.w = br.ReadSingle();
                nPart.unk2 = new Quaternion();
                nPart.unk2.x = br.ReadSingle();
                nPart.unk2.y = br.ReadSingle();
                nPart.unk2.z = br.ReadSingle();
                nPart.unk2.w = br.ReadSingle();
                nPart.unk3 = new Quaternion();
                nPart.unk3.x = br.ReadSingle();
                nPart.unk3.y = br.ReadSingle();
                nPart.unk3.z = br.ReadSingle();
                nPart.unk3.w = br.ReadSingle();
                br.BaseStream.Seek(83, SeekOrigin.Current);
                parts.Add(nPart);
            }
        }
        else
            return false;

        return true;
    }
}

struct hod1_Part
{
    public int treeDepth;
    public int childCount;
    public string name;
    public Matrix4x4 transform;
}

class hod1
{
    string filename;
    public List<hod1_Part> parts;
    public hod1(string name)
    {
        filename = name;
    }
    public bool loadFromBinary(ref BinaryReader br)
    {
        string signature = new string(br.ReadChars(3));
        if (signature == "HOD")
        {
            parts = new List<hod1_Part>();
            int partCount = br.ReadInt32();
            for (int i = 0; i < partCount; i++)
            {
                hod1_Part nPart = new hod1_Part();
                nPart.treeDepth = br.ReadInt32();
                nPart.childCount = br.ReadInt32();
                nPart.name = ASCIIEncoding.ASCII.GetString(br.ReadBytes(256)).TrimEnd('\0');
                nPart.transform = new Matrix4x4();
                nPart.transform.m00 = br.ReadInt32();
                nPart.transform.m01 = br.ReadInt32();
                nPart.transform.m02 = br.ReadInt32();
                nPart.transform.m03 = br.ReadInt32();
                nPart.transform.m10 = br.ReadInt32();
                nPart.transform.m11 = br.ReadInt32();
                nPart.transform.m12 = br.ReadInt32();
                nPart.transform.m13 = br.ReadInt32();
                nPart.transform.m20 = br.ReadInt32();
                nPart.transform.m21 = br.ReadInt32();
                nPart.transform.m22 = br.ReadInt32();
                nPart.transform.m23 = br.ReadInt32();
                nPart.transform.m30 = br.ReadInt32();
                nPart.transform.m31 = br.ReadInt32();
                nPart.transform.m32 = br.ReadInt32();
                nPart.transform.m33 = br.ReadInt32();
                parts.Add(nPart);
            }
        }
        else
            return false;

        return true;
    }

    public hod2v0 convertToHod2v0()
    {
        hod2v0 hod = new hod2v0(filename);
        hod.parts = new List<hod2v0_Part>();
        for (int i = 0; i < parts.Count; i++)
        {
            hod2v0_Part nPart = new hod2v0_Part();
            nPart.treeDepth = parts[i].treeDepth;
            nPart.childCount = parts[i].childCount;
            nPart.name = parts[i].name;
            nPart.rotation = Utils.GetRotation(parts[i].transform);
            nPart.scale = Utils.GetScale(parts[i].transform);
            nPart.position = Utils.GetPosition(parts[i].transform);
            nPart.flag = 1;
            nPart.unk = new Vector3(1.0f, 1.0f, 1.0f);
            hod.parts.Add(nPart);
        }
        return hod;
    }

    public hod2v1 convertToHod2v1()
    {
        hod2v1 hod = new hod2v1(filename);
        hod.parts = new List<hod2v1_Part>();
        for (int i = 0; i < parts.Count; i++)
        {
            hod2v1_Part nPart = new hod2v1_Part();
            nPart.treeDepth = parts[i].treeDepth;
            nPart.childCount = parts[i].childCount;
            nPart.name = parts[i].name;
            nPart.rotation = Utils.GetRotation(parts[i].transform);
            nPart.scale = Utils.GetScale(parts[i].transform);
            nPart.position = Utils.GetPosition(parts[i].transform);
            nPart.unk1 = Utils.GetRotation(parts[i].transform);
            nPart.unk2 = Utils.GetRotation(parts[i].transform);
            nPart.unk3 = Utils.GetRotation(parts[i].transform);
            hod.parts.Add(nPart);
        }
        return hod;
    }
}