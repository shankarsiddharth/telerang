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

namespace telerang
{
    public class TeleRangGame : Game
    {
        //Public
        public const int WINDOW_WIDTH = 1024;

        public const int WINDOW_HEIGHT = 1024;

        public const string GAME_TITLE = "TeleRang";

        private const string TILEMAP_NAME = "untitled";
        private const string NINJA_SPRITESHEET = "Animation/Ninja";
        private const string BOOMERANG_SPRITESHEET = "boomerang";
        private const string CURSOR_SPRITESHEET = "cursor_hand";
        private const string FLYING_CAR_SPRITESHEET = "flyingcar";

        private const string PLATFORM_LAYER_NAME = "Platform";
        private const string MOVING_PLATFORM_LAYER_NAME = "MovingPlatform";
        private const string OBSTACLES_LAYER_NAME = "Obstacle";
        
        private const int TILE_WIDTH = 64;
        private const int TILE_HEIGHT = 64;

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

        private readonly CollisionComponent _collisionComponent;

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
                new Texture2D(GraphicsDevice, TILE_WIDTH, TILE_HEIGHT),
                new Vector2(GraphicsDevice.Viewport.Width / 2.0f, TILE_HEIGHT))
            {
                DrawOrder = 100
            };

            _spriteSheetTexture = Content.Load<Texture2D>(BOOMERANG_SPRITESHEET);
            _boomerang = new Boomerang(_spriteSheetTexture, _ninja.Position, Content.Load<Texture2D>(CURSOR_SPRITESHEET), _ninja, MAXIMUM_DISTANCE, _tiledMap)
            {
                DrawOrder = 101,  
                MaxTime = TELEPORTING_MAX_TIME,
                TileWidth = TILE_WIDTH,
                TileHeight = TILE_HEIGHT,
                Speed = 1.0f,
                CollisionComponentSimple = _collisionComponent,
                WindowWidth = WINDOW_WIDTH
            };
            _boomerang.OnBoomerangRelease += _ninja.OnBoomerangReleased;
            _boomerang.OnBoomerangTeleport += _ninja.OnBoomerangTeleport;
            _boomerang.OnBoomerangCatch += _ninja.OnBoomerangCatch;


            _entityManager.AddEntity(_ninja);
            _entityManager.AddEntity(_boomerang);
            _collisionComponent.Insert(_ninja);
            _collisionComponent.Insert(_boomerang);

            _entityFactory = new EntityFactory();
            _entityFactory.CreatePlatforms(Content, _collisionComponent, _entityManager, TILEMAP_NAME, PLATFORM_LAYER_NAME);
            _entityFactory.CreateObstacles(Content, _collisionComponent, _entityManager, TILEMAP_NAME, OBSTACLES_LAYER_NAME);
            _spriteSheetTexture = Content.Load<Texture2D>(FLYING_CAR_SPRITESHEET);
            _entityFactory.CreateMovingPlatforms(Content, _collisionComponent, _entityManager, _spriteSheetTexture, TILEMAP_NAME, MOVING_PLATFORM_LAYER_NAME, WINDOW_WIDTH);
        
            // === Particle ===
            _boomerangTrailTexture = new Texture2D(GraphicsDevice, 1, 1);
            _boomerangTrail = new BoomerangTrail(_boomerangTrailTexture, _boomerang);
            _visualEntityManager.AddEntity(_boomerangTrail);

            _boomerang.BoomerangReleased += _boomerangTrail.OnBoomerangReleased;

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
    }
}