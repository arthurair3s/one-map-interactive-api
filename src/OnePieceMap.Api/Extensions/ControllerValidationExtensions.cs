using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace OnePieceMap.Api.Extensions;

public static class ControllerValidationExtensions
{
    // Runs a FluentValidation validator and returns a ProblemDetails-shaped 400 when invalid, or null when valid.
    public static async Task<ActionResult?> ValidateAsync<T>(this ControllerBase controller, IValidator<T> validator, T instance)
    {
        var result = await validator.ValidateAsync(instance);
        if (result.IsValid)
        {
            return null;
        }

        foreach (var error in result.Errors)
        {
            controller.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        return controller.ValidationProblem(controller.ModelState);
    }

    // A missing non-nullable `int` query parameter silently binds to 0 instead of failing
    // model validation (unlike missing body-bound complex types), so required query params
    // must be declared nullable and checked explicitly to get a proper 400 (e.g. RN08).
    public static ActionResult? RequireQueryParam(this ControllerBase controller, int? value, string paramName)
    {
        if (value is not null)
        {
            return null;
        }

        controller.ModelState.AddModelError(paramName, $"The '{paramName}' query parameter is required.");
        return controller.ValidationProblem(controller.ModelState);
    }
}
