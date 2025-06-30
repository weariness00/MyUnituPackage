// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.RendererUtils;
// using UnityEngine.Rendering.RenderGraphModule;
// using UnityEngine.Rendering.Universal;
//
// public class FMOD_OcclusionMaterialOnlyFeature : ScriptableRendererFeature
// {
//     class FMOD_OcclusionMaterialOnlyPass : ScriptableRenderPass
//     {
//         static readonly ShaderTagId shaderTagId = new ShaderTagId("FMODOcclusionOnly");
//
//         public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
//         {
//             var cameraData = frameData.Get<UniversalCameraData>();
//             var renderingData = frameData.Get<UniversalRenderingData>();
//
//             using (var builder = renderGraph.AddRenderPass("FMOD_Occlusion_Material_Only", out PassData passData))
//             {
//                 passData.rendererListDesc = new RendererListDesc(
//                     shaderTagId,
//                     renderingData.cullResults,
//                     cameraData.camera)
//                 {
//                     sortingCriteria = SortingCriteria.CommonOpaque,
//                     renderQueueRange = RenderQueueRange.all,
//                     layerMask = cameraData.camera.cullingMask,
//                 };
//                 
//                 builder.AllowPassCulling(false);
//                 builder.SetRenderFunc((PassData data, RenderGraphContext context) =>
//                 {
//                     context.cmd.Blit(data.);
//                 });
//             }
//         }
//
//
//         // 빈 PassData 구조체 (필요시 데이터 전달용으로 활용)
//         class PassData
//         {
//             public TextureHandle outputTexture;
//             public Material material;   
//         }
//
//     }
//
//     FMOD_OcclusionMaterialOnlyPass pass;
//     
//     public override void Create()
//     {
//         pass = new FMOD_OcclusionMaterialOnlyPass();
//         pass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques; // 위치는 필요에 따라 조정
//     }
//
//     public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//     {
//         renderer.EnqueuePass(pass);
//     }
// }
