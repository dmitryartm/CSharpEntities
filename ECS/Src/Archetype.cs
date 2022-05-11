using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ECS {
    public class Archetype {
        internal Archetype(
                ComponentTypeIndexer componentTypeIndexer,
                SharedComponentIndexer sharedComponentIndexer,
                ImmutableSortedSet<int> components,
                ImmutableSortedDictionary<int, SharedComponentIndex> sharedComponents) {
            this.componentTypeIndexer = componentTypeIndexer;
            this.sharedComponentIndexer = sharedComponentIndexer;
            this.components = components;
            this.sharedComponents = sharedComponents;
        }

        internal Archetype(ComponentTypeIndexer componentTypeIndexer, SharedComponentIndexer sharedComponentIndexer)
            : this(componentTypeIndexer, sharedComponentIndexer, ImmutableSortedSet<int>.Empty, ImmutableSortedDictionary<int, SharedComponentIndex>.Empty) { }

        internal readonly ComponentTypeIndexer componentTypeIndexer;
        internal readonly SharedComponentIndexer sharedComponentIndexer;
        internal readonly ImmutableSortedSet<int> components;
        internal readonly ImmutableSortedDictionary<int, SharedComponentIndex> sharedComponents;

        private string _id;
        public string Id {
            get {
                if (components.IsEmpty && this.sharedComponents.IsEmpty) {
                    _id = string.Empty;
                }

                if (_id == null) {
                    var id = new StringBuilder();
                    foreach (var component in components) {
                        id.Append(component);
                        id.Append(';');
                    }
                    id.AppendLine();
                    foreach (var sharedComponent in sharedComponents.Values) {
                        id.Append(sharedComponent.TypeIndex);
                        id.Append(':');
                        id.Append(sharedComponent.ValueIndex);
                        id.Append(';');
                    }
                    _id = id.ToString();
                }
                return _id;
            }
        }

        public int ComponentsCount => this.components.Count;

        public int SharedComponentsCount => this.sharedComponents.Count;

        public bool IsEmpty => this.ComponentsCount == 0 && this.SharedComponentsCount == 0;

        public Type Component(int index) => this.componentTypeIndexer[this.components[index]];

        public IEnumerable<Type> Components {
            get {
                foreach (var component in this.components) {
                    yield return this.componentTypeIndexer[component];
                }
            }
        }

        public IEnumerable<object> SharedComponents {
            get {
                foreach (var component in this.sharedComponents.Values) {
                    yield return this.sharedComponentIndexer.Value(component);
                }
            }
        }

        public bool HasComponents(Type type) {
            return this.components.Contains(this.componentTypeIndexer[type]);
        }

        public bool HasComponents<T>() {
            return this.HasComponents(typeof(T));
        }
        
        public bool HasComponents<T0, T1>() {
            return this.HasComponents<T0>() && this.HasComponents<T1>();
        }
        
        public bool HasComponents<T0, T1, T2>() {
            return this.HasComponents<T0>() && this.HasComponents<T1>() && this.HasComponents<T2>();
        }
        
        public bool HasComponents<T0, T1, T2, T3>() {
            return this.HasComponents<T0>() && this.HasComponents<T1>() && this.HasComponents<T2>() && this.HasComponents<T3>();
        }
        
        public bool HasComponents<T0, T1, T2, T3, T4>() {
            return this.HasComponents<T0>() && this.HasComponents<T1>() && this.HasComponents<T2>() && this.HasComponents<T3>()
                && this.HasComponents<T4>();
        }
        
        public bool HasComponents<T0, T1, T2, T3, T4, T5>() {
            return this.HasComponents<T0>() && this.HasComponents<T1>() && this.HasComponents<T2>() && this.HasComponents<T3>()
                && this.HasComponents<T4>() && this.HasComponents<T5>();
        }
        
        public bool HasSharedComponent(Type type) {
            return sharedComponentIndexer.TryIndex(type, out var index) && sharedComponents.ContainsKey(index);
        }

        public bool HasSharedComponent<T>()  where T : ISharedComponent<T> {
            return HasSharedComponent(typeof(T));
        }

        public bool HasSharedComponent<T>(T value) where T : ISharedComponent<T> {
            return sharedComponentIndexer.TryIndex(value, out var index) && sharedComponents.Contains(new KeyValuePair<int, SharedComponentIndex>(index.TypeIndex, index));
        }

        public bool TryGetSharedComponent<T>(out T value) where T : ISharedComponent<T> {
            if (sharedComponentIndexer.TryIndex<T>(out var typeIndex)
                && sharedComponents.TryGetValue(typeIndex, out var sharedComponentIndex)) {
                value = sharedComponentIndexer.Value<T>(sharedComponentIndex);
                return true;
            }
            value = default;
            return false;
        }

        public T GetSharedComponent<T>() where T : ISharedComponent<T> {
            if (this.TryGetSharedComponent<T>(out var value)) {
                return value;
            }
            throw new ArgumentException($"Archetype does not contain shared component {typeof(T).FullName}");
        }

        internal ComponentTypeLayout Layout() => new ComponentTypeLayout(this);

        static readonly string SharedComponentInterfaceName = typeof(ISharedComponent<>).Name;

        static bool IsSharedComponent(Type type) => type.GetInterface(SharedComponentInterfaceName) != null;

        public Archetype AddComponents(IEnumerable<Type> components) {
            var componentIndices = this.components;
            
            foreach (var component in components) {
                if (IsSharedComponent(component)) {
                    throw new ArgumentException($"Cannot add component. {component.FullName} is a shared component.");
                }
                
                var index = this.componentTypeIndexer[component];
                componentIndices = componentIndices.Add(index);
            }
                    
            return new Archetype(
                this.componentTypeIndexer,
                this.sharedComponentIndexer,
                componentIndices,
                this.sharedComponents);
        }

        public Archetype AddComponents(params Type[] components) {
            return AddComponents((IEnumerable<Type>)components);
        }

        public Archetype AddComponents<T0>() {
            return AddComponents(typeof(T0));
        }

        public Archetype AddComponents<T0, T1>() {
            return AddComponents(typeof(T0), typeof(T1));
        }

        public Archetype AddComponents<T0, T1, T2>() {
            return AddComponents(typeof(T0), typeof(T1), typeof(T2));
        }

        public Archetype AddComponents<T0, T1, T2, T3>() {
            return AddComponents(typeof(T0), typeof(T1), typeof(T2), typeof(T3));
        }

        public Archetype AddComponents<T0, T1, T2, T3, T4>() {
            return AddComponents(typeof(T0), typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }

        public Archetype AddComponents<T0, T1, T2, T3, T4, T5>() {
            return AddComponents(typeof(T0), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }

        public Archetype AddSharedComponent<T>(T value) where T : struct, ISharedComponent<T> {
            var index = sharedComponentIndexer.Index(value);
            return new Archetype(
                this.componentTypeIndexer,
                this.sharedComponentIndexer,
                this.components,
                this.sharedComponents.SetItem(index.TypeIndex, index));
        }

        public Archetype RemoveComponents(IEnumerable<Type> components) {
            var newComponents = this.components;
            foreach (var component in components) {
                var index = this.componentTypeIndexer[component];
                newComponents = newComponents.Remove(index);
            }

            return
                newComponents != this.components
                    ? new Archetype(
                        this.componentTypeIndexer,
                        this.sharedComponentIndexer,
                        newComponents,
                        this.sharedComponents
                    )
                    : this;
        }

        public Archetype RemoveComponents(params Type[] components) {
            return this.RemoveComponents((IEnumerable<Type>) components);
        }

        public Archetype RemoveComponents<T>() {
            return this.RemoveComponents(typeof(T));
        }

        public Archetype RemoveComponents<T0, T1>() {
            return this.RemoveComponents(typeof(T0), typeof(T1));
        }

        public Archetype RemoveComponents<T0, T1, T2>() {
            return this.RemoveComponents(
                typeof(T0),
                typeof(T1),
                typeof(T2)
            );
        }

        public Archetype RemoveComponents<T0, T1, T2, T3>() {
            return this.RemoveComponents(
                typeof(T0),
                typeof(T1),
                typeof(T2),
                typeof(T3)
            );
        }

        public Archetype RemoveComponents<T0, T1, T2, T3, T4>() {
            return this.RemoveComponents(
                typeof(T0),
                typeof(T1),
                typeof(T2),
                typeof(T3),
                typeof(T4)
            );
        }

        public Archetype RemoveComponents<T0, T1, T2, T3, T4, T5>() {
            return this.RemoveComponents(
                typeof(T0),
                typeof(T1),
                typeof(T2),
                typeof(T3),
                typeof(T4),
                typeof(T5)
            );
        }

        public Archetype RemoveSharedComponent<T>() where T : ISharedComponent<T> {
            if (this.sharedComponentIndexer.TryIndex<T>(out var index)) {
                var newSharedComponents = this.sharedComponents.Remove(index);
                if (newSharedComponents != this.sharedComponents) {
                    return new Archetype(this.componentTypeIndexer, this.sharedComponentIndexer, this.components, newSharedComponents);
                }
            }

            return this;
        }

        public override string ToString() {
            var builder = new StringBuilder();
            builder.Append("Archetype { ");
            foreach (var sharedComponent in SharedComponents) {
                builder.Append($"{sharedComponent.GetType().Name} = {sharedComponent}; ");
            }

            foreach (var component in Components) {
                builder.Append($"{component.Name}; ");
            }

            builder.Append('}');
            return builder.ToString();
        }
    }
}
