namespace Abp.Mapperly;

/// <summary>
/// Extends <see cref="IAbpMapper{TSource,TDestination}"/> with reverse-direction mapping.
/// Inherit from <see cref="ReverseMapperBase{TSource,TDestination}"/> for convenient default hook implementations.
/// </summary>
/// <typeparam name="TSource">Primary source type (forward direction).</typeparam>
/// <typeparam name="TDestination">Primary destination type (forward direction).</typeparam>
public interface IAbpReverseMapper<TSource, TDestination> : IAbpMapper<TSource, TDestination>
{
    /// <summary>Creates a new <typeparamref name="TSource"/> mapped from <paramref name="destination"/> (reverse direction).</summary>
    TSource ReverseMap(TDestination destination);

    /// <summary>Maps <paramref name="destination"/> onto the existing <paramref name="source"/> object (reverse direction).</summary>
    void ReverseMapTo(TDestination destination, TSource source);

    /// <summary>Called before <see cref="ReverseMap"/> or <see cref="ReverseMapTo"/>. Override to add pre-mapping logic.</summary>
    void BeforeReverseMap(TDestination destination);

    /// <summary>Called after <see cref="ReverseMap"/> or <see cref="ReverseMapTo"/>. Override to add post-mapping logic.</summary>
    void AfterReverseMap(TDestination destination, TSource source);
}
