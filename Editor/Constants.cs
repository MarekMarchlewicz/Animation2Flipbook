namespace Animation2Flipbook.Editor
{
    public static class Constants
    {
        public const int windowWidth = 400;
        public const int windowHeight = 600;
        public const string toolName = "Animation 2 Flipbook";
        public const string toolMenuName = "Tools/" + toolName;
        public const string toolDebugResetEditorPrefs = toolMenuName + "/Reset Editor Prefs";
        public const string windowTitle = toolName;

        public const string prefix = "Animation2Flipbook";
        public const string sceneUseCustomKey = prefix + ".SceneUseCustom";
        public const string sceneUseSceneCameraKey = prefix + ".SceneUseSceneCamera";
        public const string customSceneKey = prefix + ".CustomScene";
        
        public const string savePathKey = prefix + ".SavePath";
        public const string frameWidthKey = prefix + ".FrameWidth";
        public const string frameHeightKey = prefix + ".FrameHeight";
        public const string frameSizeKey = prefix + ".FrameSize";
        
        public const string cameraFieldOfViewKey = prefix + ".CameraFieldOfView";
        public const string cameraOrthographicKey = prefix + ".CameraOrthographic";
 
        public const string framerateKey = prefix + ".FPS";
        public const string combineToSpriteSheetKey = prefix + ".CombineToSpriteSheet";
        public const string paddingKey = prefix + ".Padding";
 
        public const string defaultOutputFolderName = "OutputSprites";
        public const int defaultFrameWidth = 128;
        public const int defaultFrameHeight = 128;
        public const float defaultFrameSize = 1.3f;
 
        public const float minFieldOfView = 0.1f;
        public const float defaultFramerate = 24.0f;
 
        public const float defaultCameraFieldOfView = 60f;
        public const bool defaultCameraOrthographic = true;
 
        public const bool defaultCombineToSpriteSheet = false;
        public const int defaultPadding = 0;
        public const bool defaultUseCustomScene = false;
        public const bool defaultSceneUseSceneCamera = false;
    }
}
