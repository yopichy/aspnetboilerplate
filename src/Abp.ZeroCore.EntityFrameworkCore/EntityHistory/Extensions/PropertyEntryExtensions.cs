using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Abp.EntityHistory.Extensions;

internal static class PropertyEntryExtensions
{
    internal static object GetNewValue(this PropertyEntry propertyEntry)
    {
        // A hard-deleted entity is in the Deleted state before SaveChanges and
        // transitions to Detached afterwards. In both cases there is no new value.
        // Treating Detached the same as Deleted keeps the new value null when
        // UpdateChangeSet re-reads it after SaveChanges, so the deletion is not
        // filtered out as "unchanged". Soft deletes stay Modified/Unchanged and
        // are unaffected.
        if (propertyEntry.EntityEntry.State == EntityState.Deleted ||
            propertyEntry.EntityEntry.State == EntityState.Detached)
        {
            return null;
        }

        return propertyEntry.CurrentValue;
    }

    internal static object GetOriginalValue(this PropertyEntry propertyEntry)
    {
        if (propertyEntry.EntityEntry.State == EntityState.Added)
        {
            return null;
        }

        return propertyEntry.OriginalValue;
    }
}