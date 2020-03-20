using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using AnimatorSystem;

namespace Zoxel.Voxels
{
    public class AnimationBaker : EditorWindow
    {
        public struct VertInfo
        {
            public Vector3 position;
            public Vector3 normal;
            public float extra;
        }
        private ComputeShader infoTexGen;
        private Shader playShader;

        // file storing paths
        private string outputPath;  // base path
        private string positionTexturePath;
        private string normalTexturePath;
        private string meshPath;
        //private int dataPathLength;
        // used for baking
        private GameObject prefab;
        private Texture2D positionTexture;
        private Texture2D normalTexture;

        private List<string> debugLog = new List<string>();
        public VoxDatam vox = null;
        public SkeletonDatam skeleton;
        public AnimationClip newAnim;
        public List<AnimationClip> animations = new List<AnimationClip>();
        GameObject newSkinny;
        //private bool isNewMethod = true;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Zoxel/AnimationBaker")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            AnimationBaker window = (AnimationBaker)EditorWindow.GetWindow(typeof(AnimationBaker));
            window.titleContent = new GUIContent("Animation Baker");
            window.Show();
        }


        public int GetDataPathLength()
        {
            return Application.dataPath.Length - 6;
        }

        void OnGUI()
        {
            // Select a VoxelDatam
            // Select a skeleton as well!
            // then it creates the prefab
            // with skinned mesh
            // you chose some animations as well
            // no need to chose prefab

            // perhaps step 1 can be to chose a voxel mesh
            // step 2 can be to chose a skeleton
            // step 3 is to chose some animations for that skeleton
            // then it will bake them into textures / animation file

            // after baked - show baked files
            // have an option to close / go back
            // You have baked Timmy, which 3 animation
            // then links to them
            // Then it links to the textures
            // and also to the animatorData file

            // I must link up the animator data file to the monster data (automate this)
            // then load it into the game

            //outputPath = GUILayout.TextField(outputPath);
            GUILayout.Label("Welcome to Animation Baker.");
            //GUILayout.Label("It can be animations of a prefab into textures.");
            //GUILayout.Label("It also outputs an animation file which is needed \nfor the animation system in the game.");
            NewWay();
        }

        public VoxDatam monster;
        private void NewWay()
        {
            GUILayout.Space(15);
            GUILayout.Label("Chose a Monster:");
            VoxDatam newMonster = EditorGUILayout.ObjectField(monster, typeof(VoxDatam), false) as VoxDatam;
            if (newMonster != monster)
            {
                monster = newMonster;
                vox = monster;
                outputPath = Path.GetDirectoryName(Path.GetFullPath(AssetDatabase.GetAssetPath(vox))) + "\\";
            }
            GUILayout.Label("Chose a Skeleton:");
            SkeletonDatam newSkeleton = EditorGUILayout.ObjectField(skeleton, typeof(SkeletonDatam), false) as SkeletonDatam;
            if (newSkeleton != skeleton)
            {
                skeleton = newSkeleton;
            }
            if (vox == null)
            {
                return;
            }
            //GUILayout.Space(10);
            //GUILayout.Label("   Located at: " + outputPath);
            // chose a vox
            /*GUILayout.Space(15);
            GUILayout.Label("Chose a Vox Model:");
            VoxDatam newVox = EditorGUILayout.ObjectField(vox, typeof(VoxDatam)) as VoxDatam;
            if (newVox != vox)
            {
                vox = newVox;
                outputPath = Path.GetDirectoryName(Path.GetFullPath(AssetDatabase.GetAssetPath(vox))) + "\\";
            }*/
            // Grab is mesh!
            //GUILayout.Space(15);
            //GUILayout.Label("Chose a Skeleton:");
            //skeleton = EditorGUILayout.ObjectField(skeleton, typeof(SkeletonDatam)) as SkeletonDatam;
            // chose animations
            if (animations.Count > 0)
            {
                if (GUILayout.Button("Bake Animator"))
                {
                    BakeVox();
                }
            }
            GUILayout.Label("Add Animation:");
            newAnim = EditorGUILayout.ObjectField(newAnim, typeof(AnimationClip), true) as AnimationClip;
            // if (GUILayout.Button("Clear Animations"))
            {
            //    animations.Clear();
            }
            for (int i = 0; i < animations.Count; i++)
            {
                if (GUILayout.Button(animations[i].name + " [-]"))
                {
                    animations.RemoveAt(i);
                    break;
                }
            }
            if (newAnim != null)
            {
                animations.Add(newAnim);
                newAnim = null;
            }
            //isPlaceUpwards = GUILayout.Toggle(isPlaceUpwards, "Place Half Height Upwards?");
            /* if (GUILayout.Button("Create Instance"))
             {
                 CreateSkinny();
             }
             GUILayout.Space(5);
             if (GUILayout.Button("Bake Weighted Mesh"))
             {
                 BakeWeightedMesh();
             }*/
            GUILayout.Space(5);

            if (newSkinny)
            {
                GUILayout.Space(5);
                if (GUILayout.Button("Kill Dude Instance"))
                {
                    DestroyImmediate(newSkinny);
                }
            }
        }

        private void BakeWeightedMesh()
        {
            // bake weighted mesh mesh!
            //Mesh oldMesh = vox.bakedMesh;
            CreateSkinny();
            SkinnedMeshRenderer skinnyMesh = newSkinny.GetComponent<SkinnedMeshRenderer>();
            skeleton.BakeMeshWeights(skinnyMesh);
            VoxBaker.SaveMesh(skinnyMesh.sharedMesh, vox);
            /*
            string meshName = "Baked_" + CleanName(newSkinny) + "_WeightedMesh";
            string weightedPath = outputPath + meshName + ".asset"; //  "/" +
            if (File.Exists(weightedPath))
            {
                File.Delete(weightedPath);
            }
            weightedPath = weightedPath.Substring(GetDataPathLength());
            Mesh mesh = skinnyMesh.sharedMesh;
            mesh.name = meshName;
            //AssetDatabase.CreateAsset(mesh, weightedPath);
            DestroyImmediate(newSkinny);*/
        }

        //private bool isPlaceUpwards;
        private void CreateSkinny()
        {
            // spawn object
            newSkinny = new GameObject();
            skeleton.InstantiateBones(newSkinny);
            newSkinny.name = vox.name;
            //newSkinny.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            SkinnedMeshRenderer mesher = newSkinny.AddComponent<SkinnedMeshRenderer>();
            mesher.bones = skeleton.GetBoneTransforms();
            //mesher.sharedMesh = vox.bakedMesh;
            //mesher.sharedMaterial = vox.bakedMaterial;
            mesher.rootBone = newSkinny.transform;
            /*if (isPlaceUpwards)
            {
                newSkinny.transform.position = new Vector3(0, mesher.bounds.extents.y, 0);
            }*/
            Animation animationComponent = newSkinny.AddComponent<Animation>();
            for (int i = 0; i < animations.Count; i++)
            {
                animationComponent.AddClip(animations[i], animations[i].name);
            }
        }
        public void BakeVox(bool isDestroy = true)
        {
            CreateSkinny();
            Bake(newSkinny, animations.ToArray());
            //monster.bakedMaterial = newSkinny.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
            //monster.animator = lastCreatedAnimator;
            if (isDestroy)
                DestroyImmediate(newSkinny);
        }

        private void DebugLog(string newLog)
        {
            debugLog.Add(newLog);
            if (debugLog.Count >= 20)
            {
                debugLog.RemoveAt(0);
            }
        }

        public string CleanName(GameObject prefab)
        {
            return StringUtils.CreateFileName(prefab.name);
        }

        private void Bake(GameObject animatingCharacter, AnimationClip[] clips)
        {
            if (!infoTexGen)
            {
                infoTexGen = (ComputeShader)Resources.Load("MeshInfoTextureGen", typeof(ComputeShader));
            }
            if (!playShader)
            {
                playShader = (Shader)Resources.Load("BakedShader", typeof(Shader));
            }
            //GameObject instance = prefab;
            SkinnedMeshRenderer skinRenderer = animatingCharacter.GetComponent<SkinnedMeshRenderer>();
            Transform instanceTransform = animatingCharacter.transform;

            DeletePreviouFiles(skinRenderer);

            AssetDatabase.SaveAssets();

           // var newAnimation = FindAnimation(instanceTransform);

            // Save mesh

            var scale = Vector3.one;
            
            // Get the clip info!
            var totalFrames = 0;
            //DebugLog("Baking " + clips.Length + " clips!");
            for (int i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                if (clip != null)
                {
                    var frame = Mathf.CeilToInt(clip.frameRate * clip.length) + 1;// Mathf.CeilToInt(clip.length / delta);
                    totalFrames += frame;
                    //DebugLog(clip.name + " has " + frame + " frames and frameRate: " + clip.frameRate);
                }
                else
                {
                    DebugLog("Clip " + i + " is null.");
                    DebugLog("Halting Processing.");
                    return; // and debug it
                }
            }

            // to store metadata
            //totalFrames += 1;

            var verticesCount = skinRenderer.sharedMesh.vertexCount;
            var texHeight = Mathf.NextPowerOfTwo(totalFrames);
            var texWidth = Mathf.NextPowerOfTwo(verticesCount);
            Debug.Log("Total Frames: " + totalFrames + " out of " + texHeight);
            Debug.Log("Vertices Count: " + verticesCount + " out of " + texWidth);
            Debug.Log("Texture Size: " + texWidth + " x " + texHeight);

            //var boneMeshes = new List<MeshFilter>();
            //FindMeshInBones(boneMeshes, instanceTransform);
            var infoList = new List<VertInfo>();
            for (int i = 0; i < clips.Length; i++)
            {
                infoList.AddRange( BakeClip(clips[i], animatingCharacter) );
            }

            BakeTextures(texWidth, texHeight, verticesCount, ref infoList);

            var mat = new Material(playShader);
            //mat.SetTexture("_MainTex", skinRenderer.sharedMaterial.GetTexture("_BaseMap"));
            //mat.SetColor("_Color", skinRenderer.sharedMaterial.color);
            mat.SetTexture("_PosTex", positionTexture);
            mat.SetTexture("_NmlTex", normalTexture);
            mat.enableInstancing = true;
            mat.name = CleanName(skinRenderer.gameObject) + "_Material";
            skinRenderer.sharedMaterial = mat;

            InitAnimator(mat, skinRenderer.sharedMesh, clips);
            //vox.animator = lastCreatedAnimator;
            SaveAnimator(lastCreatedAnimator, mat, positionTexture, normalTexture);
            //SaveUnityAssets(skinRenderer.sharedMesh, mat, skinRenderer, clips);
        }

        private static void SaveAnimator(AnimatorDatam datam, Material material, Texture2D positionTexture, Texture2D normalTexture)
        {
            string assetPath = AssetDatabase.GetAssetPath(datam);
            string startFolder = FileUtil.GetProjectRelativePath(assetPath);
            string path = EditorUtility.SaveFilePanel("Save Animation Asset", startFolder, datam.name, "asset");
            if (string.IsNullOrEmpty(path)) return;
            path = FileUtil.GetProjectRelativePath(path);
            AssetDatabase.CreateAsset(datam, path);
            AssetDatabase.CreateAsset(material, path.Replace(datam.name, datam.name.Replace("Animator", "Material")));
            AssetDatabase.CreateAsset(positionTexture, path.Replace(datam.name, datam.name.Replace("Animator", "Position Texture")));
            AssetDatabase.CreateAsset(normalTexture, path.Replace(datam.name, datam.name.Replace("Animator", "Normal Texture")));
            AssetDatabase.SaveAssets();
        }

        private AnimationClip[] GetClips()
        {
            if (prefab == null)
            {
                return new AnimationClip[0];
            }
            Animation prefabAnimation = prefab.GetComponent<Animation>();
            if (prefabAnimation == null)
            {
                return new AnimationClip[0];
            }
            AnimationClip[] clips = new AnimationClip[prefabAnimation.GetClipCount()];
            int i = 0;
            foreach (AnimationState state in prefabAnimation)
            {
                if (state.clip == null)
                {
                    Debug.LogError(i + " has null animation in " + prefabAnimation.name);
                    return new AnimationClip[0];
                }
                // do initialisation or something on clip
                clips[i] = state.clip;
                i++;
            }
            return clips;
        }

        private Animation FindAnimation(Transform parent)
        {
            Animation animator;
            if (
                (animator = parent.GetComponent<Animation>()) ||
                (animator = parent.GetComponentInChildren<Animation>())
            )
            {
                return animator;
            }
            return null;
        }

        private void FindMeshInBones(List<MeshFilter> filters, Transform bone)
        {
            foreach (Transform child in bone)
            {
                FindMeshInBones(filters, child);
            }
            var filter = bone.GetComponent<MeshFilter>();
            if (filter != null)
            {
                filters.Add(filter);
            }
        }

        private void BakeTextures(int texWidth, int texHeight, int verticesCount, ref List<VertInfo> infoList)
        {
            // bake  the vertexes and normals into the textures
            var positionsRenderTexture = new RenderTexture(texWidth, texHeight, 0, RenderTextureFormat.ARGBHalf);
            var normalRenderTexture = new RenderTexture(texWidth, texHeight, 0, RenderTextureFormat.ARGBHalf);
            positionTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGBAHalf, false, false);
            normalTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGBAHalf, false, false);
            positionTexture.wrapMode = TextureWrapMode.Clamp;
            normalTexture.wrapMode = TextureWrapMode.Clamp;
            positionTexture.filterMode = FilterMode.Point;
            normalTexture.filterMode = FilterMode.Point;

            foreach (var rt in new[] { positionsRenderTexture, normalRenderTexture })
            {
                rt.enableRandomWrite = true;
                rt.Create();
                RenderTexture.active = rt;
                GL.Clear(true, true, Color.clear);
            }

            var buffer = new ComputeBuffer(infoList.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(VertInfo)));
            buffer.SetData(infoList.ToArray());

            var kernel = infoTexGen.FindKernel("CSMain");
            uint x, y, z;
            infoTexGen.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
            infoTexGen.SetInt("VertCount", verticesCount);
            infoTexGen.SetBuffer(kernel, "Info", buffer);
            infoTexGen.SetTexture(kernel, "OutPosition", positionsRenderTexture);
            infoTexGen.SetTexture(kernel, "OutNormal", normalRenderTexture);
            infoTexGen.Dispatch(kernel, verticesCount / (int)x + 1, texHeight / (int)y + 1, 1);

            // convert the texture back to normal texture2d then to texture
            var posTex = RenderTextureToTexture2D.Convert(positionsRenderTexture);
            var normTex = RenderTextureToTexture2D.Convert(normalRenderTexture);
            Graphics.CopyTexture(posTex, positionTexture);
            Graphics.CopyTexture(normTex, normalTexture);
            // release all the buffer data
            positionsRenderTexture.Release();
            normalRenderTexture.Release();
            buffer.Release();
            positionTexture.Apply();
            normalTexture.Apply();
        }

        // List<MeshFilter> boneMeshes,
        private List<VertInfo> BakeClip(AnimationClip clip, GameObject instance)
        {
            SkinnedMeshRenderer skinRenderer = instance.GetComponent<SkinnedMeshRenderer>();
            Mesh newMesh = skinRenderer.sharedMesh;
            List<VertInfo> infoList = new List<VertInfo>();
            Vector3 boneOffset = Vector3.zero;
            float boneScale = 0f;
            Mesh animMesh = new Mesh();
            float currentTime = 0f;
            float frameDelta = 1 / clip.frameRate;// 60.0f;
            //int count = 0;
            // for each frame in clip - bake the verts
            while (currentTime < clip.length)
            {
                //Debug.LogError(clip.name + " - Sampling Clip at " + currentTime);
                clip.SampleAnimation(instance, Mathf.Clamp(currentTime, 0, clip.length));
                try
                {
                    skinRenderer.BakeMesh(animMesh);
                }
                catch (Exception e)
                {
                    Debug.LogError("Caught Exception in baking: " + e.ToString());
                    Debug.LogError("Skin mesh bones: " + instance.GetComponent<SkinnedMeshRenderer>().bones.Length);
                    //instance.GetComponent<SkinnedMeshRenderer>().wei
                }
                if (boneScale == 0)
                {
                    var bounds = new Bounds();
                    for (int j = 0; j < animMesh.vertexCount; j++)
                    {
                        var point = instance.transform.TransformPoint(animMesh.vertices[j]);
                        if (j == 0)
                        {
                            bounds.center = point;
                        }
                        bounds.Encapsulate(point);
                    }
                    /*foreach (var filter in boneMeshes)
                    {
                        var boneMesh = filter.sharedMesh;
                        for (int j = 0; j < boneMesh.vertexCount; j++)
                        {
                            var point = filter.transform.TransformPoint(boneMesh.vertices[j]);
                            bounds.Encapsulate(point);
                        }
                    }*/
                    //boneScale = newMesh.bounds.size.y / bounds.size.y;
                    //boneOffset.y = 0 - bounds.min.y;
                    //boneOffset.y = 0 - bounds.extents.y;
                    boneScale = 1;

                    //if (boneScale != 0)
                    //    Debug.LogError("Setting Bone Scale based on bounds to " + boneScale);
                }
                //DebugLog("Added " + animMesh.vertexCount + " verts for clip. frameDelta: " + frameDelta);
                for (int j = 0; j < animMesh.vertexCount; j++)
                {
                    var vert = (instance.transform.TransformPoint(animMesh.vertices[j]) + boneOffset) * boneScale;
                    infoList.Add(new VertInfo { position = vert, normal = animMesh.normals[j], extra = 1 });    // extra is random! alpha chanel for position, it isn't used at all!
                    //Debug.LogError(clip.name + " - vert " + j + ": " + vert.ToString());
                }
                //count++;
                currentTime += frameDelta;
            }
            return infoList;
        }

        private Vector3 PopulateMesh(Mesh newMesh, Mesh oldMesh, List<MeshFilter> boneMeshes, Transform offsetTransform)
        {
            var min = 0f;
            List<Vector3> vertices = new List<Vector3>(oldMesh.vertexCount);
            foreach (var vertex in oldMesh.vertices)
            {
                var point = offsetTransform.TransformPoint(vertex);
                vertices.Add(point);
                if (point.y < min)
                {
                    min = point.y;
                }
            }
            var vertexOffset = new Vector3(0, -min, 0);
            offsetTransform.position = vertexOffset;
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = vertices[i] + vertexOffset;
            }
            newMesh.subMeshCount = oldMesh.subMeshCount;
            newMesh.SetVertices(vertices);
            for (int i = 0; i < oldMesh.subMeshCount; i++)
            {
                newMesh.SetTriangles(oldMesh.GetTriangles(i).ToArray(), i);
            }
            var offset = vertices.Count;
            newMesh.uv = oldMesh.uv.ToArray();
            newMesh.normals = oldMesh.normals.ToArray();
            newMesh.tangents = oldMesh.tangents.ToArray();
            newMesh.colors = oldMesh.colors.ToArray();
            foreach (var filter in boneMeshes)
            {
                var boneMesh = filter.sharedMesh;
                var newVerts = newMesh.vertices.ToList();
                var newUv = newMesh.uv.ToList();
                var newNormals = newMesh.normals.ToList();
                var newTangents = newMesh.tangents.ToList();
                var newColors = newMesh.colors.ToList();
                var newTris = newMesh.triangles.ToList();

                for (int i = 0; i < boneMesh.vertexCount; i++)
                {
                    newVerts.Add(filter.transform.TransformPoint(boneMesh.vertices[i]));
                }
                newMesh.vertices = newVerts.ToArray();

                var boneTris = boneMesh.triangles.ToList();
                for (int i = 0; i < boneTris.Count; i++)
                {
                    boneTris[i] = boneTris[i] + offset;
                }
                newTris.AddRange(boneTris);
                newMesh.SetTriangles(newTris, 0);

                newUv.AddRange(boneMesh.uv);
                newNormals.AddRange(boneMesh.normals);
                newTangents.AddRange(boneMesh.tangents);
                newColors.AddRange(boneMesh.colors);

                newMesh.uv = newUv.ToArray();
                newMesh.normals = newNormals.ToArray();
                newMesh.tangents = newTangents.ToArray();
                if (oldMesh.colors.Length > 0)
                    newMesh.colors = newColors.ToArray();

                offset += boneMesh.vertexCount;
            }
            newMesh.RecalculateBounds();
            newMesh.MarkDynamic();
            return vertexOffset;
        }

        private SkinnedMeshRenderer FindSkinnedMeshRenderer(Transform parent)
        {
            SkinnedMeshRenderer mr;
            if (
                (mr = parent.GetComponent<SkinnedMeshRenderer>()) ||
                (mr = parent.GetComponentInChildren<SkinnedMeshRenderer>())
            )
            {
                return mr;
            }
            return null;
        }

        #region Saving


        string materialPath = "";
        string animatorPath = "";

        private void DeletePreviouFiles(SkinnedMeshRenderer skinRenderer)
        {
            try
            {
                string matName = "Baked_" + CleanName(skinRenderer.gameObject) + "_Material";
                materialPath = outputPath + "/" + matName + ".mat";
                if (File.Exists(meshPath))
                {
                    File.Delete(meshPath);
                }
                materialPath = materialPath.Substring(GetDataPathLength());

                string matName2 = "Baked_" + CleanName(skinRenderer.gameObject) + "_Animator";
                animatorPath = outputPath + "/" + matName2 + ".asset";
                if (File.Exists(animatorPath))
                {
                    File.Delete(animatorPath);
                }
                animatorPath = animatorPath.Substring(GetDataPathLength());

                // string filePath = StringUtils.Combine(outputPath, CleanName(skinRenderer.gameObject) + "_Data.asset");
                // filePath = "Assets" + filePath.Substring(Application.dataPath.Length);

                meshPath = outputPath + "/" + "Baked_" + CleanName(skinRenderer.gameObject) + "Mesh.asset";
                if (File.Exists(meshPath))
                {
                    File.Delete(meshPath);
                }
                meshPath = meshPath.Substring(GetDataPathLength());

                positionTexturePath = StringUtils.Combine(outputPath, "Baked_" + "Positions_Texture.asset");
                if (File.Exists(positionTexturePath))
                {
                    File.Delete(positionTexturePath);
                }
                positionTexturePath = positionTexturePath.Substring(GetDataPathLength());

                normalTexturePath = StringUtils.Combine(outputPath, "Baked_" + "Normals_Texture.asset");
                if (File.Exists(normalTexturePath))
                {
                    File.Delete(normalTexturePath);
                }
                normalTexturePath = normalTexturePath.Substring(GetDataPathLength());

                    /*prefabPath = StringUtils.Combine(outputPath, CleanName(skinRenderer.gameObject) + ".prefab");
                    if (File.Exists(prefabPath))
                    {
                        File.Delete(prefabPath);
                    }
                    prefabPath = prefabPath.Substring(GetDataPathLength());*/


            } 
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
            AssetDatabase.Refresh();
        }

        private AnimatorDatam lastCreatedAnimator;
        private void SaveUnityAssets(Mesh mesh, Material material, SkinnedMeshRenderer skinRenderer, AnimationClip[] clips)
        {
            //var materialPath = outputPath + "/" + mat.name + ".mat";
            //materialPath = materialPath.Substring(GetDataPathLength());
            AssetDatabase.CreateAsset(material, materialPath);

            // save images
            if (positionTexture == null)
            {
                DebugLog("Position Texture is null..");
                AssetDatabase.Refresh();
                return;
            }
            AssetDatabase.CreateAsset(positionTexture, positionTexturePath);
            AssetDatabase.CreateAsset(normalTexture, normalTexturePath);
            
            AssetDatabase.CreateAsset(lastCreatedAnimator, animatorPath);
            //DebugLog("Created animator asset at: " + filePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            /*EditorUtility.FocusProjectWindow();
            Selection.activeObject = lastCreatedAnimator;
            AssetDatabase.Refresh();*/
        }

        void InitAnimator(Material material, Mesh mesh, AnimationClip[] clips)
        {
            lastCreatedAnimator = ScriptableObject.CreateInstance<AnimatorDatam>();//new AnimatorDatam();
            lastCreatedAnimator.name = vox.name.Replace("Vox", "Animator");
            lastCreatedAnimator.data = new AnimatorData();
            lastCreatedAnimator.material = material;// materials[0];
            lastCreatedAnimator.mesh = mesh;
            lastCreatedAnimator.data.datas = new AnimationData[clips.Length];
            for (int i = 0; i < lastCreatedAnimator.data.datas.Length; i++)
            {
                if (clips[i] != null)
                {
                    lastCreatedAnimator.data.datas[i].name = clips[i].name;
                    lastCreatedAnimator.data.datas[i].time = clips[i].length;
                    lastCreatedAnimator.data.datas[i].framesPerSecond = (int)clips[i].frameRate;
                    lastCreatedAnimator.data.datas[i].frames = Mathf.CeilToInt(clips[i].frameRate * clips[i].length);
                }
            }
        }
        #endregion

        private void OldWay()
        {
            GUILayout.Space(15);
            GUILayout.Label("Chose a prefab: with a SkinnedMeshRenderer and Animation component.");
            prefab = EditorGUILayout.ObjectField(prefab, typeof(GameObject), true) as GameObject;
            if (prefab != null)
            {
                if (prefab.GetComponent<Animation>() && prefab.GetComponent<SkinnedMeshRenderer>())
                {
                    GUILayout.Label("Found " + prefab.GetComponent<Animation>().GetClipCount() + " animations.");
                }
                else
                {
                    prefab = null;
                }
            }
            GUILayout.Space(15);
            GUILayout.Label("Chose a folder where to output baked textures for it.");
            if (GUILayout.Button("Select Folder"))
            {
                var path = EditorUtility.OpenFolderPanel("Choose Output Path", outputPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    outputPath = path;
                }
            }
            GUILayout.Label("Output Path " + "[" + outputPath + "]");

            GUILayout.Space(15);
            if (prefab == null)
            {
                GUILayout.Label("You must chose a valid prefab.");
                GUI.enabled = false;
            }
            else if (outputPath == "")
            {
                GUILayout.Label("You must chose a valid output path.");
                GUI.enabled = false;
            }
            else
            {
                GUILayout.Label("Ready to bake.");
            }
            if (GUILayout.Button("Bake Animations"))
            {
                debugLog.Clear();
                Bake(prefab, GetClips());
            }
            if (debugLog.Count > 0)
            {
                GUILayout.Space(15);
                for (int i = 0; i < debugLog.Count; i++)
                {
                    GUILayout.Label(debugLog[i]);
                }
            }
            GUI.enabled = true;
        }
    }
}