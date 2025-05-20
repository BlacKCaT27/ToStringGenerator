// ReSharper disable NotDisposedResourceIsReturned

using System.Collections;
using System.Collections.Immutable;

namespace Bcss.ToStringGenerator;

internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    private readonly T[]? _array;
    
    /// <param name="array">The input <see cref="ImmutableArray"/> to wrap.</param>
    public EquatableArray(T[] array)
    {
        _array = array;
    }

    /// <inheritdoc/>
    public bool Equals(EquatableArray<T> array)
    {
        if (_array is null)
        {
            return array._array is null;
        }
        if (array._array is null)
        {
            return false;
        }
        return AsSpan().SequenceEqual(array.AsSpan());
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> array && this.Equals(array);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (_array is not { } array)
        {
            return 0;
        }

        unchecked
        {
            int hash = 17;
            foreach (T item in array)
            {
                hash = hash * 31 + item.GetHashCode();
            }
            return hash;
        }
    }

    /// <summary>
    /// Returns a <see cref="ReadOnlySpan{T}"/> wrapping the current items.
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> wrapping the current items.</returns>
    private ReadOnlySpan<T> AsSpan()
    {
        return _array is not null ? _array.AsSpan() : ReadOnlySpan<T>.Empty;
    }

    /// <inheritdoc/>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return ((IEnumerable<T>)(_array ?? [])).GetEnumerator();
    }    
    
    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<T>)(_array ?? [])).GetEnumerator();
    }

    public int Count => _array?.Length ?? 0;
    
    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right)
    {
        return !left.Equals(right);
    }
}