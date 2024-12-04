// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mediator;

namespace CleanAspire.Application.Pipeline;
public class ValidationPreProcessor<TRequest,TResponse> : MessagePreProcessor<TRequest, TResponse>
{
    private readonly IValidator<TRequest>? _validator;

    public ValidationPreProcessor(IValidator<TRequest>? validator = null)
    {
        _validator = validator;
    }

    public async ValueTask<TResponse> ProcessAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (_validator != null)
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResult = await _validator.ValidateAsync(context, cancellationToken);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }
    }
}
