using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors")]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository repository;
        private readonly IMapper mapper;
        private readonly IPropertyMappingService propertyMappingService;

        public AuthorsController(ICourseLibraryRepository repository,
            IMapper mapper,
            IPropertyMappingService propertyMappingService)
        {
            this.repository = repository ??
                throw new ArgumentNullException(nameof(repository));

            this.mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));

            this.propertyMappingService = propertyMappingService ??
                throw new ArgumentNullException(nameof(propertyMappingService));
        }

        [HttpGet(Name = "GetAuthors")]
        [HttpHead]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthors(
            [FromQuery] AuthorResourceParameters resourceParameters)
        {
            if (!propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>
                (resourceParameters.OrderBy))
            {
                return BadRequest();
            }

            var authorsFromRepo = repository.GetAuthors(resourceParameters);

            var previousPageLink = authorsFromRepo.HasPrevious ?
                CreateAuthorResourseUri(resourceParameters,
                ResourseUriType.PreviousPage) : null;

            var nextPageLink = authorsFromRepo.HasNext ?
                CreateAuthorResourseUri(resourceParameters,
                ResourseUriType.PreviousPage) : null;

            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
                previousPageLink,
                nextPageLink
            };

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetadata));

            return Ok(mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
        }

        [HttpGet]
        [Route("{authorId:guid}", Name = "GetAuthor")]
        public IActionResult GetAuthors(Guid authorId)
        {
            var authorFromRepo = repository.GetAuthor(authorId);

            return authorFromRepo == null ? NotFound() : Ok(mapper.Map<AuthorDto>(authorFromRepo));
        }

        [HttpPost]
        public ActionResult<AuthorDto> CreateAuthor(AuthorForCreationDto author)
        {
            var authorEntity = mapper.Map<Author>(author);
            repository.AddAuthor(authorEntity);
            repository.Save();

            var authorToReturn = mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute("GetAuthor",
                new { authorId = authorToReturn.Id },
                authorToReturn);
        }

        [HttpDelete("{authorId}")]
        public ActionResult DeleteAuthor(Guid authorId)
        {
            var authorFromRepo = repository.GetAuthor(authorId);

            if (authorFromRepo == null)
                return NotFound();

            // CascadeOnDelete is on bt default; when we delete a author, his/her courses
            // will be deleted as will
            repository.DeleteAuthor(authorFromRepo);
            repository.Save();

            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetAuthorsOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }

        private string CreateAuthorResourseUri(
            AuthorResourceParameters resourceParameters,
            ResourseUriType type)
        {
            switch (type)
            {
                case ResourseUriType.PreviousPage:
                    return Url.Link("GetAuthors",
                        new
                        {
                            orderBy = resourceParameters.OrderBy,
                            pageNumber = resourceParameters.PageNumber - 1,
                            pageSize = resourceParameters.PageSize,
                            mainCategory = resourceParameters.MainCategory,
                            searchQuery = resourceParameters.SearchQuery
                        }); ;

                case ResourseUriType.NextPage:
                    return Url.Link("GetAuthors",
                        new
                        {
                            orderBy = resourceParameters.OrderBy,
                            pageNumber = resourceParameters.PageNumber + 1,
                            pageSize = resourceParameters.PageSize,
                            mainCategory = resourceParameters.MainCategory,
                            searchQuery = resourceParameters.SearchQuery
                        });

                default:
                    return Url.Link("GetAuthors",
                         new
                         {
                             orderBy = resourceParameters.OrderBy,
                             pageNumber = resourceParameters.PageNumber,
                             pageSize = resourceParameters.PageSize,
                             mainCategory = resourceParameters.MainCategory,
                             searchQuery = resourceParameters.SearchQuery
                         });
            }
        }
    }
}
