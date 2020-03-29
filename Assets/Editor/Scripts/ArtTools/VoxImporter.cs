using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using System.Collections.Generic;
using System;
using Unity.Mathematics;

namespace Zoxel.Voxels
{
    //public static string defaultShaderName = "HDRP/Lit";

    [ScriptedImporter(1, "vox")]
    public class VoxImporter : ScriptedImporter
    {
        public static string defaultShaderName = "Universal Render Pipeline/Lit";
        //public bool isExport;
        //public Material voxelMaterial;
        public float3 scale = new float3(1,1,1);
        public bool isItem;
        public TextureDatam itemTexture;
        [SerializeField]
        private VoxDatam vox;
        [SerializeField]
        private ItemDatam item;
        //[HideInInspector, SerializeField]
        private string voxAssetPath;
        string filename;
        public List<MaleSlot> maleSlots;
        public List<FemaleSlot> femaleSlots;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            voxAssetPath = ctx.assetPath;
            SaveVox(ctx);
            if (isItem)
            {
                if (item == null)
                {
                    item = ScriptableObject.CreateInstance<ItemDatam>();
                    item.name = filename + " Item";
                }
                if (item.Value.id == 0)
                {
                    item.Value.id = Bootstrap.GenerateUniqueID();
                }
                item.model = vox;
                item.Value.scale = 0.5f;
                item.texture = itemTexture;
                item.maleSlots = maleSlots;
                item.femaleSlots = femaleSlots;
                ctx.AddObjectToAsset(item.name, item);
            }
        }

        public void SaveVox(AssetImportContext ctx)
        {
            bool isPreExisting = vox != null;
            vox = ProcessVoxFile(vox);
            string startFolder = System.IO.Path.GetDirectoryName(voxAssetPath); //FileUtil.GetProjectRelativePath(voxAssetPath);
            ctx.AddObjectToAsset(vox.name, vox);
            ctx.SetMainObject(vox);
        }

        private VoxDatam ProcessVoxFile(VoxDatam model)
        {
            var voxel = VoxImport.VoxImportMethods.Load(voxAssetPath);
            filename = Path.GetFileNameWithoutExtension(voxAssetPath);
            if (model == null)
            {
                model = ScriptableObject.CreateInstance<VoxDatam>();
            }
            model.name = filename + " Vox";
            var priorID = model.data.id;
            if (priorID == 0)
            {
                priorID = Bootstrap.GenerateUniqueID();
            }
            model.data = new VoxData { 
                id = priorID
            };
            uint[] pallete = voxel.palette.values;  
            Color32[] colors = VoxImport.VoxImportMethods.CreateColor32FromPelatte(pallete);
            model.data.InitializeColors(colors.Length);
            for (int i = 0; i < colors.Length; i++)
            {
                model.data.colorsR[i] = colors[i].r;
                model.data.colorsG[i] = colors[i].g;
                model.data.colorsB[i] = colors[i].b;
            }
            var voxData = voxel.chunkChild[0].xyzi.voxels;
            byte voxelValue;
            model.data.size = new int3(voxData.x, voxData.y, voxData.z);
            model.data.InitializeData();
            int3 localPosition;
            for (localPosition.x = 0; localPosition.x < model.data.size.x; localPosition.x++)
            {
                for (localPosition.y = 0; localPosition.y < model.data.size.y; localPosition.y++)
                {
                    for (localPosition.z = 0; localPosition.z < model.data.size.z; localPosition.z++)
                    {
                        int modelVoxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(localPosition, model.data.size);
                        voxelValue = (byte)voxData.voxels[(int)localPosition.x, (int)localPosition.y, (int)localPosition.z];
                        if (voxelValue != 255)
                        {
                            model.data.data[modelVoxelIndex] = (byte)(voxelValue + 1);
                        }
                        else
                        {
                            model.data.data[modelVoxelIndex] = (byte)0;
                        }
                    }
                }
            }
            model.data.scale = scale;
            return model;
        }

    }
}

namespace VoxImport
{

    public class VoxFileData
    {
        public VoxFileHeader hdr;
        public VoxFileChunk main;
        public VoxFilePack pack;
        public VoxFileChunkChild[] chunkChild;
        public VoxFileRGBA palette;
    }

    public struct VoxFileHeader
    {
        public byte[] header;
        public Int32 version;
    }
    public struct VoxFileChunk
    {
        public byte[] name;
        public Int32 chunkContent;
        public Int32 chunkNums;
    }
    public struct VoxFilePack
    {
        public byte[] name;
        public Int32 chunkContent;
        public Int32 chunkNums;
        public Int32 modelNums;
    }
    public struct VoxFileChunkChild
    {
        public VoxFileSize size;
        public VoxFileXYZI xyzi;
    }

    public struct VoxFileRGBA
    {
        public byte[] name;
        public Int32 chunkContent;
        public Int32 chunkNums;
        public uint[] values;
    }


    public struct VoxFileSize
    {
        public byte[] name;
        public Int32 chunkContent;
        public Int32 chunkNums;
        public Int32 x;
        public Int32 y;
        public Int32 z;
    }

    public struct VoxFileXYZI
    {
        public byte[] name;
        public Int32 chunkContent;
        public Int32 chunkNums;
        public VoxData voxels;
    }
    public class VoxData
    {
        public int x, y, z;
        public int[,,] voxels;

        public VoxData()
        {
            x = 0; y = 0; z = 0;
        }

        public VoxData(byte[] _voxels, int xx, int yy, int zz)
        {
            x = xx;
            y = zz;
            z = yy;
            voxels = new int[x, y, z];

            for (int i = 0; i < x; ++i)
            {
                for (int j = 0; j < y; ++j)
                    for (int k = 0; k < z; ++k)
                        voxels[i, j, k] = int.MaxValue;
            }

            for (int j = 0; j < _voxels.Length; j += 4)
            {
                var x = _voxels[j];
                var y = _voxels[j + 1];
                var z = _voxels[j + 2];
                var c = _voxels[j + 3];

                voxels[x, z, y] = c;
            }
        }
    }

    public static class VoxImportMethods
    {
        public static Color32[] CreateColor32FromPelatte(uint[] palette)
        {
            Debug.Assert(palette.Length == 256);

            Color32[] colors = new Color32[256];

            for (uint j = 0; j < 256; j++)
            {
                uint rgba = palette[j];

                Color32 color = new Color32();
                color.r = (byte)((rgba >> 0) & 0xFF);
                color.g = (byte)((rgba >> 8) & 0xFF);
                color.b = (byte)((rgba >> 16) & 0xFF);
                color.a = (byte)((rgba >> 24) & 0xFF);

                colors[j] = color;
            }

            return colors;
        }

        private static uint[] _paletteDefault = new uint[256]
        {
                0x00000000, 0xffffffff, 0xffccffff, 0xff99ffff, 0xff66ffff, 0xff33ffff, 0xff00ffff, 0xffffccff, 0xffccccff, 0xff99ccff, 0xff66ccff, 0xff33ccff, 0xff00ccff, 0xffff99ff, 0xffcc99ff, 0xff9999ff,
                0xff6699ff, 0xff3399ff, 0xff0099ff, 0xffff66ff, 0xffcc66ff, 0xff9966ff, 0xff6666ff, 0xff3366ff, 0xff0066ff, 0xffff33ff, 0xffcc33ff, 0xff9933ff, 0xff6633ff, 0xff3333ff, 0xff0033ff, 0xffff00ff,
                0xffcc00ff, 0xff9900ff, 0xff6600ff, 0xff3300ff, 0xff0000ff, 0xffffffcc, 0xffccffcc, 0xff99ffcc, 0xff66ffcc, 0xff33ffcc, 0xff00ffcc, 0xffffcccc, 0xffcccccc, 0xff99cccc, 0xff66cccc, 0xff33cccc,
                0xff00cccc, 0xffff99cc, 0xffcc99cc, 0xff9999cc, 0xff6699cc, 0xff3399cc, 0xff0099cc, 0xffff66cc, 0xffcc66cc, 0xff9966cc, 0xff6666cc, 0xff3366cc, 0xff0066cc, 0xffff33cc, 0xffcc33cc, 0xff9933cc,
                0xff6633cc, 0xff3333cc, 0xff0033cc, 0xffff00cc, 0xffcc00cc, 0xff9900cc, 0xff6600cc, 0xff3300cc, 0xff0000cc, 0xffffff99, 0xffccff99, 0xff99ff99, 0xff66ff99, 0xff33ff99, 0xff00ff99, 0xffffcc99,
                0xffcccc99, 0xff99cc99, 0xff66cc99, 0xff33cc99, 0xff00cc99, 0xffff9999, 0xffcc9999, 0xff999999, 0xff669999, 0xff339999, 0xff009999, 0xffff6699, 0xffcc6699, 0xff996699, 0xff666699, 0xff336699,
                0xff006699, 0xffff3399, 0xffcc3399, 0xff993399, 0xff663399, 0xff333399, 0xff003399, 0xffff0099, 0xffcc0099, 0xff990099, 0xff660099, 0xff330099, 0xff000099, 0xffffff66, 0xffccff66, 0xff99ff66,
                0xff66ff66, 0xff33ff66, 0xff00ff66, 0xffffcc66, 0xffcccc66, 0xff99cc66, 0xff66cc66, 0xff33cc66, 0xff00cc66, 0xffff9966, 0xffcc9966, 0xff999966, 0xff669966, 0xff339966, 0xff009966, 0xffff6666,
                0xffcc6666, 0xff996666, 0xff666666, 0xff336666, 0xff006666, 0xffff3366, 0xffcc3366, 0xff993366, 0xff663366, 0xff333366, 0xff003366, 0xffff0066, 0xffcc0066, 0xff990066, 0xff660066, 0xff330066,
                0xff000066, 0xffffff33, 0xffccff33, 0xff99ff33, 0xff66ff33, 0xff33ff33, 0xff00ff33, 0xffffcc33, 0xffcccc33, 0xff99cc33, 0xff66cc33, 0xff33cc33, 0xff00cc33, 0xffff9933, 0xffcc9933, 0xff999933,
                0xff669933, 0xff339933, 0xff009933, 0xffff6633, 0xffcc6633, 0xff996633, 0xff666633, 0xff336633, 0xff006633, 0xffff3333, 0xffcc3333, 0xff993333, 0xff663333, 0xff333333, 0xff003333, 0xffff0033,
                0xffcc0033, 0xff990033, 0xff660033, 0xff330033, 0xff000033, 0xffffff00, 0xffccff00, 0xff99ff00, 0xff66ff00, 0xff33ff00, 0xff00ff00, 0xffffcc00, 0xffcccc00, 0xff99cc00, 0xff66cc00, 0xff33cc00,
                0xff00cc00, 0xffff9900, 0xffcc9900, 0xff999900, 0xff669900, 0xff339900, 0xff009900, 0xffff6600, 0xffcc6600, 0xff996600, 0xff666600, 0xff336600, 0xff006600, 0xffff3300, 0xffcc3300, 0xff993300,
                0xff663300, 0xff333300, 0xff003300, 0xffff0000, 0xffcc0000, 0xff990000, 0xff660000, 0xff330000, 0xff0000ee, 0xff0000dd, 0xff0000bb, 0xff0000aa, 0xff000088, 0xff000077, 0xff000055, 0xff000044,
                0xff000022, 0xff000011, 0xff00ee00, 0xff00dd00, 0xff00bb00, 0xff00aa00, 0xff008800, 0xff007700, 0xff005500, 0xff004400, 0xff002200, 0xff001100, 0xffee0000, 0xffdd0000, 0xffbb0000, 0xffaa0000,
                0xff880000, 0xff770000, 0xff550000, 0xff440000, 0xff220000, 0xff110000, 0xffeeeeee, 0xffdddddd, 0xffbbbbbb, 0xffaaaaaa, 0xff888888, 0xff777777, 0xff555555, 0xff444444, 0xff222222, 0xff111111
        };

        public static VoxFileData Load(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (stream == null)
                    throw new System.Exception("Failed to open file for FileStream.");

                using (var reader = new BinaryReader(stream))
                {
                    VoxFileData voxel = new VoxFileData();
                    voxel.hdr.header = reader.ReadBytes(4);
                    voxel.hdr.version = reader.ReadInt32();

                    if (voxel.hdr.header[0] != 'V' || voxel.hdr.header[1] != 'O' || voxel.hdr.header[2] != 'X' || voxel.hdr.header[3] != ' ')
                        throw new System.Exception("Bad Token: token is not VOX.");

                    if (voxel.hdr.version != 150)
                        throw new System.Exception("The version of file isn't 150 that version of vox, tihs version of file is " + voxel.hdr.version + ".");

                    voxel.main.name = reader.ReadBytes(4);
                    voxel.main.chunkContent = reader.ReadInt32();
                    voxel.main.chunkNums = reader.ReadInt32();

                    if (voxel.main.name[0] != 'M' || voxel.main.name[1] != 'A' || voxel.main.name[2] != 'I' || voxel.main.name[3] != 'N')
                        throw new System.Exception("Bad Token: token is not MAIN.");

                    if (voxel.main.chunkContent != 0)
                        throw new System.Exception("Bad Token: chunk content is " + voxel.main.chunkContent + ", it should be 0.");

                    if (reader.PeekChar() == 'P')
                    {
                        voxel.pack.name = reader.ReadBytes(4);
                        if (voxel.pack.name[0] != 'P' || voxel.pack.name[1] != 'A' || voxel.pack.name[2] != 'C' || voxel.pack.name[3] != 'K')
                            throw new System.Exception("Bad Token: token is not PACK");

                        voxel.pack.chunkContent = reader.ReadInt32();
                        voxel.pack.chunkNums = reader.ReadInt32();
                        voxel.pack.modelNums = reader.ReadInt32();

                        if (voxel.pack.modelNums == 0)
                            throw new System.Exception("Bad Token: model nums must be greater than zero.");
                    }
                    else
                    {
                        voxel.pack.chunkContent = 0;
                        voxel.pack.chunkNums = 0;
                        voxel.pack.modelNums = 1;
                    }

                    voxel.chunkChild = new VoxFileChunkChild[voxel.pack.modelNums];

                    for (int i = 0; i < voxel.pack.modelNums; i++)
                    {
                        var chunk = new VoxFileChunkChild();

                        chunk.size.name = reader.ReadBytes(4);
                        chunk.size.chunkContent = reader.ReadInt32();
                        chunk.size.chunkNums = reader.ReadInt32();
                        chunk.size.x = reader.ReadInt32();
                        chunk.size.y = reader.ReadInt32();
                        chunk.size.z = reader.ReadInt32();

                        if (chunk.size.name[0] != 'S' || chunk.size.name[1] != 'I' || chunk.size.name[2] != 'Z' || chunk.size.name[3] != 'E')
                            throw new System.Exception("Bad Token: token is not SIZE");

                        if (chunk.size.chunkContent != 12)
                            throw new System.Exception("Bad Token: chunk content is " + chunk.size.chunkContent + ", it should be 12.");

                        chunk.xyzi.name = reader.ReadBytes(4);
                        if (chunk.xyzi.name[0] != 'X' || chunk.xyzi.name[1] != 'Y' || chunk.xyzi.name[2] != 'Z' || chunk.xyzi.name[3] != 'I')
                            throw new System.Exception("Bad Token: token is not XYZI");

                        chunk.xyzi.chunkContent = reader.ReadInt32();
                        chunk.xyzi.chunkNums = reader.ReadInt32();
                        if (chunk.xyzi.chunkNums != 0)
                            throw new System.Exception("Bad Token: chunk nums is " + chunk.xyzi.chunkNums + ",i t should be 0.");

                        var voxelNums = reader.ReadInt32();
                        var voxels = new byte[voxelNums * 4];
                        if (reader.Read(voxels, 0, voxels.Length) != voxels.Length)
                            throw new System.Exception("Failed to read voxels");

                        chunk.xyzi.voxels = new VoxData(voxels, chunk.size.x, chunk.size.y, chunk.size.z);

                        voxel.chunkChild[i] = chunk;
                    }

                    if (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        byte[] palette = reader.ReadBytes(4);
                        if (palette[0] != 'R' || palette[1] != 'G' || palette[2] != 'B' || palette[3] != 'A')
                        {
                            var str = System.Text.Encoding.Default.GetString(palette);
                            throw new System.Exception("Bad Token: token is not RGBA: " + str.ToString());

                        }

                        voxel.palette.chunkContent = reader.ReadInt32();
                        voxel.palette.chunkNums = reader.ReadInt32();

                        var bytePalette = new byte[voxel.palette.chunkContent];
                        reader.Read(bytePalette, 0, voxel.palette.chunkContent);

                        voxel.palette.values = new uint[voxel.palette.chunkContent / 4];

                        for (int i = 4; i < bytePalette.Length; i += 4)
                            voxel.palette.values[i / 4] = BitConverter.ToUInt32(bytePalette, i - 4);
                    }
                    else
                    {
                        voxel.palette.values = new uint[256];
                        _paletteDefault.CopyTo(voxel.palette.values, 0);
                    }

                    return voxel;
                }
            }
        }
    }
}




/*
void CalculateBoneDistances(Vector3 vertexPosition, ref Transform[] bones, ref float[] boneInfluenceRadius, out float[] boneDistances, out int closestIndex)
{
    boneDistances = new float[bones.Length];
    float closestDistance = 10000;
    closestIndex = -1;
    for (int i = 0; i < bones.Length; i++)
    {
        boneDistances[i] = Vector3.Distance(bones[i].position, vertexPosition);
        // check if distance is less then others, check if is in bones influence radius
        if (boneDistances[i] < closestDistance && boneDistances[i] < boneInfluenceRadius[i])
        {
            closestDistance = boneDistances[i];
            closestIndex = i;
        }
    }
}


void GenerateSkeleton(MeshRenderer meshRenderer, GameObject voxModel, Material material, int i, Transform child, Mesh mesh)
{
    // if (isSkinnedMesh)
    {
        // remove mesh renderer and add skinned meshRendeerer
        DestroyImmediate(meshRenderer);
        SkinnedMeshRenderer skinnyMesh = voxModel.transform.GetChild(i).gameObject.AddComponent<SkinnedMeshRenderer>();
        skinnyMesh.sharedMaterial = material;
        // generate weights
        //mesh.boneWeights = new 

        // first get our list of bones
        List<Vector3> bonePositions = new List<Vector3>();
        List<string> boneNames = new List<string>();
        List<string> boneParents = new List<string>();
        List<float> boneInfluences = new List<float>();
        // hips
        boneNames.Add("Hip");
        boneParents.Add("Root");
        bonePositions.Add(new Vector3(0, -0.175f, 0));
        boneInfluences.Add(0.5f);
        // chest
        boneNames.Add("Chest");
        boneParents.Add("Hip");
        bonePositions.Add(new Vector3(0, 0.4f, -0.1f));
        boneInfluences.Add(0.4f);
        // head
        boneNames.Add("Head");
        boneParents.Add("Chest");
        bonePositions.Add(new Vector3(0, 0.75f, -0.1f));
        boneInfluences.Add(0.3f);

        // Right Arm
        boneNames.Add("RightShoulder");
        boneParents.Add("Chest");
        bonePositions.Add(new Vector3(0.22f, 0.4f, -0.1f));
        boneInfluences.Add(0.19f);
        boneNames.Add("RightElbow");
        boneParents.Add("RightShoulder");
        bonePositions.Add(new Vector3(0.325f, 0.175f, -0.1f));
        boneInfluences.Add(0.16f);
        boneNames.Add("RightHand");
        boneParents.Add("RightElbow");
        bonePositions.Add(new Vector3(0.325f, 0, -0.1f));
        boneInfluences.Add(0.12f);

        // Right Arm
        boneNames.Add("LeftShoulder");
        boneParents.Add("Chest");
        bonePositions.Add(new Vector3(-0.22f, 0.4f, -0.1f));
        boneInfluences.Add(0.19f);
        boneNames.Add("LeftElbow");
        boneParents.Add("LeftShoulder");
        bonePositions.Add(new Vector3(-0.325f, 0.175f, -0.1f));
        boneInfluences.Add(0.16f);
        boneNames.Add("LeftHand");
        boneParents.Add("LeftElbow");
        bonePositions.Add(new Vector3(-0.325f, 0, -0.1f));
        boneInfluences.Add(0.12f);

        // Left Leg
        boneNames.Add("LeftKnee");
        boneParents.Add("Hip");
        bonePositions.Add(new Vector3(0.2f, -0.575f, -0.1f));
        boneInfluences.Add(0.44f);
        boneNames.Add("LeftFoot");
        boneParents.Add("LeftKnee");
        bonePositions.Add(new Vector3(0.2f, -0.8f, -0.1f));
        boneInfluences.Add(0.5f);

        // Right Leg
        boneNames.Add("RightKnee");
        boneParents.Add("Hip");
        bonePositions.Add(new Vector3(-0.2f, -0.575f, -0.1f));
        boneInfluences.Add(0.44f);
        boneNames.Add("RightFoot");
        boneParents.Add("RightKnee");
        bonePositions.Add(new Vector3(-0.2f, -0.8f, -0.1f));
        boneInfluences.Add(0.5f);


        BoneWeight[] weights = new BoneWeight[mesh.vertexCount];
        Transform[] boneTransforms = new Transform[bonePositions.Count];
        Matrix4x4[] bindPoses = new Matrix4x4[bonePositions.Count];
        float[] boneInfluenceRadius = new float[bonePositions.Count];
        for (int j = 0; j < boneTransforms.Length; j++)
        {
            boneTransforms[j] = new GameObject(boneNames[j]).transform;// "Bone" + j).transform;
                                                                       //boneInfluenceRadius[j] = 0.5f;
            int boneParentIndex = boneNames.IndexOf(boneParents[j]);
            if (boneParentIndex != -1)
            {
                boneTransforms[j].parent = boneTransforms[boneParentIndex];
            }
            else
            {
                boneTransforms[j].parent = child.transform;
            }
            boneInfluenceRadius[j] = boneInfluences[j];// 0.1f;
            boneTransforms[j].position = bonePositions[j];
            bindPoses[j] = boneTransforms[j].worldToLocalMatrix * child.transform.localToWorldMatrix;
        }
        for (int j = 0; j < weights.Length; j++)
        {
            // get vertex point
            Vector3 vertex = mesh.vertices[j];
            float[] boneDistances;
            int closestIndex;
            CalculateBoneDistances(vertex, ref boneTransforms, ref boneInfluenceRadius, out boneDistances, out closestIndex);
            weights[j].boneIndex0 = closestIndex;
            weights[j].weight0 = 1;
        }
        mesh.boneWeights = weights;
        mesh.bindposes = bindPoses;
        skinnyMesh.rootBone = child.transform;
        skinnyMesh.bones = boneTransforms;
        skinnyMesh.sharedMesh = mesh;
        //ctx.AddObjectToAsset(filename + "_" + (i + 1), mesh);
    }
}

void FlipNormals(ref Mesh mesh)
{
    int[] tris = mesh.triangles;
    for (int i = 0; i < tris.Length / 3; i++)
    {
        int a = tris[i * 3 + 0];
        int b = tris[i * 3 + 1];
        int c = tris[i * 3 + 2];
        tris[i * 3 + 0] = c;
        tris[i * 3 + 1] = b;
        tris[i * 3 + 2] = a;
    }
    mesh.triangles = tris;
}*/



/*public struct VoxFileMaterial
{
    public int id;
    public int type;
    public float weight;
    public int propertyBits;
    public float[] propertyValue;
}*/
/*public int count
{
    get
    {
        int _count = 0;

        for (int i = 0; i < x; ++i)
        {
            for (int j = 0; j < y; ++j)
                for (int k = 0; k < z; ++k)
                    if (voxels[i, j, k] != int.MaxValue)
                        _count++;
        }

        return _count;
    }
}*/
/*
public int GetMajorityColorIndex(int xx, int yy, int zz, int lodLevel)
{
    xx = Mathf.Min(xx, x - 2);
    yy = Mathf.Min(yy, y - 2);
    zz = Mathf.Min(zz, z - 2);

    int[] samples = new int[lodLevel * lodLevel * lodLevel];

    for (int i = 0; i < lodLevel; i++)
    {
        for (int j = 0; j < lodLevel; j++)
        {
            for (int k = 0; k < lodLevel; k++)
            {
                if (xx + i > x - 1 || yy + j > y - 1 || zz + k > z - 1)
                    samples[i * lodLevel * lodLevel + j * lodLevel + k] = int.MaxValue;
                else
                    samples[i * lodLevel * lodLevel + j * lodLevel + k] = voxels[xx + i, yy + j, zz + k];
            }
        }
    }

    int maxNum = 1;
    int maxNumIndex = 0;

    int[] numIndex = new int[samples.Length];

    for (int i = 0; i < samples.Length; i++)
        numIndex[i] = samples[i] == int.MaxValue ? 0 : 1;

    for (int i = 0; i < samples.Length; i++)
    {
        for (int j = 0; j < samples.Length; j++)
        {
            if (i != j && samples[i] != int.MaxValue && samples[i] == samples[j])
            {
                numIndex[i]++;
                if (numIndex[i] > maxNum)
                {
                    maxNum = numIndex[i];
                    maxNumIndex = i;
                }
            }
        }
    }

    return samples[maxNumIndex];
}

public VoxData GetVoxelDataLOD(int level)
{
    if (x <= 1 || y <= 1 || z <= 1)
        return null;

    level = Mathf.Clamp(level, 0, 16);
    if (level <= 1)
        return this;

    if (x <= level && y <= level && z <= level)
        return this;

    VoxData data = new VoxData();
    data.x = Mathf.CeilToInt((float)x / level);
    data.y = Mathf.CeilToInt((float)y / level);
    data.z = Mathf.CeilToInt((float)z / level);

    data.voxels = new int[data.x, data.y, data.z];

    for (int x = 0; x < data.x; x++)
    {
        for (int y = 0; y < data.y; y++)
        {
            for (int z = 0; z < data.z; z++)
            {
                data.voxels[x, y, z] = this.GetMajorityColorIndex(x * level, y * level, z * level, level);
            }
        }
    }

    return data;
}*/

// two transform points
/*Vector3 topBone = new Vector3(0, 0.75f, 0);
Transform topBoneTransform = new GameObject("TopBone").transform;
topBoneTransform.parent = child.transform;
topBoneTransform.localPosition = topBone;
Vector3 bottomBone = new Vector3(0, -0.75f, 0);
Transform bottomBoneTransform = new GameObject("BottomBone").transform;
bottomBoneTransform.parent = child.transform;
bottomBoneTransform.localPosition = bottomBone;*/
//var cube = LoadVoxelFileAsPrefab(name, voxel, lodLevel);

//var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
//var position = JsonUtility.FromJson<Vector3>();

//cube.transform.position = Vector3.zero;// position;
//cube.transform.localScale = new Vector3(m_Scale, m_Scale, m_Scale);

// 'cube' is a a GameObject and will be automatically converted into a prefab
// (Only the 'Main Asset' is elligible to become a Prefab.)

//var material = new Material(Shader.Find("Standard"));
//material.color = Color.red;

// Assets must be assigned a unique identifier string consistent across imports
//ctx.AddObjectToAsset("my Material", material);

// Assets that are not passed into the context as import outputs must be destroyed
//var tempMesh = new Mesh();
//DestroyImmediate(tempMesh);

/*float distanceToTop = Vector3.Distance(vertex, topBone);
float distanceToBottom = Vector3.Distance(vertex, bottomBone);
// check if point closer to top or bottom
if (distanceToTop < distanceToBottom)
{
    // Bottom bone is closer
    weights[j].boneIndex0 = 0;
    weights[j].boneIndex1 = 1;
    weights[j].weight0 = 1;
    weights[j].weight1 = 0;
}
else
{
    // top bone is closer
    weights[j].boneIndex0 = 0;
    weights[j].boneIndex1 = 1;
    weights[j].weight0 = 0;
    weights[j].weight1 = 1;
    //weights[j].weight0 = distanceToTop;//1;
    //weights[j].weight1 = distanceToBottom;// 0;
}
if (distanceToTop - 0.1f >= distanceToBottom && distanceToTop + 0.1f <= distanceToBottom)
{
    // if distances are close!
    weights[j].weight0 = distanceToTop;
    weights[j].weight1 = distanceToBottom;
}*/

// ctx.set

//VoxFileImport.isOptimizeMesh = isOptimizeMesh;
//VoxFileImport.mode = mode;
//string file = File.ReadAllText(ctx.voxAssetPath);
//Debug.LogError("Importing file: " + ctx.voxAssetPath + "::" + file.Length);
//string outpath = "Assets/";
//Debug.LogError("LOading asset path: " + voxAssetPath)/

/*Debug.LogError("Creating vox model: " + map.name + " - " + map.Value.size);
ctx.AddObjectToAsset(map.name, map);
ctx.SetMainObject(map);*/
//{
//}
//voxModel.transform.localScale = new Vector3(1 / 16f, 1 / 16f, 1 / 16f);
// Add voxel mesh


/*if (voxModel.GetComponent<MeshFilter>() != null)
{
    ctx.AddObjectToAsset(Path.GetFileNameWithoutExtension(ctx.voxAssetPath), 
        voxModel.GetComponent<MeshFilter>().mesh);
}
Mesh mesh = null;
Material material = null;
for (int i = 0; i < voxModel.transform.childCount; i++)
{
    GameObject child = voxModel.transform.GetChild(i).gameObject;
    MeshFilter meshFilter = child.GetComponent<MeshFilter>();
    MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();

    if (meshFilter != null)
    {
        mesh = meshFilter.sharedMesh;
        mesh.name = filename + "_Mesh_" + (i + 1);
        // centre the mesh
        Vector3 buffer = new Vector3(-0.5f, 0, -0.5f);
        Vector3[] verts = mesh.vertices;
        // min max
        Vector3 minVector = new Vector3(10000, 10000, 10000);
        Vector3 maxVector = new Vector3(-10000, -10000, -10000);
        for (int a = 0; a < mesh.vertexCount; a++)
        {
            if (verts[a].x < minVector.x)
            {
                minVector.x = verts[a].x;
            }
            if (verts[a].y < minVector.y)
            {
                minVector.y = verts[a].y;
            }
            if (verts[a].z < minVector.z)
            {
                minVector.z = verts[a].z;
            }

            if (verts[a].x > maxVector.x)
            {
                maxVector.x = verts[a].x;
            }
            if (verts[a].y > maxVector.y)
            {
                maxVector.y = verts[a].y;
            }
            if (verts[a].z > maxVector.z)
            {
                maxVector.z = verts[a].z;
            }
        }
        Vector3 size = maxVector - minVector; // we should get a positive size
        for (int a = 0; a < mesh.vertexCount; a++)
        {
            verts[a] = (verts[a] - size / 2f) / 16f;
            verts[a] = new Vector3(verts[a].x, verts[a].y, -verts[a].z) * scale;
        }
        mesh.vertices = verts;
        FlipNormals(ref mesh);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;
        if (skeleton == null)
        {
            ctx.AddObjectToAsset(filename + "_" + (i + 1), mesh);
        }
    }
    if (meshRenderer != null)
    {
        Texture texture = meshRenderer.sharedMaterial.mainTexture;
        texture.filterMode = FilterMode.Point;
        ctx.AddObjectToAsset(filename + "_Tex_" + (i + 1), texture);
        //ctx.AddObjectToAsset(filename + "_Mat_" + (i + 1), meshRenderer.material);
        material = new Material(Shader.Find(defaultShaderName));// "ForwardLit"));
        material.name = filename + "_Mat_" + (i + 1);
        material.SetTexture("_BaseMap", texture);
        meshRenderer.sharedMaterial = material;
        ctx.AddObjectToAsset(filename + "_Mat_" + (i + 1), material);
    }
    mesh = meshFilter.sharedMesh;
    material = meshRenderer.sharedMaterial;

    if (skeleton)
    {
        // as it was made by voxImporter
        DestroyImmediate(meshRenderer);
        SkinnedMeshRenderer skinnyMesh = voxModel.transform.GetChild(i).gameObject.AddComponent<SkinnedMeshRenderer>();
        skinnyMesh.sharedMesh = mesh;
        skinnyMesh.sharedMaterial = material;
        skeleton.BakeMeshWeights(skinnyMesh);
        mesh = skinnyMesh.sharedMesh;
        skinnyMesh.rootBone = child.transform;
        skinnyMesh.bones = skeleton.GetBoneTransforms();
        skinnyMesh.sharedMesh = mesh;
        ctx.AddObjectToAsset(filename, mesh);
    }
}
if (mesh != null)
{
    map.bakedMesh = mesh;
}
if (material != null)
{
    map.bakedMaterial = material;
}*/
//ctx.AddObjectToAsset(Path.GetFileNameWithoutExtension(ctx.voxAssetPath), voxModel);
//ctx.SetMainObject(voxModel);