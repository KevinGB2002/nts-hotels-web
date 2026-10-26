using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NtsHotels.Api.DTOs;
using NtsHotels.Api.Services;

namespace NtsHotels.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HotelsController(IHotelService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HotelResponseDto>>> GetAll(
        [FromQuery] string? name, [FromQuery] int? rating)
        => Ok(await service.GetAllAsync(name, rating));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HotelResponseDto>> GetById(Guid id)
    {
        var hotel = await service.GetByIdAsync(id);
        return hotel is null ? NotFound() : Ok(hotel);
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<ActionResult<HotelResponseDto>> Create(HotelCreateDto dto)
    {
        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, HotelUpdateDto dto)
        => await service.UpdateAsync(id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
        => await service.DeleteAsync(id) ? NoContent() : NotFound();
}
