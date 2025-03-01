using APICatalog.DTOs;
using APICatalog.Models;
using APICatalog.Pagination;
using APICatalog.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;

namespace APICatalog.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProductController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    [Authorize(Policy = "UserOnly")]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> Get()
    {
        var products = await _unitOfWork.ProductRepository.GetAllAsync();

        if (products is null)
        {
            return NotFound("Produtos não encontrados");
        }

        return Ok(products);
    }

    [HttpGet("pagination")]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProduts([FromQuery] ProductParameters productsParameters)
    {
        var products = await _unitOfWork.ProductRepository.GetProductsAsync(productsParameters);

        return LoadProduct(products);
    }

    [HttpGet("pagination-filter")]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProdutsPaginationFilter([FromQuery] ProductsFilterPrice productsFilterPrice)
    {
        var products = await _unitOfWork.ProductRepository.GetProductsFilterPriceAsync(productsFilterPrice);

        return LoadProduct(products);
    }

    [HttpGet("{id:int}", Name = "GetProduct")]
    public async Task<ActionResult<ProductDTO>> GetById([FromRoute] int id)
    {
        var product = await _unitOfWork.ProductRepository.GetAsync(x => x.ProductId == id);

        if (product is null)
        {
            return NotFound("Produto não encontrado");
        }

        return Ok(product);
    }

    [HttpGet("get-by-category")]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetByCategoryId([FromRoute] int categoryId)
    {
        var products = await _unitOfWork.ProductRepository.GetByCategoryAsync(categoryId);

        if (products is null)
        {
            return NotFound("Produtos não encontrados");
        }

        return Ok(products);
    }

    [HttpPost()]
    public async Task<ActionResult<ProductDTO>> Post([FromBody] ProductDTO productReq)
    {
        if (productReq is null)
        {
            return BadRequest("Produto invalido");
        }

        Product product = _mapper.Map<Product>(productReq);

        var resultProduct = _unitOfWork.ProductRepository.Create(product);
        await _unitOfWork.CommitAsync();

        return new CreatedAtRouteResult("GetProduct", new { id = resultProduct.ProductId }, resultProduct);
    }

    [HttpPatch("{id}/update-partial")]
    public async Task<ActionResult<ProductDTOUpdateResponse>> Patch([FromRoute] int id, JsonPatchDocument<ProductDTOUpdateRequest> patchProductDTO)
    {
        if (patchProductDTO is null || id <= 0)
        {
            return BadRequest();
        }

        var product = await _unitOfWork.ProductRepository.GetAsync(x => x.ProductId == id);
        if (product is null)
        {
            return NotFound("Produto não encontrado");
        }

        var productUpdateRequest = _mapper.Map<ProductDTOUpdateRequest>(product);
        patchProductDTO.ApplyTo(productUpdateRequest, ModelState);

        if (!ModelState.IsValid || TryValidateModel(productUpdateRequest))
        {
            return BadRequest(ModelState);
        }

        _mapper.Map(productUpdateRequest, product);
        _unitOfWork.ProductRepository.Update(product);
        await _unitOfWork.CommitAsync();

        return Ok(_mapper.Map<ProductDTOUpdateResponse>(product));
    }


    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDTO>> Put([FromRoute] int id, ProductDTO productReq)
    {
        if (productReq.ProductId != id)
        {
            return BadRequest();
        }

        Product product = _mapper.Map<Product>(productReq);

        _unitOfWork.ProductRepository.Update(product);
        await _unitOfWork.CommitAsync();

        return Ok(productReq);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ProductDTO>> Delete([FromRoute] int id)
    {
        var product = await _unitOfWork.ProductRepository.GetAsync(x => x.ProductId == id);

        if (product is null)
        {
            return NotFound("Produto não encontrado");
        }

        _unitOfWork.ProductRepository.Delete(product);
        await _unitOfWork.CommitAsync();

        return Ok(product);
    }

    private ActionResult<IEnumerable<ProductDTO>> LoadProduct(IPagedList<Product>? products)
    {
        var metadata = new
        {
            products.Count,
            products.PageSize,
            products.PageCount,
            products.TotalItemCount,
            products.HasNextPage,
            products.HasPreviousPage
        };

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        IEnumerable<ProductDTO> productsDto = _mapper.Map<IEnumerable<ProductDTO>>(products);
        if (products is null)
        {
            return NotFound("Produtos não encontrados");
        }

        return Ok(productsDto);
    }
}
