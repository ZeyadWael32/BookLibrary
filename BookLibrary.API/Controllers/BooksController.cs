using AutoMapper;
using BookLibrary.API.Models;
using BookLibrary.API.Models.Dto;
using BookLibrary.API.Models.Queries;
using BookLibrary.API.Repository.IRepository;
using BookLibrary.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLibrary.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ApiControllerBase
{
    private readonly IBookService _bookService;
    private readonly IBookRepository _bookRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    public BooksController(IBookService bookService, IBookRepository bookRepository, ICategoryRepository categoryRepository, IMapper mapper)
    {
        _bookService = bookService;
        _bookRepository = bookRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<BookDto>>> GetBooks([FromQuery] BookQueryParameters query, CancellationToken cancellationToken)
    {
        var response = await _bookService.GetPagedAsync(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:int}", Name = "GetBook")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookDto>> GetBook(int id, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetByIdAsync(id, cancellationToken);
        if (book is null)
        {
            return NotFound();
        }
        return Ok(book);
    }

    [HttpPost]
    [Authorize(Policy = "CatalogWriter")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookDto>> CreateBook([FromBody] BookCreateDto createDto, CancellationToken cancellationToken)
    {
        var result = await _bookService.CreateAsync(createDto, cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return CreatedAtRoute("GetBook", new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:int}", Name = "UpdateBook")]
    [Authorize(Policy = "CatalogWriter")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookDto>> UpdateBook(int id, [FromBody] BookUpdateDto updateDto, CancellationToken cancellationToken)
    {
        var result = await _bookService.UpdateAsync(id, updateDto, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CatalogWriter")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteBook(int id, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetAsync(u => u.Id == id, tracked: true, includeProperties: "Category", cancellationToken: cancellationToken);
        if (book is null)
        {
            return NotFound();
        }
        await _bookRepository.RemoveAsync(book, cancellationToken);
        return NoContent();
    }
}
