using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
	partial class EqualityComparer<T>
	{
		static volatile EqualityComparer<T> defaultComparer;

		public static EqualityComparer<T> Default {
			[MethodImplAttribute (MethodImplOptions.AggressiveInlining)]
			get {
				EqualityComparer<T> comparer = defaultComparer;
				if (comparer == null) {
					comparer = CreateComparer();
					defaultComparer = comparer;
				}
				return comparer;
			}
		}

		static EqualityComparer<T> CreateComparer ()
		{
			RuntimeType t = (RuntimeType)typeof(T);
			if (t == typeof(byte)) {
				return (EqualityComparer<T>)(object)(new ByteEqualityComparer());
			}

			/////////////////////////////////////////////////
			// KEEP THIS IN SYNC WITH THE DEVIRT CODE
			// IN METHOD-TO-IR.C
			/////////////////////////////////////////////////

			if (typeof(IEquatable<T>).IsAssignableFrom(t)) {
				return (EqualityComparer<T>)RuntimeType.CreateInstanceForAnotherGenericParameter (typeof(GenericEqualityComparer<>), t);
			}

			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)) {
				RuntimeType u = (RuntimeType)t.GetGenericArguments()[0];
				if (typeof(IEquatable<>).MakeGenericType(u).IsAssignableFrom(u)) {
					return (EqualityComparer<T>)RuntimeType.CreateInstanceForAnotherGenericParameter (typeof(NullableEqualityComparer<>), u);
				}
			}

			if (t.IsEnum) {
				return (EqualityComparer<T>)RuntimeType.CreateInstanceForAnotherGenericParameter (typeof(EnumEqualityComparer<>), t);
			}
			
			return new ObjectEqualityComparer<T>();
		}
	}

	partial class EnumEqualityComparer<T>
	{
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public override bool Equals (T x, T y) => JitHelpers.UnsafeEnumCast (x) == JitHelpers.UnsafeEnumCast (y);
	}
}