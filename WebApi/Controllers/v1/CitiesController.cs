namespace WebApi.Controllers.v1
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using AutoMapper;
    using FluentValidation.AspNetCore;
    using Application.Dtos.City;
    using Application.Interfaces.City;
    using Application.Validation.City;
    using Domain.Entities;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;
    using Application.Wrappers;

    [ApiController]
    [Route("api/Cities")]
    [ApiVersion("1.0")]
    public class CitiesController: Controller
    {
        private readonly ICityRepository _cityRepository;
        private readonly IMapper _mapper;

        public CitiesController(ICityRepository cityRepository
            , IMapper mapper)
        {
            _cityRepository = cityRepository ??
                throw new ArgumentNullException(nameof(cityRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }
        
        /// <summary>
        /// Gets a list of all Cities.
        /// </summary>
        /// <response code="200">City list returned successfully.</response>
        /// <response code="400">City has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the City.</response>
        /// <remarks>
        /// Requests can be narrowed down with a variety of query string values:
        /// ## Query String Parameters
        /// - **PageNumber**: An integer value that designates the page of records that should be returned.
        /// - **PageSize**: An integer value that designates the number of records returned on the given page that you would like to return. This value is capped by the internal MaxPageSize.
        /// - **SortOrder**: A comma delimited ordered list of property names to sort by. Adding a `-` before the name switches to sorting descendingly.
        /// - **Filters**: A comma delimited list of fields to filter by formatted as `{Name}{Operator}{Value}` where
        ///     - {Name} is the name of a filterable property. You can also have multiple names (for OR logic) by enclosing them in brackets and using a pipe delimiter, eg. `(LikeCount|CommentCount)>10` asks if LikeCount or CommentCount is >10
        ///     - {Operator} is one of the Operators below
        ///     - {Value} is the value to use for filtering. You can also have multiple values (for OR logic) by using a pipe delimiter, eg.`Title@= new|hot` will return posts with titles that contain the text "new" or "hot"
        ///
        ///    | Operator | Meaning                       | Operator  | Meaning                                      |
        ///    | -------- | ----------------------------- | --------- | -------------------------------------------- |
        ///    | `==`     | Equals                        |  `!@=`    | Does not Contains                            |
        ///    | `!=`     | Not equals                    |  `!_=`    | Does not Starts with                         |
        ///    | `>`      | Greater than                  |  `@=*`    | Case-insensitive string Contains             |
        ///    | `&lt;`   | Less than                     |  `_=*`    | Case-insensitive string Starts with          |
        ///    | `>=`     | Greater than or equal to      |  `==*`    | Case-insensitive string Equals               |
        ///    | `&lt;=`  | Less than or equal to         |  `!=*`    | Case-insensitive string Not equals           |
        ///    | `@=`     | Contains                      |  `!@=*`   | Case-insensitive string does not Contains    |
        ///    | `_=`     | Starts with                   |  `!_=*`   | Case-insensitive string does not Starts with |
        /// </remarks>
        [ProducesResponseType(typeof(Response<IEnumerable<CityDto>>), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpGet(Name = "GetCities")]
        public async Task<IActionResult> GetCities([FromQuery] CityParametersDto cityParametersDto)
        {
            var citysFromRepo = await _cityRepository.GetCitiesAsync(cityParametersDto);

            var paginationMetadata = new
            {
                totalCount = citysFromRepo.TotalCount,
                pageSize = citysFromRepo.PageSize,
                currentPageSize = citysFromRepo.CurrentPageSize,
                currentStartIndex = citysFromRepo.CurrentStartIndex,
                currentEndIndex = citysFromRepo.CurrentEndIndex,
                pageNumber = citysFromRepo.PageNumber,
                totalPages = citysFromRepo.TotalPages,
                hasPrevious = citysFromRepo.HasPrevious,
                hasNext = citysFromRepo.HasNext
            };

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetadata));

            var citiesDto = _mapper.Map<IEnumerable<CityDto>>(citysFromRepo);
            var response = new Response<IEnumerable<CityDto>>(citiesDto);

            return Ok(response);
        }
        
        /// <summary>
        /// Gets a single City by ID.
        /// </summary>
        /// <response code="200">City record returned successfully.</response>
        /// <response code="400">City has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the City.</response>
        [ProducesResponseType(typeof(Response<CityDto>), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces("application/json")]
        [HttpGet("{cityId}", Name = "GetCity")]
        public async Task<ActionResult<CityDto>> GetCity(int cityId)
        {
            var cityFromRepo = await _cityRepository.GetCityAsync(cityId);

            if (cityFromRepo == null)
            {
                return NotFound();
            }

            var cityDto = _mapper.Map<CityDto>(cityFromRepo);
            var response = new Response<CityDto>(cityDto);

            return Ok(response);
        }
        
        /// <summary>
        /// Creates a new City record.
        /// </summary>
        /// <response code="201">City created.</response>
        /// <response code="400">City has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the City.</response>
        [ProducesResponseType(typeof(Response<CityDto>), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpPost]
        public async Task<ActionResult<CityDto>> AddCity([FromBody]CityForCreationDto cityForCreation)
        {
            var validationResults = new CityForCreationDtoValidator().Validate(cityForCreation);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }

            var city = _mapper.Map<City>(cityForCreation);
            await _cityRepository.AddCity(city);
            var saveSuccessful = await _cityRepository.SaveAsync();

            if(saveSuccessful)
            {
                var cityFromRepo = await _cityRepository.GetCityAsync(city.CityId);
                var cityDto = _mapper.Map<CityDto>(cityFromRepo);
                var response = new Response<CityDto>(cityDto);
                
                return CreatedAtRoute("GetCity",
                    new { cityDto.CityId },
                    response);
            }

            return StatusCode(500);
        }
        
        /// <summary>
        /// Deletes an existing City record.
        /// </summary>
        /// <response code="201">City deleted.</response>
        /// <response code="400">City has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the City.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces("application/json")]
        [HttpDelete("{cityId}")]
        public async Task<ActionResult> DeleteCity(int cityId)
        {
            var cityFromRepo = await _cityRepository.GetCityAsync(cityId);

            if (cityFromRepo == null)
            {
                return NotFound();
            }

            _cityRepository.DeleteCity(cityFromRepo);
            await _cityRepository.SaveAsync();

            return NoContent();
        }
        
        /// <summary>
        /// Updates an entire existing City.
        /// </summary>
        /// <response code="201">City updated.</response>
        /// <response code="400">City has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the City.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces("application/json")]
        [HttpPut("{cityId}")]
        public async Task<IActionResult> UpdateCity(int cityId, CityForUpdateDto city)
        {
            var cityFromRepo = await _cityRepository.GetCityAsync(cityId);

            if (cityFromRepo == null)
            {
                return NotFound();
            }

            var validationResults = new CityForUpdateDtoValidator().Validate(city);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }

            _mapper.Map(city, cityFromRepo);
            _cityRepository.UpdateCity(cityFromRepo);

            await _cityRepository.SaveAsync();

            return NoContent();
        }
        
        /// <summary>
        /// Updates specific properties on an existing City.
        /// </summary>
        /// <response code="201">City updated.</response>
        /// <response code="400">City has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the City.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpPatch("{cityId}")]
        public async Task<IActionResult> PartiallyUpdateCity(int cityId, JsonPatchDocument<CityForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var existingCity = await _cityRepository.GetCityAsync(cityId);

            if (existingCity == null)
            {
                return NotFound();
            }

            var cityToPatch = _mapper.Map<CityForUpdateDto>(existingCity); // map the city we got from the database to an updatable city model
            patchDoc.ApplyTo(cityToPatch, ModelState); // apply patchdoc updates to the updatable city

            if (!TryValidateModel(cityToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(cityToPatch, existingCity); // apply updates from the updatable city to the db entity so we can apply the updates to the database
            _cityRepository.UpdateCity(existingCity); // apply business updates to data if needed

            await _cityRepository.SaveAsync(); // save changes in the database

            return NoContent();
        }
    }
}