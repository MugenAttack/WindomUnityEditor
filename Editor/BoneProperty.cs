using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public struct BpBoneData
{
    public string Name;
    public int Level;
    public int ParentBoneIdx;
    public Matrix4x4 TransMat;
    public Matrix4x4 OffsetMat;
    public int EulerMode;
    public int BoneLayers;
    public int[] BoneFlag;
    public float[] LimitAng;
    public string Windom_FileName;
    public int Windom_Hide;

};

public static class BoneProperty
{

    public static BpBoneData[] Read(string path)
    {
        XmlDocument Doc = new XmlDocument();
        Doc.Load(path);
        XmlNode Bp = Doc.SelectSingleNode("BoneProperty");

        BpBoneData[] Data = new BpBoneData[int.Parse(Bp.Attributes["Count"].Value)];

        for (int i = 0; i < Data.Length; i++)
        {
            XmlNode Bone = Bp.ChildNodes[i];
            Data[i].Name = Bone.Name;
            Data[i].Level = int.Parse(Bone.ChildNodes[0].Attributes["Value"].Value);
            Data[i].ParentBoneIdx = int.Parse(Bone.ChildNodes[1].Attributes["Value"].Value);
            Data[i].TransMat = toMatrix(Bone.ChildNodes[2].InnerText);
            Data[i].OffsetMat = toMatrix(Bone.ChildNodes[3].InnerText);
            Data[i].EulerMode = int.Parse(Bone.ChildNodes[4].Attributes["Value"].Value);
            Data[i].BoneLayers = int.Parse(Bone.ChildNodes[5].Attributes["Value"].Value);
            Data[i].BoneFlag = new int[2];
            Data[i].BoneFlag[0] = int.Parse(Bone.ChildNodes[6].Attributes["Value"].Value);
            Data[i].BoneFlag[1] = int.Parse(Bone.ChildNodes[6].Attributes["Value2"].Value);
            Data[i].LimitAng = toLimitAng(Bone.ChildNodes[7].InnerText);
            Data[i].Windom_FileName = Bone.ChildNodes[8].Attributes["Text"].Value;
            Data[i].Windom_Hide = int.Parse(Bone.ChildNodes[9].Attributes["Value"].Value);
        }

        return Data;
    }

    public static void Write(BpBoneData[] Data)
    {

    }

    public static Matrix4x4 toMatrix(string Val)
    {
        string[] s = Val.Split(" ".ToCharArray());

        Matrix4x4 m = new Matrix4x4
        {
            m00 = float.Parse(s[0]),
            m01 = float.Parse(s[1]),
            m02 = float.Parse(s[2]),
            m03 = float.Parse(s[3]),
            m10 = float.Parse(s[4]),
            m11 = float.Parse(s[5]),
            m12 = float.Parse(s[6]),
            m13 = float.Parse(s[7]),
            m20 = float.Parse(s[8]),
            m21 = float.Parse(s[9]),
            m22 = float.Parse(s[10]),
            m23 = float.Parse(s[11]),
            m30 = float.Parse(s[12]),
            m31 = float.Parse(s[13]),
            m32 = float.Parse(s[14]),
            m33 = float.Parse(s[15])
        };

        return m;
    }

    public static float[] toLimitAng(string val)
    {
        string[] s = val.Split(" ".ToCharArray());
        float[] v = new float[6];

        for (int j = 0; j < 6; j++)
            v[j] = float.Parse(s[j]);

        return v;
    }
}
