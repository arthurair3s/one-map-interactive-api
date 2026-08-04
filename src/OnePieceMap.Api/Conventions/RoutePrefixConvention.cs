using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OnePieceMap.Api.Conventions;

// Prepends a fixed route prefix (e.g. "api/v1") to every controller,
// so controllers declare only [Route("[controller]")] and stay version-agnostic.
public class RoutePrefixConvention(string prefix) : IApplicationModelConvention
{
    private readonly AttributeRouteModel _prefix = new(new RouteAttribute(prefix));

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            var routedSelectors = controller.Selectors.Where(s => s.AttributeRouteModel is not null).ToList();

            if (routedSelectors.Count > 0)
            {
                foreach (var selector in routedSelectors)
                {
                    selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(_prefix, selector.AttributeRouteModel);
                }
            }
            else
            {
                controller.Selectors.Add(new SelectorModel { AttributeRouteModel = _prefix });
            }
        }
    }
}
