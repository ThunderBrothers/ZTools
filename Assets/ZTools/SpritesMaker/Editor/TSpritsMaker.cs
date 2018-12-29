using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.UI;
using System;

namespace ZTools.SpritsMaker
{
    public class SpritsMaker : EditorWindow
    {
        //默认位置
        //美术给的原始图片路径
        private static string ImagePath = Application.dataPath + "/ZTools/SpritesMaker/Texture";
        //生成出的Animation的路径
        private static string AnimationPath = Application.dataPath + "/ZTools/SpritesMaker/Animations";
        //生成出的AnimationController的路径
        //private static string AnimationControllerPath = Application.dataPath + "/ZTools/SpritesMaker/AnimalContrller";
        enum FileStructure : int
        {
            单级目录 = 0,
            二级目录 = 1,
            三级目录 = 2
        }
        enum AnimatedType 
        {
            SpriteRenderer,
            Image
        }
        //当前位置
        private static string _ImagePath = null;
        private static string _AnimationPath = null;
        //private static string _AnimationControllerPath = null;
        private static FileStructure _fileStructure = FileStructure.二级目录;//文件目录结构
        private static AnimatedType _animatedType = AnimatedType.SpriteRenderer;//动画类型
        private static bool hasSelected = false;//已经选择处理文件
        private static int frames = 30;//创建的动画帧数
        private static bool localLocation = true;//默认打包动画文件在源图片文件夹下
        //进度条
        private static string temp;//当前创建的动画
        public static int number = 0;//完成个数


        [MenuItem("ZTools/SpritsMaker")]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(SpritsMaker));
        }
        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("帮助"))
            {
                Help();
            }
            if (GUILayout.Button("重置工具"))
            {
                Reset();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _animatedType = (AnimatedType)EditorGUILayout.EnumPopup("动画的类型", _animatedType);

            GUILayout.Label("动画帧数(10 ～ 60帧)");
            frames = EditorGUILayout.IntSlider(frames, 10, 60);
            GUILayout.EndHorizontal();
            _fileStructure = (FileStructure)EditorGUILayout.EnumPopup("文件目录结构", _fileStructure);
            GUILayout.BeginHorizontal();


            if (GUILayout.Button("选择要转换的图片文件根目录"))
            {
                _ImagePath = EditorUtility.OpenFolderPanel("选择要转换的图片文件根目录", Application.dataPath, "null");
                hasSelected = true;
            }
            //显示其他选项
            if (hasSelected)
            {
                localLocation = GUILayout.Toggle(localLocation, "动画文件本地放置");
                if (!localLocation)
                {
                    if (GUILayout.Button("选择动画保存的目录"))
                    {
                        _AnimationPath = EditorUtility.OpenFolderPanel("选择要转换的图片文件根目录", Application.dataPath, "null");
                    }
                }
                if (GUILayout.Button("创建动画"))
                {
                    BuildAniamtion();
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label("要处理的文件目录");
                EditorGUILayout.SelectableLabel(_ImagePath);
                if (!localLocation)
                {
                    GUILayout.Label("保持动画文件目录");
                    EditorGUILayout.SelectableLabel(_AnimationPath);
                }
            }
            GUILayout.EndVertical();
        }
        //重置数据
        void Reset()
        {
            //_AnimationControllerPath = AnimationControllerPath;
            _AnimationPath = AnimationPath;
            _ImagePath = ImagePath;
            localLocation = true;
            hasSelected = false;
            frames = 30;
            _fileStructure = FileStructure.二级目录;
        }
        //每次创建后重置
        static void ResetEveryBuild()
        {
            temp = null;
            number = 0;
        }
        //帮助
        void Help()
        {
            EditorUtility.DisplayDialog("帮助", "  1.设置序列帧动画帧数 限制10到60可以在代码里修改   2.文件夹目录结构支持三种 一级目录选择文件夹只有一个动作的图片 二级目录选择文件夹内还有多个文件夹存放图片 三级目录是所有二级目录的根目录  3.选择目录结构后选择资源路径即可生成动画文件", "OK");
        }
        //创建
        static void BuildAniamtion()
        {
            DirectoryInfo raw = new DirectoryInfo(_ImagePath);
            List<AnimationClip> clips = new List<AnimationClip>();
            switch (_fileStructure)
            {
                case FileStructure.单级目录:
                    clips.Add(BuildAnimationClip(raw));
                    break;
                case FileStructure.二级目录:
                    foreach (DirectoryInfo dictorys in raw.GetDirectories())
                    {
                        clips.Add(BuildAnimationClip(dictorys));
                    }
                    break;
                case FileStructure.三级目录:
                    foreach (DirectoryInfo dictorys in raw.GetDirectories())
                    {
                        foreach (DirectoryInfo dictoryAnimations in dictorys.GetDirectories())
                        {
                            //每个文件夹就是一组帧动画，这里把每个文件夹下的所有图片生成出一个动画文件
                            clips.Add(BuildAnimationClip(dictoryAnimations));
                        }
                        //把所有的动画文件生成在一个AnimationController里
                        // UnityEditor.Animations.AnimatorController controller = BuildAnimationController(clips, dictorys.Name);
                        //最后生成程序用的Prefab文件
                        // BuildPrefab(dictorys, controller);
                    }
                    break;
            }
            EditorUtility.DisplayDialog("完成" + number.ToString() + "个动画", temp, "OK");
            ResetEveryBuild();
        }
        //创建文件
        static AnimationClip BuildAnimationClip(DirectoryInfo dictorys)
        {
            string animationName = dictorys.Name;
            //查找所有图片，因为我找的测试动画是.jpg
            FileInfo[] images = dictorys.GetFiles("*.png");
            if (images.Length == 0)
            {
                EditorUtility.DisplayDialog("警告", "子文件夹里有无效文件夹 自动跳过 自己检查有没有少的", "我要重新检查生成个数");
                return null;
            }
            AnimationClip clip = new AnimationClip();
#if UNITY_5
#else
//AnimationUtility.SetAnimationType (clip, ModelImporterAnimationType.Generic);
#endif
            //编辑曲线的结合
            EditorCurveBinding curveBinding = new EditorCurveBinding();
            Type t = MyGetType();
            curveBinding.type = t;
            curveBinding.path = "";
            curveBinding.propertyName = "m_Sprite";
            //对象引用键帧
            ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[images.Length];
            //动画长度是按秒为单位，1/10就表示1秒切10张图片，根据项目的情况可以自己调节
            float frameTime = 1f / frames;
            for (int i = 0; i < images.Length; i++)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath(DataPathToAssetPath(images[i].FullName), typeof(Sprite)) as Sprite;
                keyFrames[i] = new ObjectReferenceKeyframe();
                keyFrames[i].time = frameTime * i;
                keyFrames[i].value = sprite;
            }
            //动画帧率，30比较合适
            clip.frameRate = frames;
            //有些动画我希望天生它就动画循环
            if (animationName.IndexOf("idle") >= 0)
            {
                //设置idle文件为循环动画
                SerializedObject serializedClip = new SerializedObject(clip);
                AnimationClipSettings clipSettings = new AnimationClipSettings(serializedClip.FindProperty("m_AnimationClipSettings"));
                clipSettings.loopTime = true;
                serializedClip.ApplyModifiedProperties();
            }
            //每组动画文件夹名字 也是每个动画文件的名字
            string parentName = System.IO.Directory.GetParent(dictorys.FullName).Name;
            AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);
            //动画文件和源文件放一起
            if (localLocation)
            {
                //设置绝对路径
                string path = _ImagePath.Replace(Application.dataPath, "");
                path = path.Insert(0, "Assets");
                //生成动画文件
                AssetDatabase.CreateAsset(clip, path + "/" + animationName + ".anim");
            }
            //创建动画文件在选择的文件夹
            else
            {
                //创建文件夹 DirectoryInfo info = System.IO.Directory.CreateDirectory(_AnimationPath + "/" + parentName);
                System.IO.Directory.CreateDirectory(_AnimationPath + "/" + parentName);
                //设置绝对路径
                string path = _AnimationPath.Replace(Application.dataPath, "");
                path = path.Insert(0, "Assets");
                AssetDatabase.CreateAsset(clip, path + "/" + parentName + "/" + animationName + ".anim");
            }
            AssetDatabase.SaveAssets();
            temp += animationName + ".anim" + "    ";
            number++;
            return clip;
        }
        private static Type MyGetType()
        {
            Type temp = null;
            switch(_animatedType){
                case AnimatedType.Image:temp = typeof(Image);break;
                case AnimatedType.SpriteRenderer: temp = typeof(SpriteRenderer); break;
            }
            return temp;
        }
        //static AnimatorController BuildAnimationController(List<AnimationClip> clips, string name)
        //{
        //    AnimatorController animatorController = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(AnimationControllerPath + "/" + name + ".controller");
        //    AnimatorControllerLayer layer = animatorController.layers[0];
        //    AnimatorStateMachine sm = layer.stateMachine;
        //    foreach (AnimationClip newClip in clips)
        //    {
        //        //AnimatorStateMachine machine = sm.AddStateMachine(newClip.name);
        //        AnimatorState state = sm.AddState(newClip.name);
        //        state.motion = newClip;
        //        state.AddTransition(state);
        //        AnimatorStateTransition ast = state.AddTransition(state);
        //        //AnimatorStateTransition trans = sm.AddAnyStateTransition(state);
        //        if (newClip.name == "idle")
        //        {
        //            sm.defaultState = state;
        //        }
        //        //sm.AddEntryTransition(machine);
        //        //sm.AddStateMachineExitTransition(machine);
        //        //trans.RemoveCondition(0);
        //    }
        //    AssetDatabase.SaveAssets();
        //    return animatorController;
        //}
        //static void BuildPrefab(DirectoryInfo dictorys, UnityEditor.Animations.AnimatorController animatorCountorller)
        //{
        //    //生成Prefab 添加一张预览用的Sprite
        //    FileInfo images = dictorys.GetDirectories()[0].GetFiles("*.jpg")[0];
        //    GameObject go = new GameObject();
        //    go.name = dictorys.Name;
        //    SpriteRenderer spriteRender = go.AddComponent<SpriteRenderer>();
        //    spriteRender.sprite = AssetDatabase.LoadAssetAtPath(DataPathToAssetPath(images.FullName), typeof(Sprite)) as Sprite;
        //    Animator animator = go.AddComponent<Animator>();
        //    animator.runtimeAnimatorController = animatorCountorller;
        //    //PrefabUtility.CreatePrefab(PrefabPath + "/" + go.name + ".prefab", go);
        //    DestroyImmediate(go);
        //}
        public static string DataPathToAssetPath(string path)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
                return path.Substring(path.IndexOf("Assets\\"));
            else
                return path.Substring(path.IndexOf("Assets /"));
        }
        class AnimationClipSettings
        {
            SerializedProperty m_Property;
            private SerializedProperty Get(string property)
            {
                return m_Property.FindPropertyRelative(property);
            }
            public AnimationClipSettings(SerializedProperty prop)
            {
                m_Property = prop;
            }
            public float startTime { get { return Get("m_StartTime").floatValue; } set { Get("m_StartTime").floatValue = value; } }
            public float stopTime { get { return Get("m_StopTime").floatValue; } set { Get("m_StopTime").floatValue = value; } }
            public float orientationOffsetY { get { return Get("m_OrientationOffsetY").floatValue; } set { Get("m_OrientationOffsetY").floatValue = value; } }
            public float level { get { return Get("m_Level").floatValue; } set { Get("m_Level").floatValue = value; } }
            public float cycleOffset { get { return Get("m_CycleOffset").floatValue; } set { Get("m_CycleOffset").floatValue = value; } }
            public bool loopTime { get { return Get("m_LoopTime").boolValue; } set { Get("m_LoopTime").boolValue = value; } }
            public bool loopBlend { get { return Get("m_LoopBlend").boolValue; } set { Get("m_LoopBlend").boolValue = value; } }
            public bool loopBlendOrientation { get { return Get("m_LoopBlendOrientation").boolValue; } set { Get("m_LoopBlendOrientation").boolValue = value; } }
            public bool loopBlendPositionY { get { return Get("m_LoopBlendPositionY").boolValue; } set { Get("m_LoopBlendPositionY").boolValue = value; } }
            public bool loopBlendPositionXZ { get { return Get("m_LoopBlendPositionXZ").boolValue; } set { Get("m_LoopBlendPositionXZ").boolValue = value; } }
            public bool keepOriginalOrientation { get { return Get("m_KeepOriginalOrientation").boolValue; } set { Get("m_KeepOriginalOrientation").boolValue = value; } }
            public bool keepOriginalPositionY { get { return Get("m_KeepOriginalPositionY").boolValue; } set { Get("m_KeepOriginalPositionY").boolValue = value; } }
            public bool keepOriginalPositionXZ { get { return Get("m_KeepOriginalPositionXZ").boolValue; } set { Get("m_KeepOriginalPositionXZ").boolValue = value; } }
            public bool heightFromFeet { get { return Get("m_HeightFromFeet").boolValue; } set { Get("m_HeightFromFeet").boolValue = value; } }
            public bool mirror { get { return Get("m_Mirror").boolValue; } set { Get("m_Mirror").boolValue = value; } }
        }
        #region
        //using UnityEngine;
        //using System.Collections;
        //using System.IO;
        //using System.Collections.Generic;
        //using UnityEditor;
        //using UnityEditorInternal;

        //public class BuildAnimation : Editor 
        //{
        //    //生成出的Prefab的路径
        //	private static string PrefabPath = "Assets/Resources/Prefabs";
        //	//生成出的AnimationController的路径
        //	private static string AnimationControllerPath = "Assets/AnimationController";
        //	//生成出的Animation的路径
        //	private static string AnimationPath = "Assets/Animation";
        //    //美术给的原始图片路径
        //	private static string ImagePath = Application.dataPath +"/Raw";
        //	[MenuItem("Build/BuildAnimaiton")]
        //	static void BuildAniamtion() 
        //	{
        //		DirectoryInfo raw = new DirectoryInfo (ImagePath);		
        //		foreach (DirectoryInfo dictorys in raw.GetDirectories()) 
        //		{
        //			List<AnimationClip> clips = new List<AnimationClip>();
        //			foreach (DirectoryInfo dictoryAnimations in dictorys.GetDirectories()) 
        //			{
        //				//每个文件夹就是一组帧动画，这里把每个文件夹下的所有图片生成出一个动画文件
        //				clips.Add(BuildAnimationClip(dictoryAnimations));
        //			}
        //			//把所有的动画文件生成在一个AnimationController里
        //			UnityEditor.Animations.AnimatorController controller =	BuildAnimationController(clips,dictorys.Name);
        //			//最后生成程序用的Prefab文件
        //			BuildPrefab(dictorys,controller);
        //		}	
        //	}
        //	static AnimationClip BuildAnimationClip(DirectoryInfo dictorys)
        //	{
        //		string animationName = dictorys.Name;
        //		//查找所有图片，因为我找的测试动画是.jpg 
        //		FileInfo []images  = dictorys.GetFiles("*.jpg");
        //		AnimationClip clip = new AnimationClip();
        //		AnimationUtility.SetAnimationType(clip,ModelImporterAnimationType.Generic);
        //		EditorCurveBinding curveBinding = new EditorCurveBinding();
        //		curveBinding.type = typeof(SpriteRenderer);
        //		curveBinding.path="";
        //		curveBinding.propertyName = "m_Sprite";
        //		ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[images.Length];
        //		//动画长度是按秒为单位，1/10就表示1秒切10张图片，根据项目的情况可以自己调节
        //		float frameTime = 1/10f;
        //		for(int i =0; i< images.Length; i++){
        //			Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DataPathToAssetPath(images[i].FullName));
        //			keyFrames[i] =   new ObjectReferenceKeyframe ();
        //			keyFrames[i].time = frameTime *i;
        //			keyFrames[i].value = sprite;
        //		}
        //		//动画帧率，30比较合适
        //		clip.frameRate = 30;
        //        //有些动画我希望天生它就动画循环
        //		if(animationName.IndexOf("idle") >=0 )
        //		{
        //			//设置idle文件为循环动画
        //			SerializedObject serializedClip = new SerializedObject(clip);
        //			AnimationClipSettings clipSettings = new AnimationClipSettings(serializedClip.FindProperty("m_AnimationClipSettings"));
        //			clipSettings.loopTime = true;
        //			serializedClip.ApplyModifiedProperties();
        //		}
        //		string parentName = System.IO.Directory.GetParent(dictorys.FullName).Name;
        //		System.IO.Directory.CreateDirectory(AnimationPath +"/"+parentName);
        //		AnimationUtility.SetObjectReferenceCurve(clip,curveBinding,keyFrames);
        //		AssetDatabase.CreateAsset(clip,AnimationPath +"/"+parentName +"/" +animationName+".anim");
        //		AssetDatabase.SaveAssets();
        //		return clip;
        //	}
        //	static UnityEditor.Animations.AnimatorController BuildAnimationController(List<AnimationClip> clips ,string name)
        //	{
        //		UnityEditor.Animations.AnimatorController animatorController = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(AnimationControllerPath +"/"+name+".controller");
        //		UnityEditor.Animations.AnimatorControllerLayer layer = animatorController.GetLayer(0);
        //		UnityEditor.Animations.AnimatorStateMachine sm = layer.stateMachine;
        //		foreach(AnimationClip newClip in clips)
        //		{
        //			UnityEditor.Animations.AnimatorState  state = sm.AddState(newClip.name);
        //			state.SetAnimationClip(newClip,layer);
        //			UnityEditor.Animations.AnimatorTransition trans = sm.AddAnyStateTransition(state);
        //			trans.RemoveCondition(0);
        //		}
        //		AssetDatabase.SaveAssets();
        //		return animatorController;
        //	}
        //	static void BuildPrefab(DirectoryInfo dictorys,UnityEditor.Animations.AnimatorController animatorCountorller)
        //	{
        //		//生成Prefab 添加一张预览用的Sprite
        //		FileInfo images  = dictorys.GetDirectories()[0].GetFiles("*.jpg")[0];
        //		GameObject go = new GameObject();
        //		go.name = dictorys.Name;
        //		SpriteRenderer spriteRender =go.AddComponent<SpriteRenderer>();
        //		spriteRender.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DataPathToAssetPath(images.FullName));
        //		Animator animator = go.AddComponent<Animator>();
        //		animator.runtimeAnimatorController = animatorCountorller;
        //		PrefabUtility.CreatePrefab(PrefabPath+"/"+go.name+".prefab",go);
        //		DestroyImmediate(go);
        //	}
        //	public static string DataPathToAssetPath(string path)
        //	{
        //		if (Application.platform == RuntimePlatform.WindowsEditor)
        //			return path.Substring(path.IndexOf("Assets\\"));
        //		else
        //			return path.Substring(path.IndexOf("Assets/"));
        //	}
        //	class AnimationClipSettings
        //	{
        //		SerializedProperty m_Property;
        //		private SerializedProperty Get (string property) { return m_Property.FindPropertyRelative(property); }
        //		public AnimationClipSettings(SerializedProperty prop) { m_Property = prop; }
        //		public float startTime   { get { return Get("m_StartTime").floatValue; } set { Get("m_StartTime").floatValue = value; } }
        //		public float stopTime	{ get { return Get("m_StopTime").floatValue; }  set { Get("m_StopTime").floatValue = value; } }
        //		public float orientationOffsetY { get { return Get("m_OrientationOffsetY").floatValue; } set { Get("m_OrientationOffsetY").floatValue = value; } }
        //		public float level { get { return Get("m_Level").floatValue; } set { Get("m_Level").floatValue = value; } }
        //		public float cycleOffset { get { return Get("m_CycleOffset").floatValue; } set { Get("m_CycleOffset").floatValue = value; } }
        //		public bool loopTime { get { return Get("m_LoopTime").boolValue; } set { Get("m_LoopTime").boolValue = value; } }
        //		public bool loopBlend { get { return Get("m_LoopBlend").boolValue; } set { Get("m_LoopBlend").boolValue = value; } }
        //		public bool loopBlendOrientation { get { return Get("m_LoopBlendOrientation").boolValue; } set { Get("m_LoopBlendOrientation").boolValue = value; } }
        //		public bool loopBlendPositionY { get { return Get("m_LoopBlendPositionY").boolValue; } set { Get("m_LoopBlendPositionY").boolValue = value; } }
        //		public bool loopBlendPositionXZ { get { return Get("m_LoopBlendPositionXZ").boolValue; } set { Get("m_LoopBlendPositionXZ").boolValue = value; } }
        //		public bool keepOriginalOrientation { get { return Get("m_KeepOriginalOrientation").boolValue; } set { Get("m_KeepOriginalOrientation").boolValue = value; } }
        //		public bool keepOriginalPositionY { get { return Get("m_KeepOriginalPositionY").boolValue; } set { Get("m_KeepOriginalPositionY").boolValue = value; } }
        //		public bool keepOriginalPositionXZ { get { return Get("m_KeepOriginalPositionXZ").boolValue; } set { Get("m_KeepOriginalPositionXZ").boolValue = value; } }
        //		public bool heightFromFeet { get { return Get("m_HeightFromFeet").boolValue; } set { Get("m_HeightFromFeet").boolValue = value; } }
        //		public bool mirror { get { return Get("m_Mirror").boolValue; } set { Get("m_Mirror").boolValue = value; } }
        //	}
        //}
        #endregion
    }
}
