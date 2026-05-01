namespace Abp.Mapperly;

/// <summary>
/// Defines the contract for a Mapperly-based object mapper between two types.
/// Implement this interface on your <c>[Mapper] partial class</c>, or inherit from
/// <see cref="MapperBase{TSource,TDestination}"/> which provides empty default hook implementations.
/// </summary>
/// <typeparam name="TSource">Source type</typeparam>
/// <typeparam name="TDestination">Destination type</typeparam>
public interface IAbpMapper<TSource, TDestination>
{
    /// <summary>Creates a new <typeparamref name="TDestination"/> mapped from <paramref name="source"/>.</summary>
    TDestination Map(TSource source);

    /// <summary>Maps <paramref name="source"/> onto the existing <paramref name="destination"/> object and returns it.</summary>
    TDestination MapTo(TSource source, TDestination destination);

    /// <summary>Called before <see cref="Map"/> or <see cref="MapTo"/>. Override to add pre-mapping logic.</summary>
    void BeforeMap(TSource source);

    /// <summary>Called after <see cref="Map"/> or <see cref="MapTo"/>. Override to add post-mapping logic.</summary>
    void AfterMap(TSource source, TDestination destination);
}
