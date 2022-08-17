using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

[ExecuteInEditMode]         // 让编辑器在不运行状态下运行
public class SpreadFeature : ScriptableRendererFeature
{
    [System.Serializable]   // renderfeature 的面板
    public class featureSetting
    {
        public bool SpreadSwitch = false;
        [Header("资源信息")]
        public Texture2D dissolveMap;
        public float dissolveMapSize = 0.5f;
        [Space(20)]
        [Header("渐变控制")]
        public float spreadWidth = 10;
        public float spreadSpeed = 20;
        [Range(0, 1)]
        public float offset = 0.7f;
        public Color spreadColor = new Color(80f / 255, 100f / 255, 100f / 255, 1f);
        public float objMaxDistance = 600;
        public float fullMaxDistance = 700;

        [HideInInspector]
        public float distance = 0;
        public string passRenderName = "Spread";
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingTransparents;
    }
    public featureSetting setting = new featureSetting();  // 实例化
    class SpreadRenderPass : ScriptableRenderPass
    {
        public bool SpreadSwitch = false;
        public Texture2D dissolveMap;
        public float dissolveMapSize;

        public float spreadWidth;
        public float spreadSpeed;

        public float offset;
        public Color spreadColor;
        public float objMaxDistance;
        public float fullMaxDistance;

        public float distance = 0;
        public Material passMat = null;
        public string passName;

        private RenderTargetIdentifier passSource { get; set; } // 源图像,这里一般是cameraColorTex

        public void setup(RenderTargetIdentifier sour) // 把相机的图像copy过来
        {
            this.passSource = sour;
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            if (passMat == null)   // 自动寻址真方便
                passMat = new Material(Shader.Find("PostProcess/Spread"));   
            if (SpreadSwitch)
            {
                distance += spreadSpeed * Time.deltaTime;
                if (distance > fullMaxDistance)
                    distance = 0;
                // Debug.Log("RenderFeaure Distance = " + distance);
                passMat.SetFloat("_Distance", distance);
                passMat.SetFloat("_FullMaxDistance", fullMaxDistance);
                passMat.SetFloat("_ObjMaxDistance", objMaxDistance);
                passMat.SetFloat("_SpreadWidth", spreadWidth);
                passMat.SetFloat("_Offset", offset * 0.03f);
                passMat.SetColor("_Color", spreadColor);
                passMat.SetTexture("_DissolveMap", dissolveMap);
                passMat.SetFloat("_DissolveMapSize", dissolveMapSize);

                int tempID = Shader.PropertyToID("temp");

                RenderTextureDescriptor getCameraData = renderingData.cameraData.cameraTargetDescriptor;   // 拿到相机数据，方便创建共享屬性的rt
                CommandBuffer cmd = CommandBufferPool.Get(passName); // 类似于cbuffer，把整个渲染命令圈起来
                cmd.GetTemporaryRT(tempID, getCameraData);    //申请一个临时图像，并设置相机rt的参数进去
                RenderTargetIdentifier temp = tempID;

                cmd.Blit(passSource, temp);
                cmd.SetGlobalTexture("_CamColorTex", temp);   // 加入这行会导致 scene 窗口变黑，所以先存在temp中
                cmd.Blit(null, passSource, passMat, -1);

                cmd.ReleaseTemporaryRT(tempID);
                context.ExecuteCommandBuffer(cmd);            //执行命令缓冲区的该命令
                CommandBufferPool.Release(cmd);               //释放该命令
            }
        }
    }
    
    SpreadRenderPass m_ScriptablePass;
    private DepthOnlyPass depthOnlyPass;            // 调用自带的 depthOnlyPass 
    private RenderTargetHandle depthTexHandle;
    public override void Create()
    {
        m_ScriptablePass = new SpreadRenderPass();
        m_ScriptablePass.renderPassEvent = setting.passEvent;   // 渲染位置
        m_ScriptablePass.passName = setting.passRenderName;     // 渲染名称

        m_ScriptablePass.SpreadSwitch = setting.SpreadSwitch;       // 开关
        m_ScriptablePass.dissolveMap = setting.dissolveMap;
        m_ScriptablePass.dissolveMapSize = setting.dissolveMapSize;
        m_ScriptablePass.spreadWidth = setting.spreadWidth;
        m_ScriptablePass.spreadSpeed = setting.spreadSpeed;
        m_ScriptablePass.offset = setting.offset;
        m_ScriptablePass.spreadColor = setting.spreadColor;
        m_ScriptablePass.objMaxDistance = setting.objMaxDistance;
        m_ScriptablePass.fullMaxDistance = setting.fullMaxDistance;
        m_ScriptablePass.distance = setting.distance;

        depthOnlyPass = new DepthOnlyPass(RenderPassEvent.BeforeRenderingPrePasses, RenderQueueRange.transparent, -1);
        depthTexHandle.Init("_TransparentsDepthBlend");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        depthOnlyPass.Setup(renderingData.cameraData.cameraTargetDescriptor, depthTexHandle);   // 先用depthOnlyPass渲染深度
        renderer.EnqueuePass(depthOnlyPass);
        m_ScriptablePass.setup(renderer.cameraColorTarget);     // 通过setup函数设置不同的渲染阶段的渲染结果进 passSource 里面
        renderer.EnqueuePass(m_ScriptablePass);                 // 执行
    }
}
