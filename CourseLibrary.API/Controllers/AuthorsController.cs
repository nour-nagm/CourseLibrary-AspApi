using AutoMapper;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors")]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository repository;
        private readonly IMapper mapper;

        public AuthorsController(ICourseLibraryRepository repository, IMapper mapper)
        {
            this.repository = repository ?? 
                throw new ArgumentNullException(nameof(repository));
            this.mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthors(
            [FromQuery] AuthorResourceParameters resourceParameters)
        {
            var authorsFromRepo = repository.GetAuthors(resourceParameters);
           
            return Ok(mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
        }

        [Route("{authorId:guid}")]
        public IActionResult GetAuthors(Guid authorId) 
        {
            var authorFromRepo = repository.GetAuthor(authorId);

            return authorFromRepo == null ? NotFound() : Ok(mapper.Map<AuthorDto>(authorFromRepo));
        }
    }
}
