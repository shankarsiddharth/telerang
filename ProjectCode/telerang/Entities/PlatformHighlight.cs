using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.VectorDraw;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Containers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.TextureAtlases;

namespace telerang.Entities
{
    class PlatformHighlight : IVisualEntity
    {
        public int DrawOrder { get; set; }

        public Vector2 Position { get; set; }

        private ParticleEffect _particleEffect;
        private Texture2D _particleTexture;
        private TextureRegion2D textureRegion;

        private Platform _platform;
        private Vector2 _size
        {
            get => new Vector2(((RectangleF)Boundary).Width,((RectangleF) Boundary).Height);
        }
        public IShapeF Boundary
        {
            get => _platform.Bounds;
        }

        // Particle Parameters
        private float _timeSpan = 0.75f;
        private Color _color = Color.Yellow;

        public PlatformHighlight(Texture2D trailParticleTexture2D, Platform platform)
        {
            if (platform == null)
            {
                throw new ArgumentNullException("Null Platform cannot be set for BoomerangTrail");
            }
            _platform = platform;
            Position = Boundary.Position;
            
            DrawOrder = 0;

            _particleTexture = trailParticleTexture2D;
            _particleTexture.SetData(new[] { _color });
            textureRegion = new TextureRegion2D(_particleTexture);
        }

        public void Update(GameTime gameTime)
        {
            // Test
            if (Keyboard.GetState().IsKeyDown(Keys.Z))
            {
                Enable();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.X))
            {
                Disable();
            }
            // Test end

            if (_particleEffect != null)
            {
                Position = Boundary.Position + _size/2.0f;
                _particleEffect.Position = Position;
                _particleEffect.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (_particleEffect != null)
            {
                spriteBatch.Draw(_particleEffect);
            }
        }

        private void Enable()
        {
            if (_particleEffect != null)
            {
                _particleEffect.Emitters[0].AutoTrigger = true;
            }
            else
            {
                _particleEffect = new ParticleEffect(autoTrigger: false)
                {
                    Position = Position,
                    Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(_timeSpan),
                        Profile.BoxUniform(_size.X, _size.Y))
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(0f, 0f),
                            Quantity = 5,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(3.0f, 4.0f),
                        },
                        Modifiers =
                        {
                            new AgeModifier
                            {
                                //Interpolators =
                                //{
                                //    new ColorInterpolator
                                //    {
                                //        StartValue = HslColor.FromRgb(_color),
                                //        EndValue = HslColor.FromRgb(Color.Transparent)
                                //    }
                                //}
                            },
                            new RotationModifier {RotationRate = -2.1f},
                            //new RectangleContainerModifier {Width = 800, Height = 480},
                            new LinearGravityModifier {Direction = -Vector2.UnitY, Strength = 150f},
                        }
                    }
                }
                };
            }
        }

        private void Disable()
        {
            if (_particleEffect != null)
            {
                _particleEffect.Emitters[0].AutoTrigger = false;
            }
        }

        public void OnBoomerangAim(object sender, TeleRangEventArgs teleRangEventArgs)
        {
            Enable();
        }

        public void OnBoomerangReleased(object sender, TeleRangEventArgs teleRangEventArgs)
        {
            Disable();
        }
    }
}
