﻿using GameWorld.Core.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameWorld.Core.Rendering.RenderItems
{
    public class LineRenderItem : IRenderItem
    {
        public LineMeshRender LineMesh { get; set; }

        public Matrix ModelMatrix { get; set; } = Matrix.Identity;

        public void Draw(GraphicsDevice device, CommonShaderParameters parameters)
        {
            LineMesh.Render(device, parameters, ModelMatrix);
        }

        public void DrawGlowPass(GraphicsDevice device, CommonShaderParameters parameters)
        {
    
        }
    }
}
