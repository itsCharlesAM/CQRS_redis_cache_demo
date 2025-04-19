using Domain.Entities;
using MediatR;


namespace Application.Queries
{
    public record GetProductByIdQuery(int Id) : IRequest<Product?>;

}
