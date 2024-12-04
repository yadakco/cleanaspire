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
public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(command => command.Ids)
            .NotEmpty().WithMessage("At least one product ID is required.")
            .Must(ids => ids != null && ids.All(id => !string.IsNullOrWhiteSpace(id))).WithMessage("Product IDs must not be empty or whitespace.");
    }
}
