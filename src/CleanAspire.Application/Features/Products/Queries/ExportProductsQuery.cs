using System.Globalization;
using CleanAspire.Application.Features.Products.DTOs;
using CsvHelper;

namespace CleanAspire.Application.Features.Products.Queries;
public record ExportProductsQuery(string Keywords) : IRequest<Stream>;


public class ExportProductsQueryHandler : IRequestHandler<ExportProductsQuery, Stream>
{
    private readonly IApplicationDbContext _context;

    public ExportProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Stream> Handle(ExportProductsQuery request, CancellationToken cancellationToken)
    {
        var data = await _context.Products.Where(x => x.SKU.Contains(request.Keywords) || x.Name.Contains(request.Keywords) || x.Description.Contains(request.Keywords))
                        .Select(t => new ProductDto
                        {
                            Id = t.Id,
                            Name = t.Name,
                            Description = t.Description,
                            Price = t.Price,
                            SKU = t.SKU,
                            UOM = t.UOM,
                            Currency = t.Currency,
                            Category = (ProductCategoryDto?)t.Category
                        }).ToListAsync(cancellationToken);
        var steam = new MemoryStream();
        using (var writer = new StreamWriter(steam, leaveOpen: true))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(data);
            await writer.FlushAsync();
        }
        return steam;
    }
}
