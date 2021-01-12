namespace WebApi.Controllers.v1
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using AutoMapper;
    using FluentValidation.AspNetCore;
    using Application.Dtos.Vet;
    using Application.Interfaces.Vet;
    using Application.Validation.Vet;
    using Domain.Entities;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;
    using Application.Wrappers;

    [ApiController]
    [Route("api/Vets")]
    [ApiVersion("1.0")]
    public class VetsController: Controller
    {
        private readonly IVetRepository _vetRepository;
        private readonly IMapper _mapper;

        public VetsController(IVetRepository vetRepository
            , IMapper mapper)
        {
            _vetRepository = vetRepository ??
                throw new ArgumentNullException(nameof(vetRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }
        
        /// <summary>
        /// Gets a list of all Vets.
        /// </summary>
        /// <response code="200">Vet list returned successfully.</response>
        /// <response code="400">Vet has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Vet.</response>
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
        [ProducesResponseType(typeof(Response<IEnumerable<VetDto>>), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpGet(Name = "GetVets")]
        public async Task<IActionResult> GetVets([FromQuery] VetParametersDto vetParametersDto)
        {
            var vetsFromRepo = await _vetRepository.GetVetsAsync(vetParametersDto);

            var paginationMetadata = new
            {
                totalCount = vetsFromRepo.TotalCount,
                pageSize = vetsFromRepo.PageSize,
                currentPageSize = vetsFromRepo.CurrentPageSize,
                currentStartIndex = vetsFromRepo.CurrentStartIndex,
                currentEndIndex = vetsFromRepo.CurrentEndIndex,
                pageNumber = vetsFromRepo.PageNumber,
                totalPages = vetsFromRepo.TotalPages,
                hasPrevious = vetsFromRepo.HasPrevious,
                hasNext = vetsFromRepo.HasNext
            };

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetadata));

            var vetsDto = _mapper.Map<IEnumerable<VetDto>>(vetsFromRepo);
            var response = new Response<IEnumerable<VetDto>>(vetsDto);

            return Ok(response);
        }
        
        /// <summary>
        /// Gets a single Vet by ID.
        /// </summary>
        /// <response code="200">Vet record returned successfully.</response>
        /// <response code="400">Vet has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Vet.</response>
        [ProducesResponseType(typeof(Response<VetDto>), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces("application/json")]
        [HttpGet("{vetId}", Name = "GetVet")]
        public async Task<ActionResult<VetDto>> GetVet(int vetId)
        {
            var vetFromRepo = await _vetRepository.GetVetAsync(vetId);

            if (vetFromRepo == null)
            {
                return NotFound();
            }

            var vetDto = _mapper.Map<VetDto>(vetFromRepo);
            var response = new Response<VetDto>(vetDto);

            return Ok(response);
        }
        
        /// <summary>
        /// Creates a new Vet record.
        /// </summary>
        /// <response code="201">Vet created.</response>
        /// <response code="400">Vet has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Vet.</response>
        [ProducesResponseType(typeof(Response<VetDto>), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpPost]
        public async Task<ActionResult<VetDto>> AddVet([FromBody]VetForCreationDto vetForCreation)
        {
            var validationResults = new VetForCreationDtoValidator().Validate(vetForCreation);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }

            var vet = _mapper.Map<Vet>(vetForCreation);
            await _vetRepository.AddVet(vet);
            var saveSuccessful = await _vetRepository.SaveAsync();

            if(saveSuccessful)
            {
                var vetFromRepo = await _vetRepository.GetVetAsync(vet.VetId);
                var vetDto = _mapper.Map<VetDto>(vetFromRepo);
                var response = new Response<VetDto>(vetDto);
                
                return CreatedAtRoute("GetVet",
                    new { vetDto.VetId },
                    response);
            }

            return StatusCode(500);
        }
        
        /// <summary>
        /// Deletes an existing Vet record.
        /// </summary>
        /// <response code="201">Vet deleted.</response>
        /// <response code="400">Vet has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Vet.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces("application/json")]
        [HttpDelete("{vetId}")]
        public async Task<ActionResult> DeleteVet(int vetId)
        {
            var vetFromRepo = await _vetRepository.GetVetAsync(vetId);

            if (vetFromRepo == null)
            {
                return NotFound();
            }

            _vetRepository.DeleteVet(vetFromRepo);
            await _vetRepository.SaveAsync();

            return NoContent();
        }
        
        /// <summary>
        /// Updates an entire existing Vet.
        /// </summary>
        /// <response code="201">Vet updated.</response>
        /// <response code="400">Vet has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Vet.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces("application/json")]
        [HttpPut("{vetId}")]
        public async Task<IActionResult> UpdateVet(int vetId, VetForUpdateDto vet)
        {
            var vetFromRepo = await _vetRepository.GetVetAsync(vetId);

            if (vetFromRepo == null)
            {
                return NotFound();
            }

            var validationResults = new VetForUpdateDtoValidator().Validate(vet);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }

            _mapper.Map(vet, vetFromRepo);
            _vetRepository.UpdateVet(vetFromRepo);

            await _vetRepository.SaveAsync();

            return NoContent();
        }
        
        /// <summary>
        /// Updates specific properties on an existing Vet.
        /// </summary>
        /// <response code="201">Vet updated.</response>
        /// <response code="400">Vet has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Vet.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpPatch("{vetId}")]
        public async Task<IActionResult> PartiallyUpdateVet(int vetId, JsonPatchDocument<VetForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var existingVet = await _vetRepository.GetVetAsync(vetId);

            if (existingVet == null)
            {
                return NotFound();
            }

            var vetToPatch = _mapper.Map<VetForUpdateDto>(existingVet); // map the vet we got from the database to an updatable vet model
            patchDoc.ApplyTo(vetToPatch, ModelState); // apply patchdoc updates to the updatable vet

            if (!TryValidateModel(vetToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(vetToPatch, existingVet); // apply updates from the updatable vet to the db entity so we can apply the updates to the database
            _vetRepository.UpdateVet(existingVet); // apply business updates to data if needed

            await _vetRepository.SaveAsync(); // save changes in the database

            return NoContent();
        }
    }
}