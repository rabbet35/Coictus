﻿using OpenTK.Graphics.OpenGL;
using RabbetGameEngine.Models;
using RabbetGameEngine.Rendering;
namespace RabbetGameEngine.SubRendering
{
    public class BatchLerpISpheresTransparent : Batch
    {
        public BatchLerpISpheresTransparent(int renderLayer = 0) : base(RenderType.lerpISpheresTransparent, renderLayer)
        {
        }

        protected override void buildBatch()
        {
            ShaderUtil.tryGetShader(ShaderUtil.lerpISpheresTransparentName, out batchShader);
            batchShader.use();
            batchShader.setUniformMat4F("projectionMatrix", Renderer.projMatrix);
            batchShader.setUniformVec2F("viewPortSize", Renderer.viewPortSize);
            batchedPoints = new PointParticle[RenderConstants.INIT_BATCH_ARRAY_SIZE];
            VertexBufferLayout l13 = new VertexBufferLayout();
            PointParticle.configureLayout(l13);
            PointParticle.configureLayout(l13);
            vao.addBufferDynamic(RenderConstants.INIT_BATCH_ARRAY_SIZE * PointParticle.SIZE_BYTES, l13);
            vao.drawType = PrimitiveType.Points;
        }

        public override bool tryToFitInBatchPoints(PointCloudModel mod)
        {
            int n = batchedPoints.Length;
            if (!BatchUtil.canFitOrResize(ref batchedPoints, mod.points.Length * 2, pointsItterator, maxPointCount)) return false;
            if (batchedPoints.Length != n)
            {
                vao.resizeBuffer(0, batchedPoints.Length * PointParticle.SIZE_BYTES);
            }
            for (n = 0; n < mod.points.Length; n++)
            {
                batchedPoints[pointsItterator] = mod.points[n];
                batchedPoints[pointsItterator + 1] = mod.prevPoints[n];
                pointsItterator += 2;
            }
            hasBeenUsed = true;
            return true;
        }

        public override bool tryToFitInBatchLerpPoint(PointParticle p, PointParticle prevP)
        {
            int n = batchedPoints.Length;
            if (!BatchUtil.canFitOrResize(ref batchedPoints, 2, pointsItterator, maxPointCount)) return false;
            if (batchedPoints.Length != n)
            {
                vao.resizeBuffer(0, batchedPoints.Length * PointParticle.SIZE_BYTES);
            }
            batchedPoints[pointsItterator] = p;
            batchedPoints[pointsItterator + 1] = prevP;
            pointsItterator += 2;
            hasBeenUsed = true;
            return true;
        }

        public override void updateBuffers()
        {
            vao.updateBuffer(0, batchedPoints, pointsItterator * PointParticle.SIZE_BYTES);
        }

        public override void updateUniforms(World thePlanet)
        {
            batchShader.use();
            batchShader.setUniformMat4F("projectionMatrix", Renderer.projMatrix);
            batchShader.setUniformVec2F("viewPortSize", Renderer.viewPortSize);
            batchShader.setUniformVec3F("fogColor", thePlanet.getFogColor());
            batchShader.setUniform1F("fogStart", thePlanet.getFogStart());
            batchShader.setUniform1F("fogEnd", thePlanet.getFogEnd());
        }

        public override void drawBatch(World thePlanet)
        {
            vao.bind();
            batchShader.use();
            batchShader.setUniform1F("percentageToNextTick", TicksAndFrames.getPercentageToNextTick());
            GL.DrawArrays(PrimitiveType.Points, 0, pointsItterator / 2);
            vao.unBind();
        }
    }
}
