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
    class BoomerangTrail : IVisualEntity
    {
        public Vector2 Position { get; set; }
        public int DrawOrder { get; set; }

        bool _enabled;

        private ParticleEffect _particleEffect;
        private Texture2D _particleTexture;
        private TextureRegion2D textureRegion;
        private Boomerang _boomerang;
        private Timer _disableTimer;

        // Particle parameters //
        private float _timeSpan = 0.5f;
        // Particle parameters end //

        public BoomerangTrail(Texture2D trailParticleTexture2D, Boomerang boomerang = null)
        {
            if(boomerang == null)
            {
                throw new ArgumentNullException("Null Boomerang cannot be set for BoomerangTrail");
            }
            _boomerang = boomerang;
            Position = _boomerang.Position;
            DrawOrder = 255;

            _particleTexture = trailParticleTexture2D;
            _particleTexture.SetData(new[] { Color.White });
            textureRegion = new TextureRegion2D(_particleTexture);
            _disableTimer = new Timer(_timeSpan);
            _disableTimer.Stop();
            _disableTimer.Elapsed += new ElapsedEventHandler(disableElapsed);
            Disable();
        }
        public void Update(GameTime gameTime)
        {
            // Test
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                Enable();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                Disable();
            }
            // Test end

            if (_enabled)
            {
                Position = _boomerang.Position;
                _particleEffect.Position = Position;
                _particleEffect.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (_enabled)
            {
                spriteBatch.Draw(_particleEffect);
            }
        }

        public void Enable()
        {
            if (_particleEffect != null && !_enabled)
            {
                _particleEffect.Dispose();
            }

            _particleEffect = new ParticleEffect(autoTrigger: false)
            {
                Position = Position,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(_timeSpan),
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
            _enabled = true;
        }

        public void Disable()
        {
            _disableTimer.Start();
            _enabled = false;
        }

        void disableElapsed(object sender, ElapsedEventArgs e)
        {
            if (_particleEffect != null && !_enabled)
            {
                _particleEffect.Dispose();
            }
        }

        public void OnBoomerangReleased(object sender, TeleRangEventArgs teleRangEventArgs)
        {
            Enable();
        }

        public void OnBoomerangCatched(object sender, TeleRangEventArgs teleRangEventArgs)
        {
            Disable();
        }

        public void OnBoomerangTeleported(object sender, TeleRangEventArgs teleRangEventArgs)
        {
            Disable();
        }
    }
}
