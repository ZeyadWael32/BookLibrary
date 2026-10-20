using AutoMapper;
using BookLibrary.API.Models;
using BookLibrary.API.Models.Dto;
using BookLibrary.API.Repository;
using BookLibrary.API.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace BookLibrary.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories(CancellationToken cancellationToken)
    {
        var categoryList = await _categoryRepository.GetAllAsync(tracked: false, cancellationToken: cancellationToken);
        var response = _mapper.Map<IEnumerable<CategoryDto>>(categoryList);
        return Ok(response);
    }

    [HttpGet("{id:int}", Name = "GetCategory")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetAsync(u => u.Id == id, tracked: false, cancellationToken: cancellationToken);
        if (category is null)
        {
            return NotFound();
        }
        var response = _mapper.Map<CategoryDto>(category);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CategoryCreateDto createDto, CancellationToken cancellationToken)
    {
        if (await _categoryRepository.NameExistsAsync(createDto.Name, cancellationToken: cancellationToken))
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Category name already exists",
                Detail = "A category with the provided name already exists."
            });
        }

        Category category = _mapper.Map<Category>(createDto);
        await _categoryRepository.CreateAsync(category, cancellationToken);
        var createdCategory = await _categoryRepository.GetAsync(u => u.Id == category.Id, tracked: false, cancellationToken: cancellationToken);
        var response = _mapper.Map<CategoryDto>(createdCategory);
        return CreatedAtRoute("GetCategory", new { id = category.Id }, response);
    }

    [HttpPut("{id:int}", Name = "UpdateCategory")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] CategoryUpdateDto updateDto, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetAsync(u => u.Id == id, tracked: true, cancellationToken: cancellationToken);

        if (category is null)
        {
            return NotFound();
        }

        if (await _categoryRepository.NameExistsAsync(updateDto.Name, id, cancellationToken: cancellationToken))
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Category name already exists",
                Detail = "A category with the provided name already exists."
            });
        }

        _mapper.Map(updateDto, category);
        await _categoryRepository.UpdateAsync(category, cancellationToken);
        var updatedCategory = await _categoryRepository.GetAsync(u => u.Id == category.Id, tracked: false, cancellationToken: cancellationToken);
        var response = _mapper.Map<CategoryDto>(updatedCategory);
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetAsync(u => u.Id == id, tracked: true, cancellationToken: cancellationToken);
        if (category is null)
        {
            return NotFound();
        }
        await _categoryRepository.RemoveAsync(category, cancellationToken);
        return NoContent();
    }
}
