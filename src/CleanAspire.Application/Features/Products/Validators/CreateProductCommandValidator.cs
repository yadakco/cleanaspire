// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanAspire.Application.Features.Products.Commands;


namespace CleanAspire.Application.Features.Products.Validators;
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.SKU)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.");

        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");

        RuleFor(command => command.Category)
            .NotNull().WithMessage("Product category is required.");

        RuleFor(command => command.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(command => command.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Must(BeAValidCurrency).WithMessage("Invalid currency code.");

        RuleFor(command => command.UOM)
            .NotEmpty().WithMessage("Unit of Measure (UOM) is required.")
            .MaximumLength(10).WithMessage("UOM must not exceed 10 characters.");
    }

    private bool BeAValidCurrency(string currency)
    {
        // Here you can implement logic to validate currency codes, for example, using a predefined list of valid codes.
        var validCurrencies = new[] { "USD", "EUR", "GBP", "CNY" }; // Example valid currency codes.
        return validCurrencies.Contains(currency);
    }
}
