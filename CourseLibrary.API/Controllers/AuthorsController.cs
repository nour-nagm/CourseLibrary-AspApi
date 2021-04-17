using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors")]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository repository;

        public AuthorsController(ICourseLibraryRepository repository)
        {
            this.repository = repository ?? 
                throw new ArgumentNullException(nameof(repository));
        }

        [HttpGet()]
        public IActionResult GetAuthors()
        {
            var authors = repository.GetAuthors();
            return Ok(authors);
        }

        [Route("{authorId:guid}")]
        public IActionResult GetAuthors(Guid authorId) 
        {
            var author = repository.GetAuthor(authorId);

            return author == null ? NotFound() : Ok(author);
        }
    }
}
