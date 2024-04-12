using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TelemarketingControlSystem.Helper
{
	public class SwaggerTenantParam : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{


			//if (operation.Parameters == null)
			//    operation.Parameters = new List<OpenApiParameter>();

			//operation.Parameters.Add(new OpenApiParameter
			//{
			//    Name = "Tenant",
			//    In = ParameterLocation.Header,
			//    Description = "Tenant Name",
			//    Required = false,
			//    Schema = new OpenApiSchema
			//    {
			//        Type = "string"
			//    }
			//});

		}
	}
}
