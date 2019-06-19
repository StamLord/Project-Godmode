using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(DistortionRenderer), PostProcessEvent.BeforeStack, "Custom/Distortion")]
public class Distortion : PostProcessEffectSettings
{
    [Range(0f, 1.0f), Tooltip("The magnitude in texels of distorion effect.")]
    public FloatParameter Magnitude = new FloatParameter { value = 1.0f };

    [Range(0,4), Tooltip("The down-scale factor to apply to the generated texture.")]
    public IntParameter DownScaleFactor = new IntParameter { value = 0 };

    [Tooltip("Displays effect in debug view")]
    public BoolParameter DebugView = new BoolParameter { value = false };
}
