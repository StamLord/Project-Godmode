using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DistortionRenderer : PostProcessEffectRenderer<Distortion>
{
    private int _globalDistortionTexID;
    private Shader _distortionShader;

    public override DepthTextureMode GetCameraFlags()
    {
        return DepthTextureMode.Depth;
    }

    public override void Init()
    {
        _globalDistortionTexID = Shader.PropertyToID("_GlobalDistortionTex");
        _distortionShader = Shader.Find("Hidden/Custom/Distortion");
        base.Init();
    }

    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(_distortionShader);
        sheet.properties.SetFloat("_Magnitude", settings.Magnitude);

        if(!settings.DebugView)
        {
            context.command.GetTemporaryRT(_globalDistortionTexID,
                context.camera.pixelWidth >> settings.DownScaleFactor,
                context.camera.pixelHeight >> settings.DownScaleFactor,
                0, FilterMode.Bilinear, RenderTextureFormat.RGFloat);
            context.command.SetRenderTarget(_globalDistortionTexID);
            context.command.ClearRenderTarget(false, true, Color.clear);
        }

        //DistortionManager.Instance.PopulateCommandBuffer(context.command);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
