using Abp.Configuration.Startup;

namespace Abp.Mapperly;

/// <summary>
/// Defines extension methods to <see cref="IModuleConfigurations"/> to allow to configure Abp.Mapperly module.
/// </summary>
public static class AbpMapperlyConfigurationExtensions
{
    /// <summary>
    /// Used to configure Abp.Mapperly module.
    /// </summary>
    public static IAbpMapperlyConfiguration AbpMapperly(this IModuleConfigurations configurations)
    {
        return configurations.AbpConfiguration.Get<IAbpMapperlyConfiguration>();
    }
}
