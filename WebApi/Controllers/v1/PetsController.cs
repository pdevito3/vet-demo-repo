namespace WebApi.Controllers.v1
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using AutoMapper;
    using FluentValidation.AspNetCore;
    using Application.Dtos.Pet;
    using Application.Interfaces.Pet;
    using Application.Validation.Pet;
    using Domain.Entities;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;
    using Application.Wrappers;

    [ApiController]
    [Route("api/Pets")]
    [ApiVersion("1.0")]
    public class PetsController: Controller
    {
        private readonly IPetRepository _petRepository;
        private readonly IMapper _mapper;

        public PetsController(IPetRepository petRepository
            , IMapper mapper)
        {
            _petRepository = petRepository ??
                throw new ArgumentNullException(nameof(petRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }
        
        /// <summary>
        /// Gets a list of all Pets.
        /// </summary>
        /// <response code="200">Pet list returned successfully.</response>
        /// <response code="400">Pet has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Pet.</response>
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
        [ProducesResponseType(typeof(Response<IEnumerable<PetDto>>), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpGet(Name = "GetPets")]
        public async Task<IActionResult> GetPets([FromQuery] PetParametersDto petParametersDto)
        {
            var petsFromRepo = await _petRepository.GetPetsAsync(petParametersDto);

            var paginationMetadata = new
            {
                totalCount = petsFromRepo.TotalCount,
                pageSize = petsFromRepo.PageSize,
                currentPageSize = petsFromRepo.CurrentPageSize,
                currentStartIndex = petsFromRepo.CurrentStartIndex,
                currentEndIndex = petsFromRepo.CurrentEndIndex,
                pageNumber = petsFromRepo.PageNumber,
                totalPages = petsFromRepo.TotalPages,
                hasPrevious = petsFromRepo.HasPrevious,
                hasNext = petsFromRepo.HasNext
            };

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetadata));

            var petsDto = _mapper.Map<IEnumerable<PetDto>>(petsFromRepo);
            var response = new Response<IEnumerable<PetDto>>(petsDto);

            return Ok(response);
        }
        
        /// <summary>
        /// Gets a single Pet by ID.
        /// </summary>
        /// <response code="200">Pet record returned successfully.</response>
        /// <response code="400">Pet has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Pet.</response>
        [ProducesResponseType(typeof(Response<PetDto>), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces("application/json")]
        [HttpGet("{petId}", Name = "GetPet")]
        public async Task<ActionResult<PetDto>> GetPet(Guid petId)
        {
            var petFromRepo = await _petRepository.GetPetAsync(petId);

            if (petFromRepo == null)
            {
                return NotFound();
            }

            var petDto = _mapper.Map<PetDto>(petFromRepo);
            var response = new Response<PetDto>(petDto);

            return Ok(response);
        }
        
        /// <summary>
        /// Creates a new Pet record.
        /// </summary>
        /// <response code="201">Pet created.</response>
        /// <response code="400">Pet has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Pet.</response>
        [ProducesResponseType(typeof(Response<PetDto>), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpPost]
        public async Task<ActionResult<PetDto>> AddPet([FromBody]PetForCreationDto petForCreation)
        {
            var validationResults = new PetForCreationDtoValidator().Validate(petForCreation);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }

            var pet = _mapper.Map<Pet>(petForCreation);
            await _petRepository.AddPet(pet);
            var saveSuccessful = await _petRepository.SaveAsync();

            if(saveSuccessful)
            {
                var petFromRepo = await _petRepository.GetPetAsync(pet.PetId);
                var petDto = _mapper.Map<PetDto>(petFromRepo);
                var response = new Response<PetDto>(petDto);
                
                return CreatedAtRoute("GetPet",
                    new { petDto.PetId },
                    response);
            }

            return StatusCode(500);
        }
        
        /// <summary>
        /// Deletes an existing Pet record.
        /// </summary>
        /// <response code="201">Pet deleted.</response>
        /// <response code="400">Pet has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Pet.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces("application/json")]
        [HttpDelete("{petId}")]
        public async Task<ActionResult> DeletePet(Guid petId)
        {
            var petFromRepo = await _petRepository.GetPetAsync(petId);

            if (petFromRepo == null)
            {
                return NotFound();
            }

            _petRepository.DeletePet(petFromRepo);
            await _petRepository.SaveAsync();

            return NoContent();
        }
        
        /// <summary>
        /// Updates an entire existing Pet.
        /// </summary>
        /// <response code="201">Pet updated.</response>
        /// <response code="400">Pet has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Pet.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces("application/json")]
        [HttpPut("{petId}")]
        public async Task<IActionResult> UpdatePet(Guid petId, PetForUpdateDto pet)
        {
            var petFromRepo = await _petRepository.GetPetAsync(petId);

            if (petFromRepo == null)
            {
                return NotFound();
            }

            var validationResults = new PetForUpdateDtoValidator().Validate(pet);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }

            _mapper.Map(pet, petFromRepo);
            _petRepository.UpdatePet(petFromRepo);

            await _petRepository.SaveAsync();

            return NoContent();
        }
        
        /// <summary>
        /// Updates specific properties on an existing Pet.
        /// </summary>
        /// <response code="201">Pet updated.</response>
        /// <response code="400">Pet has missing/invalid values.</response>
        /// <response code="500">There was an error on the server while creating the Pet.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpPatch("{petId}")]
        public async Task<IActionResult> PartiallyUpdatePet(Guid petId, JsonPatchDocument<PetForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var existingPet = await _petRepository.GetPetAsync(petId);

            if (existingPet == null)
            {
                return NotFound();
            }

            var petToPatch = _mapper.Map<PetForUpdateDto>(existingPet); // map the pet we got from the database to an updatable pet model
            patchDoc.ApplyTo(petToPatch, ModelState); // apply patchdoc updates to the updatable pet

            if (!TryValidateModel(petToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(petToPatch, existingPet); // apply updates from the updatable pet to the db entity so we can apply the updates to the database
            _petRepository.UpdatePet(existingPet); // apply business updates to data if needed

            await _petRepository.SaveAsync(); // save changes in the database

            return NoContent();
        }
    }
}