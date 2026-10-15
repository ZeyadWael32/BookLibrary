using AutoMapper;
using BookLibrary.API.Models;
using BookLibrary.API.Models.Dto;

namespace BookLibrary.API
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<Book, BookDto>().ReverseMap();
            CreateMap<Book, BookCreateDto>().ReverseMap();
            CreateMap<Book, BookUpdateDto>().ReverseMap();
        }
    }
}
