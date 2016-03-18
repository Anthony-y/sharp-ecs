﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ECSIsBetter.Exceptions;

namespace ECSIsBetter
{
    /// <summary>
    /// The object that managed all your game Entities.
    /// </summary>
    public class EntityPool
    {
        public delegate void EntityChanged(EntityPool pool, Entity entity);

        public event EntityChanged EntityAdded;
        public event EntityChanged EntityRemoved;

        private List<Entity> _activeEntities;
        private Stack<Entity> _cachedEntities;

        public List<Entity> Entities
        {
            get { return _activeEntities; }
            private set { _activeEntities = value; }
        }

        public Stack<Entity> CachedEntities
        {
            get { return _cachedEntities; }
            private set { _cachedEntities = value; }
        }

        public string Name { get; set; }

        // How many Entites the cache can store at a time.
        private readonly int MAX_CACHED_ENTITIES = 5;

        public static EntityPool New(string name)
        {
            return new EntityPool(name);
        }

        public EntityPool(string name)
        {
            _activeEntities = new List<Entity>();
            _cachedEntities = new Stack<Entity>();

            if (name != null) Name = name;
        }

        public Entity AddEntity(Entity entity)
        {
            _activeEntities.Add(entity);

            if (EntityAdded != null) EntityAdded(this, entity);
            
            return entity;
        }

        /// <summary>
        /// Creates a new Entity with "entityTag", adds it to active Entities and returns it.
        /// </summary>
        /// <param name="entityTag"></param>
        /// <returns>Final Entity</returns>
        public Entity CreateEntity(string entityTag)
        {
            Entity newEntity = null;

            var tagMatch = this.Entities.FirstOrDefault(ent => ent.Tag == entityTag);

            if (tagMatch != null)
            {
                throw new DuplicateEntityException(this);
            }

            if (_cachedEntities.Count > 0)
            {
                newEntity = _cachedEntities.Pop();
                _activeEntities.Add(newEntity);

                if (newEntity != null && _activeEntities.Contains(newEntity))
                {
                    newEntity.Tag = entityTag;
                    newEntity.OwnerPool = this;
                    Console.WriteLine("Retrieved " + newEntity.Tag + " from cache.");
                } else
                {
                    throw new EntityNotFoundException(this);
                }
            } else
            {
                newEntity = new Entity(entityTag, this);
                Console.WriteLine("Made instance because nothing in cache!");
            }

            _activeEntities.Add(newEntity);

            if (EntityAdded != null) EntityAdded(this, newEntity);

            return newEntity;
        }

        /// <summary>
        /// Adds an Entity to the cache to be re-used if cachedEntities isn't full.
        /// If the cache is full, just remove anyway.
        /// </summary>
        /// <param name="entity"></param>
        public void DestroyEntity(Entity entity)
        {
            var held = entity;

            entity.Reset();

            if (_activeEntities.Contains(entity))
            {
                if (_cachedEntities.Count < MAX_CACHED_ENTITIES)
                {
                    _cachedEntities.Push(entity);
                    _activeEntities.Remove(entity);
                }
                else
                {
                    _activeEntities.Remove(entity);
                }
            } else
            {
                throw new EntityNotFoundException(this);
            }

            if (EntityRemoved != null) EntityRemoved(this, held);
        }

        /// <summary>
        /// Doesn't add Entity to cache, just removes.
        /// </summary>
        /// <param name="entity"></param>
        public void UnsafeDestroyEntity(Entity entity)
        {
            if (entity != null)
            {
                _activeEntities.Remove(entity);
                if (EntityRemoved != null) EntityRemoved(this, entity);
            }
            else throw new EntityNotFoundException(this);
        }

        public void WipeCache()
        {
            _cachedEntities.Clear();
        }

        public void WipeEntities()
        {
            _activeEntities.Clear();
        }

        /// <summary>
        /// Operator overload to let you do "pool += entity" to add an Entity to the pool.
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static EntityPool operator + (EntityPool pool, Entity entity)
        {
            pool.AddEntity(entity);

            return pool;
        }

        /// <summary>
        /// Operator overload to let you do "pool -= entity" to remove an Entity from the pool.
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static EntityPool operator - (EntityPool pool, Entity entity)
        {
            if (entity != null)
            {
                pool.DestroyEntity(entity);
                return pool;
            } else
            {
                throw new EntityNotFoundException(pool);
            }
        }

    }
}
