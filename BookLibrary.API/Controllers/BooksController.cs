using AutoMapper;
using BookLibrary.API.Models;
using BookLibrary.API.Models.Dto;
using BookLibrary.API.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BookLibrary.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBookRepository _bookRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    public BooksController(IBookRepository bookRepository, ICategoryRepository categoryRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<BookDto>>> GetBooks(CancellationToken cancellationToken,
        [FromQuery, Range(1, 100)] int pageSize = 10, [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1)
    {
        var bookList = await _bookRepository.GetAllAsync(tracked: false, includeProperties: "Category", cancellationToken: cancellationToken, pageSize: pageSize, pageNumber: pageNumber);
        var totalCount = await _bookRepository.CountAsync(cancellationToken: cancellationToken);
        var items = _mapper.Map<IEnumerable<BookDto>>(bookList);
        var response = new PagedResponse<BookDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items
        };
        return Ok(response);
    }

    [HttpGet("{id:int}", Name = "GetBook")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookDto>> GetBook(int id, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetAsync(u => u.Id == id, tracked: false, includeProperties: "Category", cancellationToken: cancellationToken);
        if (book is null)
        {
            return NotFound();
        }
        var response = _mapper.Map<BookDto>(book);
        return Ok(response);
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
        var category = await _categoryRepository.GetAsync(c => c.Id == createDto.CategoryId, tracked: false, cancellationToken: cancellationToken);

        if (category is null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid category",
                Detail = "The provided category ID is invalid."
            });
        }

        if (await _bookRepository.IsbnExistsAsync(createDto.Isbn, cancellationToken: cancellationToken))
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "ISBN already exists",
                Detail = "A book with the provided ISBN already exists."
            });
        }

        Book book = _mapper.Map<Book>(createDto);
        await _bookRepository.CreateAsync(book, cancellationToken);
        var createdBook = await _bookRepository.GetAsync(u => u.Id == book.Id, tracked: false, includeProperties: "Category", cancellationToken: cancellationToken);
        var response = _mapper.Map<BookDto>(createdBook);
        return CreatedAtRoute("GetBook", new { id = book.Id }, response);
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
        var bookToUpdate = await _bookRepository.GetAsync(u => u.Id == id, tracked: true, includeProperties: "Category", cancellationToken: cancellationToken);

        if (bookToUpdate is null)
        {
            return NotFound();
        }

        var category = await _categoryRepository.GetAsync(c => c.Id == updateDto.CategoryId, tracked: false, cancellationToken: cancellationToken);

        if (category is null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid category",
                Detail = "The provided category ID is invalid."
            });
        }

        if (await _bookRepository.IsbnExistsAsync(updateDto.Isbn, excludeId: id, cancellationToken: cancellationToken))
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "ISBN already exists",
                Detail = "A book with the provided ISBN already exists."
            });
        }

        _mapper.Map(updateDto, bookToUpdate); // Map the updateDto to the existing book entity not creating a new instance
        await _bookRepository.UpdateAsync(bookToUpdate, cancellationToken);
        var updatedBook = await _bookRepository.GetAsync(u => u.Id == bookToUpdate.Id, tracked: false, includeProperties: "Category", cancellationToken: cancellationToken);
        var response = _mapper.Map<BookDto>(updatedBook);
        return Ok(response);
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
