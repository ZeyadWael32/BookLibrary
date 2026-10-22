using BookLibrary.API.Models.Dto;
using BookLibrary.API.Models.Queries;

namespace BookLibrary.API.Services.IServices;

public interface IBookService
{
    Task<PagedResponse<BookDto>> GetPagedAsync(BookQueryParameters query, CancellationToken cancellationToken);
    Task<BookDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<ServiceResult<BookDto>> CreateAsync(BookCreateDto dto, CancellationToken cancellationToken);
    Task<ServiceResult<BookDto>> UpdateAsync(int id, BookUpdateDto dto, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken);
}
