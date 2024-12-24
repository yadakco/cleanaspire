// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Models;
using CleanAspire.Application.Features.Products.Commands;
using CleanAspire.Application.Features.Products.DTOs;
using CleanAspire.Application.Features.Products.Queries;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using static CleanAspire.Api.Endpoints.FileUploadEndpointRegistrar;

namespace CleanAspire.Api.Endpoints;

public class ProductEndpointRegistrar(ILogger<ProductEndpointRegistrar> logger) : IEndpointRegistrar
{
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/products").WithTags("products").RequireAuthorization();

        // Get all products
        group.MapGet("/", async ([FromServices] IMediator mediator) =>
        {
            var query = new GetAllProductsQuery();
            return await mediator.Send(query);
        })
        .Produces<IEnumerable<ProductDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get all products")
        .WithDescription("Returns a list of all products in the system.");

        // Get product by ID
        group.MapGet("/{id}", (IMediator mediator, [FromRoute] string id) => mediator.Send(new GetProductByIdQuery(id)))
        .Produces<ProductDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get product by ID")
        .WithDescription("Returns the details of a specific product by its unique ID.");

        // Create a new product
        group.MapPost("/", ([FromServices] IMediator mediator, [FromBody] CreateProductCommand command) => mediator.Send(command))
             .Produces<ProductDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Create a new product")
        .WithDescription("Creates a new product with the provided details.");

        // Update an existing product
        group.MapPut("/", ([FromServices] IMediator mediator, [FromBody] UpdateProductCommand command) => mediator.Send(command))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Update an existing product")
        .WithDescription("Updates the details of an existing product.");

        // Delete products by IDs
        group.MapDelete("/", ([FromServices] IMediator mediator, [FromBody] DeleteProductCommand command) => mediator.Send(command))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Delete products by IDs")
        .WithDescription("Deletes one or more products by their unique IDs.");

        // Get products with pagination and filtering
        group.MapPost("/pagination", ([FromServices] IMediator mediator, [FromBody] ProductsWithPaginationQuery query) => mediator.Send(query))
        .Produces<PaginatedResult<ProductDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Get products with pagination")
        .WithDescription("Returns a paginated list of products based on search keywords, page size, and sorting options.");

        // Export products to CSV
        group.MapGet("/export", async ([FromQuery] string keywords, [FromServices] IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new ExportProductsQuery(keywords));
                result.Position = 0;
                return Results.File(result, "text/csv", "exported-products.csv");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error exporting products: {ex.Message}");
                return Results.Problem("An error occurred while exporting products. Please try again later.");
            }
        })
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Export Products to CSV")
        .WithDescription("Exports the product data to a CSV file based on the provided keywords. The CSV file includes product details such as ID, name, description, price, SKU, and category.");

        // Import products from CSV
        group.MapPost("/import", async ([FromForm] FileUploadRequest request, HttpContext context, [FromServices] IMediator mediator) =>
        {
            try
            {
                var response = new List<FileUploadResponse>();
                foreach (var file in request.Files)
                {
                    // Validate file type
                    if (Path.GetExtension(file.FileName).ToLower() != ".csv")
                    {
                        logger.LogWarning($"Invalid file type: {file.FileName}");
                        return Results.BadRequest("Only CSV files are supported.");
                    }
                    var fileName = file.FileName;
                    // Copy file to memory stream
                    var filestream = file.OpenReadStream();
                    var stream = new MemoryStream();
                    await filestream.CopyToAsync(stream);
                    stream.Position = 0;
                    var fileSize = stream.Length;
                    // Send the file stream to ImportProductsCommand
                    var importCommand = new ImportProductsCommand(stream);
                    await mediator.Send(importCommand);

                    response.Add(new FileUploadResponse
                    {
                        Path = file.FileName,
                        Url = $"Imported {fileName}",
                        Size = fileSize
                    });
                }

                return TypedResults.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error importing products: {ex.Message}");
                return Results.Problem("An error occurred while importing products. Please try again later.");
            }
        }).DisableAntiforgery()
          .Accepts<FileUploadRequest>("multipart/form-data")
          .Produces<IEnumerable<FileUploadResponse>>()
          .ProducesProblem(StatusCodes.Status400BadRequest)
          .ProducesProblem(StatusCodes.Status500InternalServerError)
          .WithMetadata(new ConsumesAttribute("multipart/form-data"))
          .WithSummary("Import Products from CSV")
          .WithDescription("Imports product data from one or more CSV files. The CSV files should contain product details in the required format.");

    }


}

