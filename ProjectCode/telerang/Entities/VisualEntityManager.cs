using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.VectorDraw;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace telerang.Entities
{
    public class VisualEntityManager
    {
        private readonly List<IVisualEntity> _entities = new List<IVisualEntity>();

        private readonly List<IVisualEntity> _entitiesToAdd = new List<IVisualEntity>();
        private readonly List<IVisualEntity> _entitiesToRemove = new List<IVisualEntity>();

        public IEnumerable<IVisualEntity> Entities => new ReadOnlyCollection<IVisualEntity>(_entities);

        public void Update(GameTime gameTime)
        {
            foreach (IVisualEntity entity in _entities)
            {
                if (_entitiesToRemove.Contains(entity))
                    continue;

                entity.Update(gameTime);
            }

            foreach (IVisualEntity entity in _entitiesToAdd)
            {
                _entities.Add(entity);
            }

            foreach (IVisualEntity entity in _entitiesToRemove)
            {
                _entities.Remove(entity);
            }

            _entitiesToAdd.Clear();
            _entitiesToRemove.Clear();
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            foreach (IVisualEntity entity in _entities.OrderBy(e => e.DrawOrder))
            {
                entity.Draw(spriteBatch, gameTime);
            }
        }

        public void AddEntity(IVisualEntity entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity), "Null cannot be added as an entity.");

            _entitiesToAdd.Add(entity);
        }

        public void RemoveEntity(IVisualEntity entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity), "Null is not a valid entity.");

            _entitiesToRemove.Add(entity);
        }

        public void Clear()
        {
            _entitiesToRemove.AddRange(_entities);
        }

        public IEnumerable<T> GetEntitiesOfType<T>() where T : IVisualEntity
        {
            return _entities.OfType<T>();
        }
    }
}
