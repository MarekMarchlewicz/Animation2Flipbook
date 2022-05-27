namespace Animation2Flipbook.Editor
{
    internal static class TextContent
    {
        public const string errorApplicationIsPlaying = "Cannot record while editor is playing. Exit the playmode to use Animation2Flipbook recorder.";
        
        public static readonly string errorAnimationClipEmptyOrNull = $"There are no animations in '{guiLabelAnimationClips}'.";
        public static readonly string errorAnimationClipNotAssigned = $"Some of the '{guiLabelAnimationClips}' are not assigned.";
        public static readonly string errorAnimatedObjectNotAssigned = $"'{guiLabelObjectToAnimate}' is not assigned.";
        public static readonly string errorCustomSceneNotAssigned = $"When using custom scene you must select a scene to load. Assign a Scene to be used or deselect the '{guiLabelSceneUseCustom}' in '{guiLabelShowAdvancedOptions}'";

        public const string guiLabelObjectToAnimate = "Object to Animate";
        public const string guiLabelAnimationClips = "Animation Clips";
        public const string guiLabelOutputSettings = "Output Settings";
        public const string guiLabelShowAdvancedOptions = "Advanced Options";

        public const string guiLabelSavePath = "Save Path";

        public const string guiLabelFrameWidth = "Output Width";
        public const string guiLabelFrameHeight = "Output Height";
        public const string guiLabelEncompassingFrame = "Encompassing Frame";

        public const string guiLabelCameraFieldOfView = "Field of View";
        public const string guiLabelCameraOrthographic = "Orthographic Projection";

        public const string guiLabelFramesPerSecond = "Framerate";
        public const string guiLabelCombineIntoSpriteSheet = "Combine into Spritesheet";
        public const string guiLabelPadding = "Spritesheet Padding";

        public const string guiLabelObjectPosition = "Position";
        public const string guiLabelObjectRotation = "Rotation";

        public const string guiLabelSceneUseCustom = "Use Custom Scene";
        public const string guiLabelScene = "Scene";
        public const string guiLabelSceneUseSceneCamera = "Use Scene Cameras";

        public const string guiGenerateButtonText = "Generate";
    }
}