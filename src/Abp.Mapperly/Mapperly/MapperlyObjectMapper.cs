using System;
using System.Linq;
using System.Reflection;
using Abp.Dependency;
using IObjectMapper = Abp.ObjectMapping.IObjectMapper;

namespace Abp.Mapperly;

public class MapperlyObjectMapper : IObjectMapper
{
    private readonly IIocResolver _iocResolver;

    public MapperlyObjectMapper(IIocResolver iocResolver)
    {
        _iocResolver = iocResolver;
    }

    public TDestination Map<TDestination>(object source)
    {
        if (source == null)
        {
            return default;
        }

        var mapperType = typeof(IAbpMapper<,>).MakeGenericType(source.GetType(), typeof(TDestination));
        var mapper = _iocResolver.Resolve(mapperType);
        try
        {
            var mapMethod = mapperType.GetMethod(nameof(IAbpMapper<object, TDestination>.Map), new[] { source.GetType() });
            return (TDestination)mapMethod!.Invoke(mapper, new[] { source });
        }
        finally
        {
            _iocResolver.Release(mapper);
        }
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        var mapper = _iocResolver.Resolve<IAbpMapper<TSource, TDestination>>();
        try
        {
            return mapper.MapTo(source, destination);
        }
        finally
        {
            _iocResolver.Release(mapper);
        }
    }

    public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source)
    {
        throw new NotSupportedException(
            "ProjectTo is not supported by Mapperly. " +
            "Implement IAbpMapperlyQueryableMapper<TSource, TDestination> and inject it directly."
        );
    }
}
