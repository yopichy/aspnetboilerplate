namespace Abp.Mapperly;

/// <summary>
/// Defines the contract for a Mapperly-based object mapper between two types.
/// Implement this interface on your <c>[Mapper] partial class</c> to integrate with ABP's <see cref="Abp.ObjectMapping.IObjectMapper"/>.
/// </summary>
/// <typeparam name="TSource">Source type</typeparam>
/// <typeparam name="TDestination">Destination type</typeparam>
public interface IAbpMapper<TSource, TDestination>
{
    /// <summary>Creates a new <typeparamref name="TDestination"/> mapped from <paramref name="source"/>.</summary>
    TDestination Map(TSource source);

    /// <summary>Maps <paramref name="source"/> onto the existing <paramref name="destination"/> object and returns it.</summary>
    TDestination MapTo(TSource source, TDestination destination);
}
