using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class BaseRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class featureSetting
    {
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingTransparents;
    }
    public featureSetting setting = new featureSetting();  // 实例化
    class CustomRenderPass : ScriptableRenderPass
    {
        /// <summary>
        /// 变量声明
        /// </summary>
        static string camerColorRT = "_cameraColorRT";
        static int camerColorRT_ID = Shader.PropertyToID(camerColorRT);


        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor getCameraData = renderingData.cameraData.cameraTargetDescriptor;    //
            cmd.GetTemporaryRT(camerColorRT_ID, getCameraData);

            ConfigureTarget(camerColorRT_ID);
            ConfigureClear(ClearFlag.Color,Color.black);    // 颜色clean成黑色
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("BaseRenderFeature");
            cmd.Blit(renderingData.cameraData.renderer.cameraColorTarget, camerColorRT_ID);
            context.ExecuteCommandBuffer(cmd);  // 执行。得先执行到 context里才行
            cmd.Release();
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(camerColorRT_ID);
        }
    }

    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();
        m_ScriptablePass.renderPassEvent = setting.passEvent;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


