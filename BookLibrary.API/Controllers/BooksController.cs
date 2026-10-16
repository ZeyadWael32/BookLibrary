using AutoMapper;
using BookLibrary.API.Models;
using BookLibrary.API.Models.Dto;
using BookLibrary.API.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace BookLibrary.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;
    public BooksController(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks(CancellationToken cancellationToken)
    {
        var bookList = await _bookRepository.GetAllAsync(tracked: false, cancellationToken: cancellationToken);
        var response = _mapper.Map<IEnumerable<BookDto>>(bookList);
        return Ok(response);
    }

    [HttpGet("{id:int}", Name = "GetBook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookDto>> GetBook(int id, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetAsync(u => u.Id == id, tracked: false, cancellationToken: cancellationToken);
        if (book is null)
        {
            return NotFound();
        }
        var response = _mapper.Map<BookDto>(book);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookDto>> CreateBook([FromBody] BookCreateDto createDto, CancellationToken cancellationToken)
    {
        if (await _bookRepository.IsbnExistsAsync(createDto.Isbn, cancellationToken))
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
        var response = _mapper.Map<BookDto>(book);
        return CreatedAtRoute("GetBook", new { id = book.Id }, response);
    }
}
