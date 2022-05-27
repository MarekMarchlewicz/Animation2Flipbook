using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Animation2Flipbook.Editor
{
    public class Animation2FlipbookEditorPopup : EditorWindow
    {
        string m_SavePath;
        string m_FileName;

        bool m_SceneUseCustom;
        bool m_SceneUseSceneCamera;
        SceneAsset m_Scene;

        bool m_CameraOrthographic;
        float m_CameraFieldOfView;

        int m_FrameWidth;
        int m_FrameHeight;
        float m_FrameSize;
        float m_Framerate;
        bool m_CombineToSpriteSheet;
        int m_Padding;

        Vector3 m_ObjectPosition;
        Vector3 m_ObjectRotation;

        GameObject m_ObjectToRecord;
        [SerializeField]
        List<AnimationClip> m_AnimationClips;

        SerializedObject m_SerializedObject;
        Vector2 m_AnimationClipsScrollPos;
        Vector2 m_AdvancedScrollPos;
        bool m_FoldoutAnimatedObject = true;
        bool m_FoldoutOutputSettings = true;
        bool m_FoldoutAdvancedOptions;

        bool ObjectToRecordNotAssigned => m_ObjectToRecord == null;
        bool AnimationClipsNullOrEmpty => m_AnimationClips == null || m_AnimationClips.Count == 0;
        bool AnimationClipsNotAssigned => m_AnimationClips != null && m_AnimationClips.Any(clip => clip == null);
        bool CustomSceneNotCorrect => m_SceneUseCustom && m_Scene == null;

        bool CanRecord() => EditorApplication.isPlaying || !AnimationClipsNullOrEmpty && !ObjectToRecordNotAssigned && !CustomSceneNotCorrect;

        void OnEnable()
        {
            m_SerializedObject = new SerializedObject(this);

            EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;

            LoadEditorPrefs();
        }

        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
        }

        void OnPlaymodeStateChanged(PlayModeStateChange state)
        {
            if (!FlipbookRecorderController.IsAvailable())
                return;
            var controller = FlipbookRecorderController.GetController();
            if (controller == null)
                return;

            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    controller.Run();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    SceneHelper.ReopenOriginalScene(controller.settings.originalSceneName);
                    break;
            }
        }

        void OnGUI()
        {
            m_SerializedObject.Update();

            EditorGUILayout.BeginVertical();

            GuiAnimatedObject();
            GuiAnimationClips();
            GuiOutputSettings();
            GuiAdvancedOptions();

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GuiHelpBox();

            GUI.enabled = CanRecord();
            if (GUILayout.Button(TextContent.guiGenerateButtonText))
                Generate();
            GUI.enabled = true;
        }

        void GuiHelpBox()
        {
            if (ObjectToRecordNotAssigned)
                EditorGUILayout.HelpBox(TextContent.errorAnimatedObjectNotAssigned, MessageType.Warning);
            else if (AnimationClipsNullOrEmpty)
                EditorGUILayout.HelpBox(TextContent.errorAnimationClipEmptyOrNull, MessageType.Warning);
            else if (AnimationClipsNullOrEmpty)
                EditorGUILayout.HelpBox(TextContent.errorAnimationClipNotAssigned, MessageType.Warning);
            else if (CustomSceneNotCorrect)
                EditorGUILayout.HelpBox(TextContent.errorCustomSceneNotAssigned, MessageType.Warning);
        }

        void GuiAnimatedObject()
        {
            m_FoldoutAnimatedObject = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutAnimatedObject, TextContent.guiLabelObjectToAnimate);
            if (m_FoldoutAnimatedObject)
            {
                EditorGUI.BeginChangeCheck();
                m_ObjectToRecord = (GameObject)EditorGUILayout.ObjectField(TextContent.guiLabelObjectToAnimate, m_ObjectToRecord, typeof(GameObject), false);
                if (EditorGUI.EndChangeCheck() && m_ObjectToRecord != null)
                {
                    m_ObjectPosition = m_ObjectToRecord.transform.position;
                    m_ObjectRotation = m_ObjectToRecord.transform.rotation.eulerAngles;
                }

                EditorGUI.indentLevel++;
                if (m_ObjectToRecord != null)
                {
                    m_ObjectPosition = EditorGUILayout.Vector3Field(TextContent.guiLabelObjectPosition, m_ObjectPosition);
                    m_ObjectRotation = EditorGUILayout.Vector3Field(TextContent.guiLabelObjectRotation, m_ObjectRotation);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void GuiAnimationClips()
        {
            var animList = m_SerializedObject.FindProperty(nameof(m_AnimationClips));
            m_AnimationClipsScrollPos = EditorGUILayout.BeginScrollView(m_AnimationClipsScrollPos, GUILayout.MaxHeight(150));
            EditorGUILayout.PropertyField(animList, new GUIContent(TextContent.guiLabelAnimationClips), true);
            m_SerializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
        }

        void GuiOutputSettings()
        {
            m_FoldoutOutputSettings = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutOutputSettings, TextContent.guiLabelOutputSettings);
            if (m_FoldoutOutputSettings)
            {
                EditorGUI.BeginChangeCheck();
                m_SavePath = EditorGUILayout.TextField(TextContent.guiLabelSavePath, m_SavePath);
                if (EditorGUI.EndChangeCheck())
                    EditorPrefs.SetString(Constants.savePathKey, m_SavePath);

                EditorGUI.BeginChangeCheck();
                m_FrameWidth = EditorGUILayout.IntField(TextContent.guiLabelFrameWidth, m_FrameWidth);
                if (EditorGUI.EndChangeCheck())
                    EditorPrefs.SetInt(Constants.frameWidthKey, m_FrameWidth);

                EditorGUI.BeginChangeCheck();
                m_FrameHeight = EditorGUILayout.IntField(TextContent.guiLabelFrameHeight, m_FrameHeight);
                if (EditorGUI.EndChangeCheck())
                    EditorPrefs.SetInt(Constants.frameHeightKey, m_FrameHeight);

                EditorGUI.BeginChangeCheck();
                m_Framerate = EditorGUILayout.FloatField(TextContent.guiLabelFramesPerSecond, m_Framerate);
                if (m_Framerate < 0)
                    m_Framerate = 0;
                if (EditorGUI.EndChangeCheck())
                    EditorPrefs.SetFloat(Constants.framerateKey, m_Framerate);

                EditorGUI.BeginChangeCheck();
                m_FrameSize = EditorGUILayout.FloatField(TextContent.guiLabelEncompassingFrame, m_FrameSize);
                if (m_FrameSize < 0.0f)
                    m_FrameSize = 0.0f;
                if (EditorGUI.EndChangeCheck())
                    EditorPrefs.SetFloat(Constants.frameSizeKey, m_FrameSize);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void GuiAdvancedOptions()
        {
            m_FoldoutAdvancedOptions = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutAdvancedOptions, TextContent.guiLabelShowAdvancedOptions);
            if (m_FoldoutAdvancedOptions)
            {
                m_AdvancedScrollPos = EditorGUILayout.BeginScrollView(m_AdvancedScrollPos);

                if (!(m_SceneUseCustom && m_SceneUseSceneCamera))
                {
                    EditorGUI.BeginChangeCheck();
                    m_CameraOrthographic = EditorGUILayout.Toggle(TextContent.guiLabelCameraOrthographic, m_CameraOrthographic);
                    if (EditorGUI.EndChangeCheck())
                        EditorPrefs.SetBool(Constants.cameraOrthographicKey, m_CameraOrthographic);

                    if (!m_CameraOrthographic)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUI.BeginChangeCheck();
                        m_CameraFieldOfView = EditorGUILayout.FloatField(TextContent.guiLabelCameraFieldOfView, m_CameraFieldOfView);
                        if (m_CameraFieldOfView < Constants.minFieldOfView)
                            m_CameraFieldOfView = Constants.minFieldOfView;
                        if (EditorGUI.EndChangeCheck())
                            EditorPrefs.SetFloat(Constants.cameraFieldOfViewKey, m_CameraFieldOfView);
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUILayout.Separator();

                EditorGUI.BeginChangeCheck();
                m_SceneUseCustom = EditorGUILayout.Toggle(TextContent.guiLabelSceneUseCustom, m_SceneUseCustom);
                if (EditorGUI.EndChangeCheck())
                    EditorPrefs.SetBool(Constants.sceneUseCustomKey, m_SceneUseCustom);

                if (m_SceneUseCustom)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    m_Scene = (SceneAsset)EditorGUILayout.ObjectField(TextContent.guiLabelScene, m_Scene, typeof(SceneAsset), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        var scenePath = m_Scene != null ? AssetDatabase.GetAssetPath(m_Scene) : string.Empty;
                        EditorPrefs.SetString(Constants.customSceneKey, scenePath);
                    }

                    EditorGUI.BeginChangeCheck();
                    m_SceneUseSceneCamera = EditorGUILayout.Toggle(TextContent.guiLabelSceneUseSceneCamera, m_SceneUseSceneCamera);
                    if (EditorGUI.EndChangeCheck())
                        EditorPrefs.SetBool(Constants.sceneUseSceneCameraKey, m_SceneUseSceneCamera);
                    EditorGUI.indentLevel--;
                }

                // EditorGUILayout.Separator();
                //
                // EditorGUI.BeginChangeCheck();
                // m_CombineToSpriteSheet = EditorGUILayout.Toggle(TextContent.guiLabelCombineIntoSpriteSheet, m_CombineToSpriteSheet);
                // if (EditorGUI.EndChangeCheck())
                //     EditorPrefs.SetBool(Constants.combineToSpriteSheetKey, m_CombineToSpriteSheet);
                //
                // if (m_CombineToSpriteSheet)
                // {
                //     EditorGUI.BeginChangeCheck();
                //     m_Padding = EditorGUILayout.IntField(TextContent.guiLabelPadding, m_Padding);
                //     if (EditorGUI.EndChangeCheck())
                //         EditorPrefs.GetInt(Constants.paddingKey, m_Padding);
                // }

                GUILayout.EndScrollView();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void LoadEditorPrefs()
        {
            m_SavePath = EditorPrefs.GetString(Constants.savePathKey, Constants.defaultOutputFolderName);
            m_FrameWidth = EditorPrefs.GetInt(Constants.frameWidthKey, Constants.defaultFrameWidth);
            m_FrameHeight = EditorPrefs.GetInt(Constants.frameHeightKey, Constants.defaultFrameHeight);
            m_FrameSize = EditorPrefs.GetFloat(Constants.frameSizeKey, Constants.defaultFrameSize);

            m_CameraOrthographic = EditorPrefs.GetBool(Constants.cameraOrthographicKey, Constants.defaultCameraOrthographic);
            m_CameraFieldOfView = EditorPrefs.GetFloat(Constants.cameraFieldOfViewKey, Constants.defaultCameraFieldOfView);

            m_Framerate = EditorPrefs.GetFloat(Constants.framerateKey, Constants.defaultFramerate);
            m_CombineToSpriteSheet = EditorPrefs.GetBool(Constants.combineToSpriteSheetKey, Constants.defaultCombineToSpriteSheet);
            m_Padding = EditorPrefs.GetInt(Constants.paddingKey, Constants.defaultPadding);

            m_SceneUseCustom = EditorPrefs.GetBool(Constants.sceneUseCustomKey, Constants.defaultUseCustomScene);
            m_SceneUseSceneCamera = EditorPrefs.GetBool(Constants.sceneUseSceneCameraKey, Constants.defaultSceneUseSceneCamera);
            var customScenePath = EditorPrefs.GetString(Constants.customSceneKey, string.Empty);
            if (!string.IsNullOrEmpty(customScenePath))
                m_Scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(customScenePath);
        }

        void Generate()
        {
            if (!CanRecord())
            {
                Debug.Assert(!ObjectToRecordNotAssigned, TextContent.errorAnimatedObjectNotAssigned);
                Debug.Assert(!EditorApplication.isPlaying, TextContent.errorApplicationIsPlaying);
                Debug.Assert(!AnimationClipsNullOrEmpty, TextContent.errorAnimationClipEmptyOrNull);
                Debug.Assert(!AnimationClipsNotAssigned, TextContent.errorAnimationClipNotAssigned);
                Debug.Assert(!CustomSceneNotCorrect, TextContent.errorCustomSceneNotAssigned);
                return;
            }

            var bounds = GetObjectBounds(m_ObjectToRecord);
            var cameraPosition = bounds.center;
            if (m_CameraOrthographic)
                cameraPosition += Vector3.back;
            else
                cameraPosition += Vector3.back * GetCameraDistance(m_CameraFieldOfView, bounds.size.y * m_FrameSize);

            var scenePath = m_SceneUseCustom && m_Scene != null ? AssetDatabase.GetAssetPath(m_Scene) : string.Empty;
            var settings = new RecorderSettings
            {
                animationClips = m_AnimationClips.ToArray(),
                framerate = m_Framerate,

                cameraPosition = cameraPosition,
                cameraOrthographicSize = Mathf.Max(bounds.size.x, bounds.size.y) * m_FrameSize * 0.5f,
                cameraOrthographic = m_CameraOrthographic,
                cameraFieldOfView = m_CameraFieldOfView,

                startOffset = 0f,

                width = m_FrameWidth,
                height = m_FrameHeight,

                outputPath = GetOutputPath(m_SavePath),

                animatedObject = m_ObjectToRecord,
                animatedObjectPosition = m_ObjectPosition,
                animatedObjectRotation = m_ObjectRotation,

                originalSceneName = SceneManager.GetActiveScene().path,

                sceneUseCustom = m_SceneUseCustom,
                scene = scenePath,
                sceneUseSceneCamera = m_SceneUseSceneCamera
            };

            SceneHelper.LoadRecorderScene(settings);

            FlipbookRecorderController.SpawnController(settings);

            EditorApplication.EnterPlaymode();
        }

        public static string GetOutputPath(string folderName) => Path.Combine(Application.dataPath, "..", folderName);

        [MenuItem(Constants.toolMenuName, false)]
        public static void OpenPopup()
        {
            var window = GetWindow(typeof(Animation2FlipbookEditorPopup));
            window.titleContent = new GUIContent(Constants.windowTitle);
            window.minSize = new Vector2(Constants.windowWidth, Constants.windowHeight);
        }

#if DEV_TOOLS_A2F
        [MenuItem(Constants.toolDebugResetEditorPrefs, false)]
        public static void ClearPrefs()
        {
            EditorPrefs.DeleteAll();
        }
#endif

        static Bounds GetObjectBounds(GameObject gameObject)
        {
            var bounds = new Bounds();
            var renderers = gameObject.GetComponentsInChildren<Renderer>();

            foreach (var r in renderers)
                bounds.Encapsulate(r.bounds);
            bounds.center -= gameObject.transform.position;

            return bounds;
        }

        static float GetCameraDistance(float fieldOfView, float frustumHeight)
        {
            return frustumHeight * 0.5f / Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
        }
    }
}