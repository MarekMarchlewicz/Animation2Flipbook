#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Animation2Flipbook.Editor
{
    [System.Serializable]
    internal class RecorderSettings
    {
        public GameObject animatedObject;
        public AnimationClip[] animationClips;
        public Vector3 animatedObjectPosition;
        public Vector3 animatedObjectRotation;
        
        public int width;
        public int height;
        public float startOffset;
        public float framerate;
        
        // Scene
        public bool sceneUseCustom;
        public string scene;
        public bool sceneUseSceneCamera;
        
        // Camera
        public bool cameraOrthographic;
        public float cameraOrthographicSize;
        public float cameraFieldOfView;
        public Vector3 cameraPosition;
        
        public string outputPath;
        public string originalSceneName;
    }
}
#endif