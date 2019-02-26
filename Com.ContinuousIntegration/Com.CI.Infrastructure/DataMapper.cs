using System;
using System.Linq;
using System.Reflection;

namespace Com.CI.Infrastructure
{
    public class DataMapper : IDataMapper
    {
        public Target Map<Target, Source>(Source source)
        {
            if (source == null)
            {
                return default(Target);
            }

            var sourceProperties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var target = Activator.CreateInstance<Target>();

            var targetProperties = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var targetProperty in targetProperties)
            {
                var sourceProperty = sourceProperties.SingleOrDefault(sp => sp.Name.Equals(targetProperty.Name,
                    StringComparison.InvariantCultureIgnoreCase));

                if (sourceProperty == null)
                {
                    continue;
                }

                var sourceValue = sourceProperty.GetValue(source);

                if (sourceValue == null)
                {
                    continue;
                }

                targetProperty.SetValue(target, sourceValue);
            }

            return target;
        }
    }
}
