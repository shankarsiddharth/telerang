using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.VectorDraw;
using System;
using System.Collections.Generic;
using System.Text;

using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Containers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.TextureAtlases;

namespace telerang.Entities
{
    class BoomerangTrail
    {
        private ParticleEffect _particleEffect;
        private Texture2D _particleTexture;

        private Boomerang _boomerang;
        private Vector2 _position;

        public BoomerangTrail(Texture2D trailParticleTexture2D, Boomerang boomerang = null)
        {
            if(boomerang == null)
            {
                throw new ArgumentNullException("Null Boomerang cannot be set for BoomerangTrail");
            }
            _boomerang = boomerang;
            _position = _boomerang.Position;

            _particleTexture = trailParticleTexture2D;
            _particleTexture.SetData(new[] { Color.White });

            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture);  // Has to load content in Game.LoadContent()
            _particleEffect = new ParticleEffect(autoTrigger: false)
            {
                Position = _position,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(0.5),
                        Profile.Point())
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(0f, 50f),
                            Quantity = 3,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(3.0f, 4.0f)
                        },
                        Modifiers =
                        {
                            new AgeModifier
                            {
                                Interpolators =
                                {
                                    new ColorInterpolator
                                    {
                                        StartValue = new HslColor(0.33f, 0.5f, 0.5f),
                                        EndValue = new HslColor(0.5f, 0.9f, 1.0f)
                                    }
                                }
                            },
                            new RotationModifier {RotationRate = -2.1f},
                            new RectangleContainerModifier {Width = 800, Height = 480},
                            new LinearGravityModifier {Direction = -Vector2.UnitY, Strength = 30f},
                        }
                    }
                }
            };
        }
        public void Update(GameTime gameTime)
        {
            _position = _boomerang.Position;
            _particleEffect.Position = _position;
            _particleEffect.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)  // Remember use the particle sprite batch
        {
            spriteBatch.Draw(_particleEffect);
        }
    }
}
