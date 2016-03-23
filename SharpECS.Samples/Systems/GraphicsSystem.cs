﻿using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SharpECS;
using SharpECS.Samples.Components;

namespace SharpECS.Samples.Systems
{
    public class GraphicsSystem : EntitySystem<GraphicsComponent>
    {
        public GraphicsSystem(EntityPool pool)
            : base(pool)
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
