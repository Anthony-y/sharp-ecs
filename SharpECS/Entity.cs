﻿using System;
using System.Collections.Generic;
using System.Linq;

using SharpECS.Exceptions;

namespace SharpECS
{
    public sealed class Entity
    {
        /// <summary>
        /// Events
        /// </summary>
        public event Action<Entity, IComponent> ComponentAdded;
        public event Action<Entity, IComponent> ComponentRemoved;

        public string Tag { get; set; }

        /// <summary>
        /// The pool which this Entity resides in.
        /// </summary>
        public EntityPool OwnerPool { get; set; }
        
        /// <summary>
        /// A list of this Entity's components.
        /// </summary>
        public List<IComponent> Components { get; set; }

        internal Entity(string tag, EntityPool pool)
        {   
            Tag = tag;

            if (pool == null)
            {
                throw new IndependentEntityException(this);
            }
            OwnerPool = pool;

            Components = new List<IComponent>();
        }
        
        /// <summary>
        /// Checks if this already has "component"
        /// If it does, ComponentAlreadyExistsException.
        /// Otherwise, add the component to "Components".
        /// And fire the ComponentAdded event (if it's also not null)
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        private IComponent AddComponent(IComponent component)
        {
            // If it has a component of the same type as "component".
            if (Components.FirstOrDefault(com => com.GetType() == component.GetType()) != null)
            {
                throw new ComponentAlreadyExistsException(this);
            }

            Components.Add(component);
            ComponentAdded?.Invoke(this, component);
            OwnerPool.ComponentAdded(this);

            return component;
        }

        public void RemoveComponent<T>() where T : IComponent
        {
            if (HasComponent<T>())
            {
                var componentToRemove = GetComponent<T>();

                Components.Remove(componentToRemove);
                ComponentRemoved?.Invoke(this, componentToRemove);
            }
            else
            {
                throw new ComponentNotFoundException(this);
            }
        }

        /// <summary>
        /// Checks through Components for a component of type "T"
        /// If it doesn't have one, throw ComponentNotFoundException.
        /// Otherwise returns the component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>() where T : IComponent
        {
            var match = Components.OfType<T>().FirstOrDefault();

            if (match == null)
            {
                throw new ComponentNotFoundException(this);
            }
            return match;
        }

        /// <summary>
        /// Moves a component between this and "destination".
        /// If destination or the component are null, throw a ComponentNotFoundException.
        /// Otherwise add the component to the destination and remove it from this.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="destination"></param>
        public void MoveComponent(IComponent component, Entity destination)
        {
            // If the component itself isn't null and its actually on "this".
            if (component != null && Components.FirstOrDefault(com => com.GetType() == component.GetType()) != null)
            {
                destination.AddComponent(component);
                Components.Remove(component);
            }
            else
            {
                throw new ComponentNotFoundException(this);
            }
        }

        /// <summary>
        /// Checks if "this" contains a component of type "TComponent".
        /// If it does, return true. Otherwise, return false.
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        public bool HasComponent<TComponent>() where TComponent : IComponent
        {
            var match = Components.OfType<TComponent>().FirstOrDefault();
            return match != null;
        }

        /// <summary>
        /// Remove all components.
        /// </summary>
        public void RemoveAllComponents()
        {
            Components.Clear();
        }

        /// <summary>
        /// RemoveAllComponents(), reset Tag, OwnerPool.
        /// </summary>
        public void Reset()
        {
            RemoveAllComponents();

            Tag = string.Empty;
            OwnerPool = null;
        }

        public void AddComponents(IEnumerable<IComponent> components)
        {
            components.ToList().ForEach(com => AddComponent(com));
        }

        /// <summary>
        /// Allows an infinite(?) number of components as parameters and adds them all at once to "this".
        /// </summary>
        /// <param name="components"></param>
        public void AddComponents(params IComponent[] components)
        {
            // Can't just call "AddComponents(IEnumerable<IComponent> components)" because
            // the compiler can't tell the difference between the two signatures.
            components.ToList().ForEach(com => AddComponent(com));
        }

        /// <summary>
        /// Moves this Entity to another EntityPool (if it isn't null)
        /// </summary>
        /// <param name="pool"></param>
        public void MoveTo(EntityPool pool)
        {
            if (pool != null)
            {
                pool.AddEntity(this);
                OwnerPool.DestroyEntity(this);
                OwnerPool = pool;
            }
            else
            {
                throw new NullEntityPoolException(pool);
            }
        }

        /// <summary>
        /// Creates a "carbon copy" of this Entity with the tag of "newTag".
        /// </summary>
        /// <param name="newTag"></param>
        /// <returns></returns>
        public Entity CarbonCopy(string newTag)
        {
            var newEntity = OwnerPool.CreateEntity(newTag);

            newEntity.AddComponents(Components);

            return newEntity;
        }

        /// <summary>
        /// Lets you add "component" to "entity" with a +=
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        public static Entity operator+ (Entity entity, IComponent component)
        {
            if (entity != null && component != null)
            {
                entity.AddComponent(component);
                return entity;
            }
            else
            {
                throw new Exception();
            }
        }
    }
}