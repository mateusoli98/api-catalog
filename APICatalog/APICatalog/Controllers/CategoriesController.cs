using APICatalog.DTOs;
using APICatalog.Models;
using APICatalog.Pagination;
using APICatalog.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;

namespace APICatalog.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoriesController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> Get()
    {
        var categories = await _unitOfWork.CategoryRepository.GetAllAsync();

        if (categories is null)
        {
            return NotFound("Nenhuma categoria foi encontrada");
        }

        return Ok(categories);
    }

    [HttpGet("pagination")]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategoriesPagination([FromQuery] CategoryParameters categoriesParameters)
    {
        var categories = await _unitOfWork.CategoryRepository.GetCategoriesAsync(categoriesParameters);

        return LoadCategories(categories);
    }

    [HttpGet("pagination-filter")]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategoriesPaginationFilter([FromQuery] CategoryFilterName filter)
    {
        var categories = await _unitOfWork.CategoryRepository.GetCategoriesFilterNameAsync(filter);

        return LoadCategories(categories);
    }

    [HttpGet("{id:int}", Name = "GetCategories")]
    public async Task<ActionResult<CategoryDTO>> GetById([FromRoute] int id)
    { 
        var category = await _unitOfWork.CategoryRepository.GetAsync(x => x.CategoryId == id);

        if (category is null)
        {
            return NotFound("Categoria não encontrada");
        }

        return Ok(category);
    }

    [HttpPost()]
    public async Task<ActionResult<CategoryDTO>> Post([FromBody] CategoryDTO categoryDTO)
    {
        if (categoryDTO is null)
        {
            return BadRequest("Dados invalidos");
        }

        Category category = _mapper.Map<Category>(categoryDTO);

        Category resultCategory = _unitOfWork.CategoryRepository.Create(category);
        await _unitOfWork.CommitAsync();

        return new CreatedAtRouteResult("GetCategories", new { id = resultCategory.CategoryId }, resultCategory);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDTO>> Put([FromRoute] int id, CategoryDTO categoryReq)
    {
        if (categoryReq.CategoryId != id)
        {
            return BadRequest("Categoria invalida");
        }

        Category category = _mapper.Map<Category>(categoryReq);

        _unitOfWork.CategoryRepository.Update(category);
        await _unitOfWork.CommitAsync();

        return Ok(categoryReq);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<CategoryDTO>> Delete([FromRoute] int id)
    {
        var category = await _unitOfWork.CategoryRepository.GetAsync(x => x.CategoryId == id);

        if (category is null)
        {
            return NotFound("Categoria não encontrada");
        }

        var categoryDeleted = _unitOfWork.CategoryRepository.Delete(category);
        await _unitOfWork.CommitAsync();

        return Ok(categoryDeleted);
    }

    private ActionResult<IEnumerable<CategoryDTO>> LoadCategories(IPagedList<Category>? categories)
    {
        var metadata = new
        {
            categories.Count,
            categories.PageSize,
            categories.PageCount,
            categories.TotalItemCount,
            categories.HasNextPage,
            categories.HasPreviousPage
        };

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        var categoriesDto = _mapper.Map<IEnumerable<CategoryDTO>>(categories);

        if (categories is null)
        {
            return NotFound("Categorias não encontrados");
        }

        return Ok(categoriesDto);
    }
}
