using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Abp.Dependency;
using IObjectMapper = Abp.ObjectMapping.IObjectMapper;

namespace Abp.Mapperly;

public class MapperlyObjectMapper : IObjectMapper
{
    // Compiled delegate caches — keyed by "src.FullName=>dest.FullName"
    private static readonly ConcurrentDictionary<string, Func<object, object, object>> MapPipelineCache = new();
    private static readonly ConcurrentDictionary<string, Func<object, object, object>> ReverseMapPipelineCache = new();
    private static readonly ConcurrentDictionary<string, Func<MapperlyObjectMapper, object, object>> CollectionElementDelegateCache = new();

    private readonly IIocResolver _iocResolver;

    public MapperlyObjectMapper(IIocResolver iocResolver)
    {
        _iocResolver = iocResolver;
    }

    /// <inheritdoc/>
    public TDestination Map<TDestination>(object source)
    {
        if (source == null)
        {
            return default;
        }

        var sourceType = source.GetType();
        var destType = typeof(TDestination);

        // --- Collection shortcut ---
        if (TryMapCollection(source, sourceType, destType, out TDestination collectionResult))
        {
            return collectionResult;
        }

        // --- Direct mapper: IAbpMapper<TSource, TDest> ---
        var mapperType = typeof(IAbpMapper<,>).MakeGenericType(sourceType, destType);
        if (_iocResolver.IsRegistered(mapperType))
        {
            var mapper = _iocResolver.Resolve(mapperType);
            try
            {
                var pipeline = MapPipelineCache.GetOrAdd(
                    $"{sourceType.FullName}=>{destType.FullName}",
                    _ => BuildMapPipeline(mapperType, sourceType, destType));
                return (TDestination)pipeline(mapper, source)!;
            }
            finally
            {
                _iocResolver.Release(mapper);
            }
        }

        // --- Reverse-mapper fallback: IAbpReverseMapper<TDest, TSource> ---
        var reverseMapperType = typeof(IAbpReverseMapper<,>).MakeGenericType(destType, sourceType);
        if (_iocResolver.IsRegistered(reverseMapperType))
        {
            var reverseMapper = _iocResolver.Resolve(reverseMapperType);
            try
            {
                var pipeline = ReverseMapPipelineCache.GetOrAdd(
                    $"rev:{destType.FullName}=>{sourceType.FullName}",
                    _ => BuildReverseMapPipeline(reverseMapperType, destType, sourceType));
                return (TDestination)pipeline(reverseMapper, source)!;
            }
            finally
            {
                _iocResolver.Release(reverseMapper);
            }
        }

        throw new AbpException(BuildNoMapperFoundMessage(sourceType, destType));
    }

    /// <inheritdoc/>
    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        // --- Direct mapper ---
        if (_iocResolver.IsRegistered<IAbpMapper<TSource, TDestination>>())
        {
            var mapper = _iocResolver.Resolve<IAbpMapper<TSource, TDestination>>();
            try
            {
                mapper.BeforeMap(source);
                mapper.MapTo(source, destination);
                mapper.AfterMap(source, destination);
                return destination;
            }
            finally
            {
                _iocResolver.Release(mapper);
            }
        }

        // --- Reverse-mapper fallback: IAbpReverseMapper<TDest, TSource> ---
        // The reverse mapper's forward direction is TDestination→TSource; its reverse direction is TSource→TDestination.
        // BeforeReverseMap / ReverseMapTo / AfterReverseMap all expect (TDestination arg, TSource arg)
        // which maps to (source, destination) in this call's naming convention.
        if (_iocResolver.IsRegistered<IAbpReverseMapper<TDestination, TSource>>())
        {
            var reverseMapper = _iocResolver.Resolve<IAbpReverseMapper<TDestination, TSource>>();
            try
            {
                reverseMapper.BeforeReverseMap(source);
                reverseMapper.ReverseMapTo(source, destination);
                reverseMapper.AfterReverseMap(source, destination);
                return destination;
            }
            finally
            {
                _iocResolver.Release(reverseMapper);
            }
        }

        throw new AbpException(BuildNoMapperFoundMessage(typeof(TSource), typeof(TDestination)));
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Mapperly does not support expression-tree projection.
    /// Use <see cref="IAbpMapperlyQueryableMapper{TSource,TDestination}"/> instead.</exception>
    public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source)
    {
        throw new NotSupportedException(
            "ProjectTo is not supported by Mapperly. " +
            "Inject IAbpMapperlyQueryableMapper<TSource, TDestination> and call ProjectTo directly.");
    }

    // -------------------------------------------------------------------------
    // Collection mapping
    // -------------------------------------------------------------------------

    private bool TryMapCollection<TDestination>(object source, Type sourceType, Type destType, out TDestination result)
    {
        result = default;

        if (!TryGetCollectionInfo(sourceType, out var srcElem, out _)) return false;
        if (!TryGetCollectionInfo(destType, out var destElem, out var destIsArray)) return false;

        // Only proceed if an element mapper is registered (direct or reverse).
        var elemMapperType = typeof(IAbpMapper<,>).MakeGenericType(srcElem, destElem);
        var elemRevMapperType = typeof(IAbpReverseMapper<,>).MakeGenericType(destElem, srcElem);
        if (!_iocResolver.IsRegistered(elemMapperType) && !_iocResolver.IsRegistered(elemRevMapperType))
        {
            return false;
        }

        var mapElement = CollectionElementDelegateCache.GetOrAdd(
            $"{srcElem.FullName}=>{destElem.FullName}",
            _ => BuildCollectionElementDelegate(destElem));

        var mapped = new List<object>();
        foreach (var item in (IEnumerable)source)
        {
            mapped.Add(mapElement(this, item));
        }

        if (destIsArray)
        {
            var arr = Array.CreateInstance(destElem, mapped.Count);
            for (var i = 0; i < mapped.Count; i++)
            {
                arr.SetValue(mapped[i], i);
            }
            result = (TDestination)(object)arr;
        }
        else
        {
            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(destElem))!;
            foreach (var item in mapped)
            {
                list.Add(item);
            }
            result = (TDestination)(object)list;
        }

        return true;
    }

    private static bool TryGetCollectionInfo(Type type, out Type elementType, out bool isArray)
    {
        isArray = false;
        elementType = null!;

        if (type.IsArray)
        {
            elementType = type.GetElementType()!;
            isArray = true;
            return elementType != null;
        }

        if (type.IsGenericType)
        {
            var def = type.GetGenericTypeDefinition();
            if (def == typeof(IEnumerable<>) || def == typeof(IList<>) ||
                def == typeof(List<>) || def == typeof(ICollection<>) ||
                def == typeof(IReadOnlyList<>) || def == typeof(IReadOnlyCollection<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }
        }

        foreach (var iface in type.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                elementType = iface.GetGenericArguments()[0];
                return true;
            }
        }

        return false;
    }

    // -------------------------------------------------------------------------
    // Expression-tree pipeline builders
    // -------------------------------------------------------------------------

    /// <summary>
    /// Compiles a delegate:
    /// <c>(mapper, source) => { var m = (IAbpMapper&lt;TSrc,TDest&gt;)mapper; m.BeforeMap((TSrc)source); var d = m.Map((TSrc)source); m.AfterMap((TSrc)source, d); return (object)d; }</c>
    /// </summary>
    private static Func<object, object, object> BuildMapPipeline(Type mapperType, Type sourceType, Type destType)
    {
        var mapperParam = Expression.Parameter(typeof(object), "mapper");
        var sourceParam = Expression.Parameter(typeof(object), "source");

        var typedMapperVar = Expression.Variable(mapperType, "m");
        var typedSourceVar = Expression.Variable(sourceType, "s");
        var destVar = Expression.Variable(destType, "d");

        var beforeMapMethod = mapperType.GetMethod(nameof(IAbpMapper<object, object>.BeforeMap))!;
        var mapMethod = mapperType.GetMethod(nameof(IAbpMapper<object, object>.Map), [sourceType])!;
        var afterMapMethod = mapperType.GetMethod(nameof(IAbpMapper<object, object>.AfterMap))!;

        var block = Expression.Block(
            [typedMapperVar, typedSourceVar, destVar],
            Expression.Assign(typedMapperVar, Expression.Convert(mapperParam, mapperType)),
            Expression.Assign(typedSourceVar, Expression.Convert(sourceParam, sourceType)),
            Expression.Call(typedMapperVar, beforeMapMethod, typedSourceVar),
            Expression.Assign(destVar, Expression.Call(typedMapperVar, mapMethod, typedSourceVar)),
            Expression.Call(typedMapperVar, afterMapMethod, typedSourceVar, destVar),
            Expression.Convert(destVar, typeof(object))
        );

        return Expression.Lambda<Func<object, object, object>>(block, mapperParam, sourceParam).Compile();
    }

    /// <summary>
    /// Compiles a delegate that calls BeforeReverseMap → ReverseMap → AfterReverseMap on
    /// <c>IAbpReverseMapper&lt;mapperSrcType, mapperDestType&gt;</c> where
    /// <paramref name="mapperSrcType"/> is the caller's TDestination and
    /// <paramref name="mapperDestType"/> is the caller's TSource.
    /// </summary>
    private static Func<object, object, object> BuildReverseMapPipeline(
        Type reverseMapperType, Type mapperSrcType, Type mapperDestType)
    {
        var mapperParam = Expression.Parameter(typeof(object), "mapper");
        var sourceParam = Expression.Parameter(typeof(object), "source"); // caller's TSource = mapper's TDest

        var typedMapperVar = Expression.Variable(reverseMapperType, "m");
        var typedSourceVar = Expression.Variable(mapperDestType, "s");
        var resultVar = Expression.Variable(mapperSrcType, "r");

        var beforeReverseMap = reverseMapperType.GetMethod(nameof(IAbpReverseMapper<object, object>.BeforeReverseMap))!;
        var reverseMap = reverseMapperType.GetMethod(nameof(IAbpReverseMapper<object, object>.ReverseMap), [mapperDestType])!;
        var afterReverseMap = reverseMapperType.GetMethod(nameof(IAbpReverseMapper<object, object>.AfterReverseMap))!;

        var block = Expression.Block(
            [typedMapperVar, typedSourceVar, resultVar],
            Expression.Assign(typedMapperVar, Expression.Convert(mapperParam, reverseMapperType)),
            Expression.Assign(typedSourceVar, Expression.Convert(sourceParam, mapperDestType)),
            Expression.Call(typedMapperVar, beforeReverseMap, typedSourceVar),
            Expression.Assign(resultVar, Expression.Call(typedMapperVar, reverseMap, typedSourceVar)),
            Expression.Call(typedMapperVar, afterReverseMap, typedSourceVar, resultVar),
            Expression.Convert(resultVar, typeof(object))
        );

        return Expression.Lambda<Func<object, object, object>>(block, mapperParam, sourceParam).Compile();
    }

    /// <summary>
    /// Compiles a delegate that calls <c>Map&lt;<paramref name="destElem"/>&gt;(element)</c>
    /// on this mapper instance. Used to map individual elements inside a collection.
    /// </summary>
    private static Func<MapperlyObjectMapper, object, object> BuildCollectionElementDelegate(Type destElem)
    {
        var mapMethod = typeof(MapperlyObjectMapper)
            .GetMethod(nameof(Map), [typeof(object)])!
            .MakeGenericMethod(destElem);

        var selfParam = Expression.Parameter(typeof(MapperlyObjectMapper), "self");
        var elemParam = Expression.Parameter(typeof(object), "element");

        return Expression.Lambda<Func<MapperlyObjectMapper, object, object>>(
            Expression.Convert(Expression.Call(selfParam, mapMethod, elemParam), typeof(object)),
            selfParam, elemParam
        ).Compile();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string BuildNoMapperFoundMessage(Type sourceType, Type destType)
    {
        return
            $"No Mapperly mapper was registered for {sourceType.FullName} -> {destType.FullName}. " +
            $"Create a [Mapper] partial class that inherits MapperBase<{sourceType.Name}, {destType.Name}> " +
            $"(one-way) or ReverseMapperBase<{sourceType.Name}, {destType.Name}> (two-way) " +
            "and ensure the module containing it is loaded.";
    }
}

