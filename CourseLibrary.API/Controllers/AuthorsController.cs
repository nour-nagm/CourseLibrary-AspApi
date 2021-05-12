using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IPropertyCheckerService propertyCheckerService;

        public AuthorsController(ICourseLibraryRepository repository,
            IMapper mapper,
            IPropertyMappingService propertyMappingService,
            IPropertyCheckerService propertyCheckerService)
        {
            this.repository = repository ??
                throw new ArgumentNullException(nameof(repository));

            this.mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));

            this.propertyMappingService = propertyMappingService ??
                throw new ArgumentNullException(nameof(propertyMappingService));

            this.propertyCheckerService = propertyCheckerService ??
                throw new ArgumentNullException(nameof(propertyMappingService));
        }

        [HttpGet(Name = "GetAuthors")]
        [HttpHead]
        public IActionResult GetAuthors(
            [FromQuery] AuthorsResourceParameters resourceParameters,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType,
                out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            if (!propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>
                (resourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!propertyCheckerService.TypeHasProperties<AuthorDto>
                (resourceParameters.Fields))
            {
                return BadRequest();
            }

            var authorsFromRepo = repository.GetAuthors(resourceParameters);

            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages
            };

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetadata));


            var shapedAuthors = mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo)
                .ShapeData(resourceParameters.Fields); // reminder : this is collection of ExpandoObjects

            //Doesn't include links in the body
            if (parsedMediaType.MediaType != "application/vnd.marvin.hateoas+json")
                return Ok(shapedAuthors);

            var shapedAuthorsWithLinks = shapedAuthors.Select(author =>
            {
                (author as IDictionary<string, object>)
                    .Add("links", CreateLinksForAuthor((Guid)(author as IDictionary<string, object>)["Id"], null));
                return author as IDictionary<string, object>;
            });

            var linkedCollectionResource = new
            {
                value = shapedAuthorsWithLinks,
                links = CreateLinksForAuthors(resourceParameters,
                                              authorsFromRepo.HasNext,
                                              authorsFromRepo.HasPrevious)
            };

            return Ok(linkedCollectionResource);
        }

        [HttpGet]
        [Route("{authorId:guid}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid authorId, string fields,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType,
                out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            if (!propertyCheckerService.TypeHasProperties<AuthorDto>(fields))
                return BadRequest();

            var authorFromRepo = repository.GetAuthor(authorId);

            if (authorFromRepo == null)
                return NotFound();

            var linkedResourceToReturn =
                mapper.Map<AuthorDto>(authorFromRepo).ShapeData(fields)
                as IDictionary<string, object>;

            if (parsedMediaType.MediaType == "application/vnd.marvin.hateoas+json")
            {
                linkedResourceToReturn.Add("links",
                    CreateLinksForAuthor(authorId, fields));
            }

            return Ok(linkedResourceToReturn);
        }

        [HttpPost(Name = "CreateAuthor")]
        public ActionResult<AuthorDto> CreateAuthor(AuthorForCreationDto author)
        {
            var authorEntity = mapper.Map<Author>(author);
            repository.AddAuthor(authorEntity);
            repository.Save();

            var authorToReturn = mapper.Map<AuthorDto>(authorEntity);

            var linkedResourceToReturn = authorToReturn.ShapeData(null)
               as IDictionary<string, object>;

            linkedResourceToReturn.Add("links",
                CreateLinksForAuthor(authorToReturn.Id, null));

            return CreatedAtRoute("GetAuthor",
                new { authorId = linkedResourceToReturn["Id"] },
                linkedResourceToReturn);
        }

        [HttpDelete("{authorId}", Name = "DeleteAuthor")]
        public ActionResult DeleteAuthor(Guid authorId)
        {
            var authorFromRepo = repository.GetAuthor(authorId);

            if (authorFromRepo == null)
                return NotFound();

            // CascadeOnDelete is on by default; when we delete a author, his/her courses
            // will be deleted as will
            repository.DeleteAuthor(authorFromRepo);
            repository.Save();

            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetAuthorsOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST, DELETE");
            return Ok();
        }

        private string CreateAuthorResourseUri(
            AuthorsResourceParameters resourceParameters,
            ResourseUriType type)
        {
            return Url.Link("GetAuthors",
                new
                {
                    fields = resourceParameters.Fields,
                    orderBy = resourceParameters.OrderBy,
                    mainCategory = resourceParameters.MainCategory,
                    searchQuery = resourceParameters.SearchQuery,
                    pageSize = resourceParameters.PageSize,
                    pageNumber = type switch
                    {
                        ResourseUriType.PreviousPage => resourceParameters.PageNumber - 1,
                        ResourseUriType.NextPage => resourceParameters.PageNumber + 1,
                        //Current page
                        _ => resourceParameters.PageNumber
                    }
                });
        }

        private IEnumerable<LinkDto> CreateLinksForAuthor(Guid authorId, string fields)
        {
            return new List<LinkDto> // list of links for individual author
            {
                new
                LinkDto(
                    Url.Link(routeName: "GetAuthor", values: string.IsNullOrWhiteSpace(fields) ? new { authorId } : new { authorId, fields }), //href
                    "self", //rel 
                    "GET"), // HTTP method

                new LinkDto(Url.Link("DeleteAuthor", new { authorId }),
                        "delete_author",
                        "DELETE"),

                new LinkDto(Url.Link("CreateCourseForAuthor", new { authorId }),
                        "create_course_for_author",
                        "POST"),

                new LinkDto(Url.Link("GetCoursesForAuthor", new { authorId }),
                        "courses",
                        "GET")
            };
        }

        private IEnumerable<LinkDto> CreateLinksForAuthors(
            AuthorsResourceParameters resourceParameters,
            bool hasNext, bool hasPrevious)
        {
            // Links for getting courses for authors, creating, or deleting aren't implemented 

            var links = new List<LinkDto>();

            links.Add(
            // self
             new LinkDto(CreateAuthorResourseUri(
                 resourceParameters, ResourseUriType.Current),
                 "self",
                 "GET"));

            if (hasNext)
            {
                links.Add(
                    new LinkDto(CreateAuthorResourseUri(
                        resourceParameters, ResourseUriType.NextPage),
                        "nextPage",
                        "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateAuthorResourseUri(
                        resourceParameters, ResourseUriType.PreviousPage),
                        "previousPage",
                        "GET"));
            }

            return links;
        }
    }
}
