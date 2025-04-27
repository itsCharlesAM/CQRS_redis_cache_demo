using Application.Commands;
using Application.Interfaces;
using Application.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IProductRepository _productRepository;


        public ProductsController(IMediator mediator, IProductRepository productRepository)
        {
            _mediator = mediator;
            _productRepository = productRepository;
        }


        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(CreateProductCommand command)
        {
            var product = await _mediator.Send(command);
            return Ok(product);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Product?>> Get(int id)
        {
            var result = await _mediator.Send(new GetProductByIdQuery(id));
            return result is not null ? Ok(result) : NotFound();
        }


        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetAllProducts()
        {
            var products = await _mediator.Send(new GetAllProductsQuery());
            return Ok(products);
        }
    }
}
