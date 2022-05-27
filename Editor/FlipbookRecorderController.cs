#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine.Playables;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace Animation2Flipbook.Editor
{
    internal class FlipbookRecorderController : MonoBehaviour
    {
        [SerializeField]
        public RecorderSettings settings;

        public RecorderController recorderController;
        
        const string k_FlipbookRecorderControllerName = nameof(FlipbookRecorderController);

        [SerializeField]
        Camera m_Camera;

        const string k_CameraTag = "MainCamera";

        public static bool IsAvailable() => GetController() != null;
        
        public static FlipbookRecorderController GetController()
        {
            return GameObject.Find(k_FlipbookRecorderControllerName)?.GetComponent<FlipbookRecorderController>();
        }

        public static FlipbookRecorderController SpawnController(RecorderSettings recorderSettings)
        {
            Debug.Assert(!IsAvailable());

            var go = new GameObject(k_FlipbookRecorderControllerName);
            var controller = go.AddComponent<FlipbookRecorderController>();
            controller.Initialize(recorderSettings);
            return controller;
        }

        public void Initialize(RecorderSettings recorderSettings)
        {
            Debug.Assert(recorderSettings != null);
            settings = recorderSettings;
            
            if(!(settings.sceneUseCustom && settings.sceneUseSceneCamera))
                m_Camera = SpawnCamera(recorderSettings);
        }

        static Camera SpawnCamera(RecorderSettings settings)
        {
            var cameraGO = new GameObject("Camera");
            cameraGO.tag = k_CameraTag;
            cameraGO.transform.position = settings.cameraPosition;
            
            var camera = cameraGO.AddComponent<Camera>();
            camera.fieldOfView = settings.cameraFieldOfView;
            camera.orthographic = settings.cameraOrthographic;
            camera.orthographicSize = settings.cameraOrthographicSize;
            return camera;
        }

        public void Run()
        {
            Debug.Assert(Application.isPlaying);
            Debug.Assert(settings != null);
            Debug.Assert(m_Camera != null);
            
            StartCoroutine(RecordingRoutine());
        }

        IEnumerator RecordingRoutine()
        {
            yield return null;

            var animatedObject = InstantiateAnimatedObject(settings.animatedObject, settings.animatedObjectPosition, settings.animatedObjectRotation);

            foreach (var animationClip in settings.animationClips)
                yield return StartCoroutine(RecordClip(animatedObject, animationClip));

            EditorApplication.ExitPlaymode();
        }

        IEnumerator RecordClip(GameObject animatedObject, AnimationClip animationClip)
        {
            var recorder = GetRecorder(settings, animationClip);
            Debug.Assert(recorder != null);

            var firstFrame = settings.startOffset > 0.0f ? (int) (settings.startOffset * settings.framerate) : 0;
            var lastFrame = (int)(settings.framerate * animationClip.length);
            recorder.Settings.SetRecordModeToFrameInterval(firstFrame, lastFrame);
            recorder.Settings.FrameRate = settings.framerate;
            recorder.PrepareRecording();

            var animator = animatedObject.GetComponent<Animator>();
            var clip = AnimationPlayableUtilities.PlayClip(animator, animationClip, out var graph);
            
            recorder.StartRecording();
            
            while (recorder.IsRecording())
                yield return new WaitForEndOfFrame();

            graph.Destroy();
            clip.Destroy();
            
            recorder.StopRecording();
        }

        static GameObject InstantiateAnimatedObject(GameObject objectToAnimate, Vector3 position, Vector3 rotation)
        {
            var animatedObject = Instantiate(objectToAnimate);
            animatedObject.transform.position = position;
            animatedObject.transform.rotation = Quaternion.Euler(rotation);
            var animator = animatedObject.GetComponent<Animator>();
            if (animator == null)
                animator = animatedObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = null;
            return animatedObject;
        }

        static RecorderController GetRecorder(RecorderSettings settings, AnimationClip animationClip)
        {
            // Image
            var imageRecorder = GetImageRecorderSettings(settings, animationClip);
            var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            controllerSettings.hideFlags = HideFlags.HideAndDontSave;
            
            // Setup Recording
            controllerSettings.AddRecorderSettings(imageRecorder);
            return new RecorderController(controllerSettings);
        }
        
        static ImageRecorderSettings GetImageRecorderSettings(RecorderSettings settings, AnimationClip animationClip)
        {
            var imageRecorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();
            imageRecorder.hideFlags = HideFlags.HideAndDontSave;
            
            imageRecorder.name = "Flipbook Image Recorder";
            imageRecorder.Enabled = true;
            imageRecorder.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
            imageRecorder.CaptureAlpha = true;
            imageRecorder.OutputFile = Path.Combine(settings.outputPath, animationClip.name)+ /*"_" + DefaultWildcard.Take + */ "_" + DefaultWildcard.Frame;
            imageRecorder.FrameRate = settings.framerate;
            imageRecorder.FrameRatePlayback = FrameRatePlayback.Constant;
            imageRecorder.imageInputSettings = new CameraInputSettings()
            {
                RecordTransparency = true,
                OutputWidth = settings.width,
                OutputHeight = settings.height,
                CameraTag = k_CameraTag
            };

            return imageRecorder;
        }
    }
}
#endif