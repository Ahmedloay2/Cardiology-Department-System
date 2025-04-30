using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Reflection; // Correct namespace for GetCustomAttribute

namespace Caridology_Department_System
{
    public class SwaggerDefaultValuesFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties == null) return;

            foreach (var property in context.Type.GetProperties())
            {
                // Use the extension method from System.Reflection
                var defaultValue = property.GetCustomAttribute<DefaultValueAttribute>();
                if (defaultValue?.Value != null)
                {
                    var propertyName = char.ToLowerInvariant(property.Name[0]) + property.Name[1..];
                    if (schema.Properties.TryGetValue(propertyName, out var schemaProperty))
                    {
                        schemaProperty.Example = new OpenApiString(defaultValue.Value.ToString());
                    }
                }
            }
        }
    }
}
