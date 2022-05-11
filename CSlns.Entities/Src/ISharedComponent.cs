namespace CSlns.Entities {
    public interface ISharedComponent<T> : IEquatable<T> where T : ISharedComponent<T> {
    }
}
