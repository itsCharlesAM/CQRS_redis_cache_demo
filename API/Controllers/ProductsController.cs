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

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }




        [HttpGet("{id}")]
        public async Task<ActionResult<Product?>> Get(int id)
        {
            var result = await _mediator.Send(new GetProductByIdQuery(id));
            return result is not null ? Ok(result) : NotFound();
        }
    }
}
