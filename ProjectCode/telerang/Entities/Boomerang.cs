using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.VectorDraw;

using System;
using System.Linq;
using System.Collections.Generic;
using telerang.Entities;
using telerang.Shapes;
using Microsoft.Xna.Framework.Audio;

namespace telerang
{
    public class Boomerang : IGameEntity
    {
        public Texture2D SpriteTexture { get; set; }
        public List<SoundEffect> SoundEffects;

        public event EventHandler<TeleRangEventArgs> OnBoomerangRelease;
        public event EventHandler<TeleRangEventArgs> OnBoomerangCatch;
        public event EventHandler<TeleRangEventArgs> OnBoomerangTeleport;
        public event EventHandler<TeleRangEventArgs> OnBoomerangAim;

        private Vector2 position;
        public Vector2 Position { 
            get {return position; } 
            set {
                position = value;
                if(Bounds!=null)Bounds.Position=value;
            } 
        }
        
        public float MaxTime { get; set; }
        public int TileWidth;
        public int TileHeight;
        public float Speed;
        public int WindowWidth;

        public bool IsColliding { get; private set; }

        private Vector2 _startPosition;
        private Texture2D _cursor;
        private Ninja _ninja;
        private float _maximumDistance;
        private float _timer;
        private Vector2 _cursorPosition;
        private TiledMap _tiledMap;
        private TiledMapTileLayer _tiledMapPlatformLayer;
        private TiledMapObjectLayer _tiledMapPlatformObjectLayer;
        private TiledMapObjectLayer _tiledMapFlyingCarObjectLayer;
        private TiledMapImageLayer _tiledMapMaskLayer;
        private TiledMapTile? _tile = null;
        private EntityManager _entityManager;


        private Vector2[] _pathToTravel;
        private int _currentPathIndex = 0;
        private float _distanceTreshold = 0.01f;

        private Vector2 _spritePosition;
        private float _angle = 0;
        private float _angularVelocity = 3.0f;
        private float _anglePassed = 0;

        private bool _isNinjaOnTheMovingPlatform { get; set; }
        private MovingPlatform _ninjaMovingPlatform;

        public Boomerang(Texture2D spriteTexture,List<SoundEffect> soundEffects, Vector2 initialPosition, Texture2D cursor, Ninja ninja, float maximumDistance, TiledMap tiledMap, EntityManager entityManager)
        {
            SpriteTexture = spriteTexture;
            SoundEffects = soundEffects;
            Position = initialPosition;
            _startPosition = Position;           
            _cursor = cursor;
            _ninja = ninja;
            _maximumDistance = maximumDistance;
            _tiledMap = tiledMap;
            _entityManager = entityManager;

            _tiledMapPlatformLayer = _tiledMap.GetLayer<TiledMapTileLayer>("Platforms");
            _tiledMapPlatformObjectLayer = _tiledMap.GetLayer<TiledMapObjectLayer>("Platform");
            _tiledMapFlyingCarObjectLayer = _tiledMap.GetLayer<TiledMapObjectLayer>("MovingPlatform");
            _tiledMapMaskLayer = _tiledMap.GetLayer<TiledMapImageLayer>("Mask");

            float width = SpriteTexture.Width;
            float height = SpriteTexture.Height;
            float maxBound = (width >= height) ? width : height;
            Bounds = new CircleF(new Vector2(Position.X - (width/2.0f), Position.Y - (height/2.0f)), maxBound);

            _ninjaMovingPlatform = null;
        }

        public int DrawOrder { get; set; }

        public IShapeF Bounds { get; private set; }

        public CollisionComponent CollisionComponentSimple { get; set; }

        public void Update(GameTime gameTime)
        {
            double deltaTime = gameTime.ElapsedGameTime.TotalMilliseconds;
            MouseState mouseState = Mouse.GetState();
            Vector2 ninjaPosition = _ninja.Position;
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);

            if (_ninja.State == NinjaState.Aiming)
            {
                ShowMask();
            }
            else
            {
                HideMask();
            }

            switch (_ninja.State)
            {
                case NinjaState.Idle:
                    {
                        IfOnMovingPlatformThenMoveNinja();
                        IfOnWinPlatformThenEndGame();

                        if (mouseState.LeftButton == ButtonState.Pressed)
                        {
                            TeleRangEventArgs teleRangEventArgs = new TeleRangEventArgs();
                            teleRangEventArgs.position = Position;
                            OnBoomerangAim?.Invoke(this, teleRangEventArgs);

                            _ninja.ChangeState(NinjaState.Aiming);
                        }
                    }
                    break;

                case NinjaState.Aiming:
                    {

                        IfOnMovingPlatformThenMoveNinja();
                        IfOnWinPlatformThenEndGame();

                        if (mousePosition.Y <= ninjaPosition.Y)
                        {
                            mousePosition.Y = ninjaPosition.Y;
                        }

                        //float maximumAllowedDistance = (ninjaPosition.Y + _maximumDistance);

                        //if (mousePosition.Y >= maximumAllowedDistance)
                        //{
                        //    mousePosition.Y = maximumAllowedDistance;
                        //}

                        Vector2 difference = Vector2.Subtract(mousePosition, ninjaPosition);

                        if (difference.Length() >= _maximumDistance)
                        {
                            difference.Normalize();
                            mousePosition = Vector2.Add(ninjaPosition, difference * _maximumDistance);
                        }

                        _cursorPosition = mousePosition;
                        if (_cursorPosition.X >= _ninja.Position.X) _ninja.PlayAnim("aimingR", true);
                        else _ninja.PlayAnim("aimingL", true);

                        if (mouseState.LeftButton == ButtonState.Released)
                        {
                            _ninja.targetPosition = mousePosition;
                            _startPosition = _ninja.Position;
                            _timer = 0f;

                            float distance = Vector2.Distance(_ninja.Position, _cursorPosition);
                            Vector2 midpoint = (_ninja.Position + _cursorPosition) / 2.0f;
                            float radius = distance / 2;
                            _pathToTravel = CreateCircle(midpoint, distance / 2.0f);
                            _currentPathIndex = 0;

                            Vector2 A = Vector2.UnitX*radius;//(new Vector2(WindowWidth, 0)) - _ninja.Position;
                            Vector2 B = _ninja.Position-midpoint;
                            _angle = (float)GetAngle(B, A);
                            _anglePassed = 0f;

                            _ninja.ChangeState(NinjaState.Teleporting);
                            SoundEffects[1].Play();
                            //_ninja.Position = mousePosition;
                            TeleRangEventArgs teleRangEventArgs = new TeleRangEventArgs();
                            teleRangEventArgs.position = Position;
                            OnBoomerangRelease?.Invoke(this, teleRangEventArgs);

                           /* Vector2 A = (new Vector2(WindowWidth, 0)) - _ninja.Position;
                            Vector2 B = _cursorPosition - _ninja.Position;
                            double angle = GetAngle(B, A);
                            float ry = distance / 2.0f;
                            float rx = ry / 2.0f;
                            _pathToTravel = CreateEllipse(rx, ry, (int)midpoint.X, (int)midpoint.Y, 64, (float)angle);
                            _currentPathIndex = 0;
                            TeleRangEventArgs teleRangEventArgs = new TeleRangEventArgs();
                            teleRangEventArgs.position = Position;
                            OnBoomerangRelease?.Invoke(this, teleRangEventArgs);
                            _ninja.ChangeState(NinjaState.Teleporting);*/
                        }
                    }
                    break;

                case NinjaState.Teleporting:
                    {
                        //MoveInLine(gameTime, mouseState);
                        MoveInCircle(gameTime, mouseState);
                        CollisionComponentSimple.Update(gameTime);
                    }
                    break;

                case NinjaState.Teleported:
                    {


                    }
                    break;

                case NinjaState.Win:
                    break;
            }
        }

        internal void RegisterEvents(ref List<MovingPlatform> movingPlatform)
        {
            for(int i = 0; i < movingPlatform.Count; i++)
            {
                movingPlatform[i].OnMovingPlatformSwitchedSides += OnMovingPlatformSideSwitched;
            }
        }

        private void IfOnMovingPlatformThenMoveNinja()
        {
            MovingPlatform CurrentMovingPlatform = GetCurrentMovingPlatform();            
            if (CurrentMovingPlatform != null)
            {
                RectangleF rectangleF = (RectangleF)CurrentMovingPlatform.Bounds;
                Vector2 newPosition = new Vector2(CurrentMovingPlatform.Position.X + (rectangleF.Width / 2.0f), CurrentMovingPlatform.Position.Y + (rectangleF.Height / 2.0f));
                _ninja.Position = newPosition;                
                _isNinjaOnTheMovingPlatform = true;
                _ninjaMovingPlatform = CurrentMovingPlatform;
            }
            else
            {
                _isNinjaOnTheMovingPlatform = false;
                _ninjaMovingPlatform = null;
            }
        }

        private void IfOnWinPlatformThenEndGame()
        {
            Platform currentPlatform = GetCurrentPlatform();
            if (currentPlatform != null)
            {
                string isWin = null;
                isWin = currentPlatform.MapObject.Properties.GetValueOrDefault<string, string>("iswin");
                if (isWin != null)
                {
                    if (isWin.Equals("iswin"))
                    {
                        _ninja.ChangeState(NinjaState.Win);
                    }
                }
            }
        }

        private void MoveInLine(GameTime gameTime, MouseState mouseState)
        {
            if (_timer <= MaxTime)
            {
                Position = Vector2.LerpPrecise(_startPosition, _ninja.targetPosition, _timer / MaxTime);
                if (mouseState.LeftButton == ButtonState.Pressed
                    && _timer > 2f * (float)gameTime.ElapsedGameTime.TotalMilliseconds)
                {
                    _ninja.ChangeState(NinjaState.Teleported);
                    TeleportNinja();
                }
            }
            else if (_timer >= 2 * MaxTime)
            {
                _ninja.Position = _startPosition;
                Position = _startPosition;
                _timer = 0f;

                TeleRangEventArgs teleRangEventArgs = new TeleRangEventArgs();
                teleRangEventArgs.position = Position;
                OnBoomerangCatch?.Invoke(this, teleRangEventArgs);

                _ninja.ChangeState(NinjaState.Idle);
            }
            else
            {
                Position = Vector2.LerpPrecise(_startPosition, _ninja.targetPosition, (2f - _timer / MaxTime));
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    _ninja.ChangeState(NinjaState.Teleported);
                    TeleportNinja();
                }
            }
            float width = SpriteTexture.Width;
            float height = SpriteTexture.Height;
            Bounds.Position = new Vector2(Position.X + (width / 2.0f), Position.Y + (height / 2.0f));

            _timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        }

        private void MoveInCircle(GameTime gameTime, MouseState mouseState)
        {
            float distance = Vector2.Distance(_ninja.Position, _cursorPosition);
            float radius = distance / 2.0f;
            Vector2 midpoint = (_ninja.Position + _cursorPosition) / 2.0f;
            if (_anglePassed >= 2 * MathF.PI)
            {
                _ninja.Position = _startPosition;
                Position = _startPosition;
                _anglePassed = 0f;
                TeleRangEventArgs teleRangEventArgs = new TeleRangEventArgs();
                teleRangEventArgs.position = Position;
                OnBoomerangCatch?.Invoke(this, teleRangEventArgs);
                _ninja.ChangeState(NinjaState.Idle);
            }
            else {
                float delta= (float)(gameTime.ElapsedGameTime.TotalSeconds * _angularVelocity);
                if (_cursorPosition.X >= _ninja.Position.X) _angle += delta;
                else _angle -= delta;
                _anglePassed += delta;
                var displacedPosition = midpoint + new Vector2(radius * (float)Math.Cos(_angle), radius * (float)Math.Sin(_angle));
                Position = displacedPosition;
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    _ninja.ChangeState(NinjaState.Teleported);
                    TeleportNinja();
                }
            }
            

            /* Vector2 currentPosition = _pathToTravel[_currentPathIndex];
             _startPosition = currentPosition;
             Vector2 nextPositionInPath = _pathToTravel[(_currentPathIndex + 1) % _pathToTravel.Length];
             _ninja.targetPosition = nextPositionInPath;
             //Vector2 differceInPosition = (nextPositionInPath - currentPosition);
             //differceInPosition.Normalize();


             if (_timer <= MaxTime)
             {
                 //Position = Vector2.LerpPrecise(_startPosition, _ninja.targetPosition, _timer / MaxTime);
                 Position = Vector2.LerpPrecise(currentPosition, nextPositionInPath, _timer / MaxTime);
                 if (Vector2.DistanceSquared(Position, nextPositionInPath) < _distanceTreshold)
                 {
                     _currentPathIndex += 1;
                 }
                 if (mouseState.LeftButton == ButtonState.Pressed
                     && _timer > 2f * (float)gameTime.ElapsedGameTime.TotalMilliseconds)
                 {
                     _ninja.ChangeState(NinjaState.Teleported);
                     TeleportNinja();
                 }
             }
             else if (_timer >= 2 * MaxTime)
             {
                 _ninja.Position = _startPosition;
                 Position = _startPosition;
                 _timer = 0f;

                 TeleRangEventArgs teleRangEventArgs = new TeleRangEventArgs();
                 teleRangEventArgs.position = Position;
                 OnBoomerangCatch?.Invoke(this, teleRangEventArgs);

                 _ninja.ChangeState(NinjaState.Idle);
             }
             else
             {
                 //Position = Vector2.LerpPrecise(_startPosition, _ninja.targetPosition, (2f - _timer / MaxTime));
                 Position = Vector2.LerpPrecise(currentPosition, nextPositionInPath, (2f - _timer / MaxTime));
                 if (Vector2.DistanceSquared(Position, nextPositionInPath) < _distanceTreshold)
                 {
                     _currentPathIndex += 1;
                 }
                 if (mouseState.LeftButton == ButtonState.Pressed)
                 {
                     _ninja.ChangeState(NinjaState.Teleported);
                     TeleportNinja();
                 }
             }
             float width = SpriteTexture.Width;
             float height = SpriteTexture.Height;
             Bounds.Position = new Vector2(Position.X + (width / 2.0f), Position.Y + (height / 2.0f));

             _timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

             *//*Vector2 currentPosition = _pathToTravel[_currentPathIndex];
             Vector2 nextPositionInPath = _pathToTravel[(_currentPathIndex + 1) % _pathToTravel.Length];
             Vector2 differceInPosition = (nextPositionInPath - currentPosition);
             differceInPosition.Normalize();
             Position = differceInPosition * (float)deltaTime * Speed;
             if (Vector2.DistanceSquared(Position, nextPositionInPath) < _distanceTreshold)
             {
                 _currentPathIndex += 1;
             }*/
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (_ninja.State)
            {
                case NinjaState.Idle:
                    {
                        spriteBatch.Draw(_cursor, Mouse.GetState().Position.ToVector2(), Color.White);
                    }
                    break;

                case NinjaState.Aiming:
                    {
                        spriteBatch.Draw(_cursor, _cursorPosition, Color.White);
                        //Primitives2D.DrawPoints(spriteBatch, _pathToTravel, Color.Black, 1.0f);
                    }
                    break;

                case NinjaState.Teleporting:
                    {
                        spriteBatch.Draw(_cursor, Mouse.GetState().Position.ToVector2(), Color.White);

                        _spritePosition = new Vector2(Position.X - (SpriteTexture.Width / 2.0f), Position.Y - (SpriteTexture.Height / 2.0f));
                        spriteBatch.Draw(SpriteTexture, _spritePosition, Color.White);
                        //Primitives2D.DrawPoints(spriteBatch, _pathToTravel, Color.Black, 1.0f);
                        //spriteBatch.DrawCircle((CircleF)Bounds, 16, Color.Red);
                    }
                    break;

                case NinjaState.Teleported:
                    {
                        spriteBatch.Draw(_cursor, Mouse.GetState().Position.ToVector2(), Color.White);
                    }
                    break;
            }
        }
        public void TeleportNinja() {
            TeleRangEventArgs teleRangEventArgs = new TeleRangEventArgs();
            teleRangEventArgs.position = Position;
            OnBoomerangTeleport?.Invoke(this, teleRangEventArgs);
            _ninja.sprite.Play("teleportStart", () => {
                _ninja.Position = Position;
                ushort x = (ushort)(Position.X / TileWidth);
                ushort y = (ushort)(Position.Y / TileHeight);

                if (CheckAbyss())
                //if (IsAbyss(x, y))
                {
                    //Console.WriteLine("Dead");
                    SoundEffects[2].Play();
                    _ninja.sprite.Play("fall", () =>
                    {
                        _ninja.ChangeState(NinjaState.Idle);
                        _ninja.ReSpawn();
                    });
                }
                else
                {
                    SoundEffects[3].Play();
                    _ninja.sprite.Play("teleportEnd", () =>
                    {

                        //Console.WriteLine("Still Alive");
                        _ninja.ChangeState(NinjaState.Idle);
                        IfOnMovingPlatformThenMoveNinja();
                        IfOnWinPlatformThenEndGame();
                    });
                }
            });
        }

        public void DrawPrimitives(PrimitiveDrawing primitiveDrawing, GameTime gameTime)
        {

           /* List<TiledMapObject> objects = new List<TiledMapObject>();

            TiledMapObject[] platformObjects = _tiledMapPlatformObjectLayer.Objects;
            TiledMapObject[] flyingCarObjects = _tiledMapFlyingCarObjectLayer.Objects;

            for (int i = 0; i < platformObjects.Length; i++)
            {
                objects.Add(platformObjects[i]);
            }
            for (int i = 0; i < flyingCarObjects.Length; i++)
            {
                objects.Add(flyingCarObjects[i]);
            }
            for (int i = 0; i < objects.Count; i++)
            {
                RectangleF boundingBox = new RectangleF(objects[i].Position, objects[i].Size);
                //primitiveDrawing.DrawRectangle(new Vector2(boundingBox.X, boundingBox.Y), boundingBox.Width, boundingBox.Height, Color.Green);
            }*/

            switch (_ninja.State)
            {
                case NinjaState.Idle:
                    {
                    }
                    break;

                case NinjaState.Aiming:
                    {
                        float distance = Vector2.Distance(_ninja.Position, _cursorPosition);   
                        Vector2 midpoint = (_ninja.Position + _cursorPosition ) / 2.0f;
                        primitiveDrawing.DrawCircle(midpoint, distance/2.0f, Color.Orange);                        
                    }
                    break;

                case NinjaState.Teleporting:
                    {
                    }
                    break;

                case NinjaState.Teleported:
                    {
                    }
                    break;
            }
        }
        
        private bool CheckAbyss()
        {
            bool abyss = true;

            List<MovingPlatform> movingPlatforms = _entityManager.GetEntitiesOfType<MovingPlatform>().ToList();
            List<Platform> platforms = _entityManager.GetEntitiesOfType<Platform>().ToList();
            List<Obstacle> obstacle = _entityManager.GetEntitiesOfType<Obstacle>().ToList();

            for (int i = 0; i < platforms.Count; i++)
            {
                RectangleF rectangleF = (RectangleF)platforms[i].Bounds;
                if (rectangleF.Contains(_ninja.Position))
                {
                    abyss = false;
                }
            }
            for (int i = 0; i < movingPlatforms.Count; i++)
            {
                RectangleF rectangleF = (RectangleF)movingPlatforms[i].Bounds;
                if (rectangleF.Contains(_ninja.Position))
                {
                    abyss = false;                    
                }
            }            
            for (int i = 0; i < obstacle.Count; i++)
            {
                RectangleF rectangleF = (RectangleF)obstacle[i].Bounds;
                if (rectangleF.Contains(_ninja.Position))
                {
                    abyss = false;
                }
            }
            return abyss;
        }

        private MovingPlatform GetCurrentMovingPlatform()
        {
            MovingPlatform movingPlatform = null;
            List<MovingPlatform> objects = _entityManager.GetEntitiesOfType<MovingPlatform>().ToList();
            for (int i = 0; i < objects.Count; i++)
            {
                RectangleF boundingBox = (RectangleF)objects[i].Bounds;
                if (boundingBox.Contains(_ninja.Position))
                {
                    return objects[i];
                }
            }
            return movingPlatform;
        }

        private Platform GetCurrentPlatform()
        {
            Platform platform = null;
            List<Platform> objects = _entityManager.GetEntitiesOfType<Platform>().ToList();
            for (int i = 0; i < objects.Count; i++)
            {
                RectangleF boundingBox = (RectangleF)objects[i].Bounds;
                if (boundingBox.Contains(_ninja.Position))
                {
                    return objects[i];
                }
            }
            return platform;
        }

        private bool IsAbyss(ushort x, ushort y)
        {            
            bool abyss = true;            

            List<TiledMapObject> objects = new List<TiledMapObject>();

            TiledMapObject[] platformObjects = _tiledMapPlatformObjectLayer.Objects;
            TiledMapObject[] flyingCarObjects = _tiledMapFlyingCarObjectLayer.Objects;

            for (int i = 0; i < platformObjects.Length; i++)
            {
                objects.Add(platformObjects[i]);
            }
            for (int i = 0; i < flyingCarObjects.Length; i++)
            {
                objects.Add(flyingCarObjects[i]);
            }
            for (int i = 0; i < objects.Count; i++)
            {
                RectangleF boundingBox = new RectangleF(objects[i].Position, objects[i].Size);
                if (boundingBox.Contains(_ninja.Position))
                {
                    abyss = false;
                    return abyss;
                }
            }
            return abyss;
        }
        
        /* private TiledMapObject GetCurrentMovingPlatform()
         {
             TiledMapObject movingPlatform = null;
             List<TiledMapObject> objects = new List<TiledMapObject>();           
             TiledMapObject[] flyingCarObjects = _tiledMapFlyingCarObjectLayer.Objects;                        
             for (int i = 0; i < flyingCarObjects.Length; i++)
             {
                 objects.Add(flyingCarObjects[i]);
             }
             for (int i = 0; i < objects.Count; i++)
             {
                 RectangleF boundingBox = new RectangleF(objects[i].Position, objects[i].Size);
                 if (boundingBox.Contains(Position))
                 {
                     return objects[i];
                 }
             }
             return movingPlatform;
         }*/

        private bool IsObject(ushort x, ushort y)
        {
            //TiledMapObjectLayer _tiledMapPlatformObjectLayer = _tiledMap.GetLayer<TiledMapObjectLayer>("Objects"); ;
            //TiledMapObject? _tile = null;
            //_tiledMapPlatformObjectLayer.Objects[0].Size

            if (_tiledMapPlatformLayer == null)
            {
                return false;
            }

            _tiledMapPlatformLayer.TryGetTile(x, y, out _tile);
            if (_tile.HasValue)
            {
                TiledMapTile tile = (TiledMapTile)(_tile);
                if (tile.IsBlank)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public static Vector2[] CreateEllipse(float rx, float ry, int h, int k, int sides, float theta)
        {
            var vertices = new Vector2[sides];

            var t = 0.0;
            var dt = 2.0 * Math.PI / sides;
            float angle = MathHelper.ToRadians(theta);

            for (var i = 0; i < sides; i++, t += dt)
            {
                var x = h + (float)(rx * Math.Cos(t));
                var y = k + (float)(ry * Math.Sin(t));
                Vector2 position = new Vector2(x, y);
                position = Vector2.Transform(position, Matrix.CreateFromAxisAngle(new Vector3(rx,ry,0), angle));
                vertices[i] = position;
            }
            return vertices;
        }

        private float GetInverseProportionOfY(float y) 
        {
            float PROPORTION_CONSTANT = 5.0f;
            return y / PROPORTION_CONSTANT;
        }


        public static Vector2[] CreateCircle(Vector2 center, float radius)
        {

            //Primitives2D.Cre

            const int CircleSegments = 32;
            const double increment = Math.PI * 2.0 / CircleSegments;
            double theta = 0.0;

            Vector2[] VectorList = new Vector2[CircleSegments * 2];

            for (int i = 0; i < CircleSegments; i++)
            {
                Vector2 v1 = center + radius * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                Vector2 v2 = center + radius * new Vector2((float)Math.Cos(theta + increment), (float)Math.Sin(theta + increment));

                VectorList[(i * 2)] = (v1);
                VectorList[((i *2) + 1)] = (v2);

                theta += increment;
            }

            return VectorList;
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            //throw new NotImplementedException();
            //Console.WriteLine(collisionInfo.Other.GetType().Name);
            if (collisionInfo.Other.GetType().Name == "Obstacle") {
                SoundEffects[0].Play();
                _ninja.ChangeState(NinjaState.Idle);
                Position = _startPosition;
                TeleRangEventArgs teleRangEventArgs = new TeleRangEventArgs();
                teleRangEventArgs.position = Position;
                OnBoomerangTeleport?.Invoke(this, teleRangEventArgs);
            }
           /* if (collisionInfo.Other.GetType().Name != "Platform")
            {
                _ninja.IsAlive = false;
                Position = _startPosition;                
            }*/
        }


        private double GetAngle(Vector2 VectorA, Vector2 VectorB)
        {
            Vector2 p = new Vector2(-VectorB.Y, VectorB.X);
            float b_coord = Vector2.Dot(VectorA, VectorB);
            float p_coord = Vector2.Dot(VectorA, p);
            return Math.Atan2(p_coord, b_coord);
        }

        private void ShowMask()
        {
            _tiledMapMaskLayer.IsVisible = true;
        }

        private void HideMask()
        {
            _tiledMapMaskLayer.IsVisible = false;
        }

        public void OnMovingPlatformSideSwitched(object sender, TeleRangEventArgs teleRangEventArgs)
        {
            if(_isNinjaOnTheMovingPlatform)
            {
                MovingPlatform movingPlatform = (MovingPlatform)sender;
                if(_ninjaMovingPlatform == movingPlatform)
                {
                    RectangleF rectangleF = (RectangleF)movingPlatform.Bounds;
                    Vector2 newPosition = new Vector2(movingPlatform.Position.X + (rectangleF.Width / 2.0f), movingPlatform.Position.Y + (rectangleF.Height / 2.0f));
                    _ninja.Position = newPosition;
                }
            }
        }
    }
}