using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands
{
    public class CreateProductCommand : IRequest<Product>
    {
        public string? Name { get; set; }
        public int Price { get; set; }
    }
}
