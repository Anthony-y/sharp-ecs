﻿using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SharpECS;
using SharpECS.Samples.Components;

namespace SharpECS.Samples.Systems
{
    internal class GraphicsSystem 
        : EntitySystem
    {
        public GraphicsSystem(EntityPool pool)
            : base(pool, typeof(GraphicsComponent), typeof(TransformComponent))
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var entity in Compatible)
            {
                var texture = entity.GetComponent<GraphicsComponent>().Texture;
                var position = entity.GetComponent<TransformComponent>().Position;

                spriteBatch.Draw
                (
                    texture,
                    position,
                    Color.White
                );
            }
        }

    }

}
