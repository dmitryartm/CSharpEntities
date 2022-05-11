using System.Runtime.Serialization;


namespace CSlns.Entities {
    public static class SerializationInfoEx {
        public static T GetValue<T>(this SerializationInfo info, string name) {
            return (T) info.GetValue(name, typeof(T));
        }
    }
}