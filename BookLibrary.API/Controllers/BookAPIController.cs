using AutoMapper;
using BookLibrary.API.Models.Dto;
using BookLibrary.API.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace BookLibrary.API.Controllers;

[Route("api/BookAPI")]
[ApiController]
public class BookAPIController : ControllerBase
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;
    public BookAPIController(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks(CancellationToken cancellationToken)
    {
        var bookList = await _bookRepository.GetAllAsync(cancellationToken: cancellationToken);
        var response = _mapper.Map<IEnumerable<BookDto>>(bookList);
        return Ok(response);
    }

    [HttpGet("{id:int}", Name = "GetBook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDto>> GetBook(int id, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetAsync(u => u.Id == id, cancellationToken: cancellationToken);
        if (book is null)
        {
            return NotFound();
        }
        var response = _mapper.Map<BookDto>(book);
        return Ok(response);
    }
}
