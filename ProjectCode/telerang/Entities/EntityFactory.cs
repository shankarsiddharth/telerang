using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Text;

namespace telerang.Entities
{
    class EntityFactory
    {
        public List<Platform> CreatePlatforms
            (
            ContentManager contentManager,
            CollisionComponent collisionComponent,
            EntityManager entityManager, 
            string LEVEL_MAP_NAME, 
            string MAP_LAYER_NAME,
            int DefaultDrawOrder = 95
            )
        {
            List<Platform> Platforms = new List<Platform>();

            TiledMap tiledMap = contentManager.Load<TiledMap>(LEVEL_MAP_NAME);
            TiledMapObjectLayer tiledMapObjectLayer = tiledMap.GetLayer<TiledMapObjectLayer>(MAP_LAYER_NAME);
            for (int i = 0; i < tiledMapObjectLayer.Objects.Length; i++)
            {
                Console.WriteLine(tiledMapObjectLayer.Objects[i].Identifier);
                Platform newPlatform = new Platform(tiledMapObjectLayer.Objects[i])
                {
                    DrawOrder = DefaultDrawOrder,
                    CollisionComponentSimple = collisionComponent
                };
                Platforms.Add(newPlatform);
                entityManager.AddEntity(newPlatform);
                collisionComponent.Insert(newPlatform);
            }
            return Platforms;
        }

        /*public List<Obstacle> CreateObstacles
            (
            ContentManager contentManager,
            CollisionComponent collisionComponent,
            EntityManager entityManager,
            string LEVEL_MAP_NAME,
            string MAP_LAYER_NAME,
            int DefaultDrawOrder = 96
            )
        {
            List<Obstacle> Obstacles = new List<Obstacle>();


        }*/

        /*public Platform CreatePlatform()
        {

        }

        public Obstacle CreateObstacle()
        {

        }


        public List<Obstacle> CreateObstacles()
        {

        }

        public MovingPlatform CreateMovingPlatform()
        {

        }


        public List<MovingPlatform> CreateMovingPlatforms()
        {

        }*/

    }
}
