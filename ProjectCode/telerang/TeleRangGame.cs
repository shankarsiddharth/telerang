using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.VectorDraw;
using MonoGame.Extended.Content;
using MonoGame.Extended.Sprites;
using telerang.Entities;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Screens;
using telerang.Screens;
using MonoGame.Extended.Screens.Transitions;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;

namespace telerang
{
    public class TeleRangGame : Game
    {
        //Public
        public const int WINDOW_WIDTH = 960;

        public const int WINDOW_HEIGHT = 960;

        public const string GAME_TITLE = "TeleRang";

        private const string TILEMAP_NAME = "Level 1.2";
        //private const string TILEMAP_NAME = "level2";
        private const string NINJA_SPRITESHEET = "Animation/Ninja";
        private const string BOOMERANG_SPRITESHEET = "boomerang";
        private const string CURSOR_SPRITESHEET = "cursor_hand";
        private const string FLYING_CAR_SPRITESHEET = "flyingcar";

        private const string PLATFORM_LAYER_NAME = "Platform";
        private const string MOVING_PLATFORM_LAYER_NAME = "MovingPlatform";
        private const string OBSTACLES_LAYER_NAME = "Obstacle";
        
        private const int TILE_WIDTH = 128;
        private const int TILE_HEIGHT = 128;

        private const float MAXIMUM_DISTANCE = 700.0f;
        private const float TELEPORTING_MAX_TIME = 500.0f;

        //Private
        private GraphicsDeviceManager _graphics;

        private SpriteBatch _spriteBatch;
        private TiledMap _tiledMap;
        private TiledMapRenderer _tiledMapRenderer;

        private PrimitiveDrawing _primitiveDrawing;
        private PrimitiveBatch _primitiveBatch;
        private Matrix _localProjection;
        private Matrix _localView;

        private EntityManager _entityManager;
        private EntityFactory _entityFactory;

        private Ninja _ninja;
        private Boomerang _boomerang;

        private Texture2D _spriteSheetTexture;
        private List<SoundEffect> _soundEffects;
        private Song _bgm;

        private readonly CollisionComponent _collisionComponent;
        private readonly ScreenManager _screenManager;

        // === Particle ===

        private BoomerangTrail _boomerangTrail;
        private Texture2D _boomerangTrailTexture;

        private VisualEntityManager _visualEntityManager;
        // === Particle end ===
         

        public TeleRangGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _entityManager = new EntityManager();
            _visualEntityManager = new VisualEntityManager();
            IsMouseVisible = false;
            _collisionComponent = new CollisionComponent(new RectangleF(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT));
            _screenManager = new ScreenManager();
            Components.Add(_screenManager);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Window.Title = GAME_TITLE;

            _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _primitiveBatch = new PrimitiveBatch(GraphicsDevice);
            _primitiveDrawing = new PrimitiveDrawing(_primitiveBatch);
            _localProjection = Matrix.CreateOrthographicOffCenter(0f, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
            _localView = Matrix.Identity;

            _tiledMap = Content.Load<TiledMap>(TILEMAP_NAME);
            _tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, _tiledMap);

            _spriteSheetTexture = Content.Load<Texture2D>(NINJA_SPRITESHEET);
            _ninja = new Ninja(
                Content.Load<SpriteSheet>("Animation/Ninja.sf", new JsonContentLoader()),
                new Texture2D(GraphicsDevice, 120, 120),
                new Vector2(GraphicsDevice.Viewport.Width / 2.0f, TILE_HEIGHT))
            {
                DrawOrder = 100
            };

            _spriteSheetTexture = Content.Load<Texture2D>(BOOMERANG_SPRITESHEET);

            _soundEffects = new List<SoundEffect>();
            _soundEffects.Add(Content.Load<SoundEffect>("Audio/Boomerang_Hits_Obstacle_Sound"));
            _soundEffects.Add(Content.Load<SoundEffect>("Audio/Boomerang_Throw_Sound"));
            _soundEffects.Add(Content.Load<SoundEffect>("Audio/Player_Falls"));
            _soundEffects.Add(Content.Load<SoundEffect>("Audio/Quick_Teleport_Sound_new"));

            _bgm=Content.Load<Song>("Audio/ClubBeat_14_V3");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.5f;
            MediaPlayer.Play(_bgm);

            _boomerang = new Boomerang(_spriteSheetTexture,_soundEffects, _ninja.Position, Content.Load<Texture2D>(CURSOR_SPRITESHEET), _ninja, MAXIMUM_DISTANCE, _tiledMap, _entityManager)
            {
                DrawOrder = 101,  
                MaxTime = TELEPORTING_MAX_TIME,
                TileWidth = TILE_WIDTH,
                TileHeight = TILE_HEIGHT,
                Speed = 1.0f,
                CollisionComponentSimple = _collisionComponent,
                WindowWidth = WINDOW_WIDTH
            };

            //_boomerang.OnBoomerangRelease += _ninja.OnBoomerangReleased;

            _entityManager.AddEntity(_ninja);
            _entityManager.AddEntity(_boomerang);
            _collisionComponent.Insert(_ninja);
            _collisionComponent.Insert(_boomerang);

            _entityFactory = new EntityFactory();
            _entityFactory.CreatePlatforms(Content, _collisionComponent, _entityManager, TILEMAP_NAME, PLATFORM_LAYER_NAME);
            _entityFactory.CreateObstacles(Content, _collisionComponent, _entityManager, TILEMAP_NAME, OBSTACLES_LAYER_NAME);
            _spriteSheetTexture = Content.Load<Texture2D>(FLYING_CAR_SPRITESHEET);
            _entityFactory.CreateMovingPlatforms(Content, _collisionComponent, _entityManager,
                _spriteSheetTexture, TILEMAP_NAME, MOVING_PLATFORM_LAYER_NAME, WINDOW_WIDTH,
                2);
            // === Particle ===
            _boomerangTrailTexture = new Texture2D(GraphicsDevice, 1, 1);
            _boomerangTrail = new BoomerangTrail(_boomerangTrailTexture, _boomerang);
            _visualEntityManager.AddEntity(_boomerangTrail);

            _boomerang.OnBoomerangRelease += _boomerangTrail.OnBoomerangReleased;
            _boomerang.OnBoomerangCatch += _boomerangTrail.OnBoomerangCatched;
            _boomerang.OnBoomerangTeleport += _boomerangTrail.OnBoomerangTeleported;

            // TODO: add visual entity factory for moving platform
            // === Particle end ===
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            _tiledMapRenderer.Update(gameTime);
            _entityManager.Update(gameTime);
            //_collisionComponent.Update(gameTime);

            // === Particle ===
            _visualEntityManager.Update(gameTime);
            // === Particle end ===
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            _tiledMapRenderer.Draw();

            _spriteBatch.Begin();
            _entityManager.Draw(_spriteBatch, gameTime);
            // === Particle ===
            _visualEntityManager.Draw(_spriteBatch, gameTime);
            // === Particle end ===
            _spriteBatch.End();

            _primitiveBatch.Begin(ref _localProjection, ref _localView);
            _entityManager.DrawPrimitives(_primitiveDrawing, gameTime);
            _primitiveBatch.End();



            base.Draw(gameTime);
        }

        private void LoadLevelLoader()
        {
            _screenManager.LoadScreen(new LevelLoader(this), new FadeTransition(GraphicsDevice, Color.Black));
        }

        private void LoadLevel1()
        {
            _screenManager.LoadScreen(new Level1(this), new FadeTransition(GraphicsDevice, Color.Black));
        }

        private void LoadLevel2()
        {
            _screenManager.LoadScreen(new Level2(this), new FadeTransition(GraphicsDevice, Color.Black));
        }

    }
}