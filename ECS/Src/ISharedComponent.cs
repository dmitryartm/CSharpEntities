using System;

namespace ECS {
    public interface ISharedComponent<T> : IEquatable<T> where T : ISharedComponent<T> {
    }
}
