﻿using GameWorld.Core.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace GameWorld.Core.Rendering.RenderItems
{

    public class BoundingBoxRenderItem : IRenderItem
    {
        Effect _shader;
        BoundingBox _bb;
        Color _colour;

        public BoundingBoxRenderItem(Effect shader, BoundingBox bb)
        {
            _shader = shader;
            _bb = bb;
            _colour = Color.Red;
        }

        public BoundingBoxRenderItem(Effect shader, BoundingBox bb, Color colour)
        {
            _shader = shader;
            _bb = bb;
            _colour = colour;
        }

        public Matrix ModelMatrix { get; set; } = Matrix.Identity;

        public void Draw(GraphicsDevice device, CommonShaderParameters parameters)
        {
            Render(device, parameters, ModelMatrix);
        }

        public void DrawGlowPass(GraphicsDevice device, CommonShaderParameters parameters)
        {
        
        }

        public void Render(GraphicsDevice device, CommonShaderParameters commonShaderParameters, Matrix ModelMatrix)
        {
            var originalVertices = new VertexPositionColor[24];
            var corners = _bb.GetCorners();

            originalVertices[0] = new VertexPositionColor(corners[0], _colour);
            originalVertices[1] = new VertexPositionColor(corners[1], _colour);

            originalVertices[2] = new VertexPositionColor(corners[1], _colour);
            originalVertices[3] = new VertexPositionColor(corners[2], _colour);

            originalVertices[4] = new VertexPositionColor(corners[2], _colour);
            originalVertices[5] = new VertexPositionColor(corners[3], _colour);

            originalVertices[6] = new VertexPositionColor(corners[3], _colour);
            originalVertices[7] = new VertexPositionColor(corners[0], _colour);

            var offset = 4;
            originalVertices[8] = new VertexPositionColor(corners[0 + offset], _colour);
            originalVertices[9] = new VertexPositionColor(corners[1 + offset], _colour);

            originalVertices[10] = new VertexPositionColor(corners[1 + offset], _colour);
            originalVertices[11] = new VertexPositionColor(corners[2 + offset], _colour);

            originalVertices[12] = new VertexPositionColor(corners[2 + offset], _colour);
            originalVertices[13] = new VertexPositionColor(corners[3 + offset], _colour);

            originalVertices[14] = new VertexPositionColor(corners[3 + offset], _colour);
            originalVertices[15] = new VertexPositionColor(corners[0 + offset], _colour);

            originalVertices[16] = new VertexPositionColor(corners[0], _colour);
            originalVertices[17] = new VertexPositionColor(corners[0 + offset], _colour);

            originalVertices[18] = new VertexPositionColor(corners[1], _colour);
            originalVertices[19] = new VertexPositionColor(corners[1 + offset], _colour);

            originalVertices[20] = new VertexPositionColor(corners[2], _colour);
            originalVertices[21] = new VertexPositionColor(corners[2 + offset], _colour);

            originalVertices[22] = new VertexPositionColor(corners[3], _colour);
            originalVertices[23] = new VertexPositionColor(corners[3 + offset], _colour);

            _shader.Parameters["View"].SetValue(commonShaderParameters.View);
            _shader.Parameters["Projection"].SetValue(commonShaderParameters.Projection);
            _shader.Parameters["World"].SetValue(ModelMatrix);

            foreach (var pass in _shader.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserPrimitives(PrimitiveType.LineList, originalVertices.ToArray(), 0, originalVertices.Count() / 2);
            }
        }
    }
}
