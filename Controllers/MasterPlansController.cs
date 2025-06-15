using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.Models;
using dotnet_rest_api.Services;

namespace dotnet_rest_api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class MasterPlansController : ControllerBase
{
    private readonly IMasterPlanService _masterPlanService;

    public MasterPlansController(IMasterPlanService masterPlanService)
    {
        _masterPlanService = masterPlanService;
    }

    /// <summary>
    /// Get all master plans
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MasterPlan>>> GetMasterPlans()
    {
        try
        {
            var masterPlans = await _masterPlanService.GetAllMasterPlansAsync();
            return Ok(masterPlans);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get master plan by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MasterPlan>> GetMasterPlan(Guid id)
    {
        try
        {
            var masterPlan = await _masterPlanService.GetMasterPlanByIdAsync(id);
            if (masterPlan == null)
                return NotFound($"Master plan with ID {id} not found");

            return Ok(masterPlan);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get master plans by project ID
    /// </summary>
    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<IEnumerable<MasterPlan>>> GetMasterPlansByProject(Guid projectId)
    {
        try
        {
            var masterPlans = await _masterPlanService.GetMasterPlansByProjectIdAsync(projectId);
            return Ok(masterPlans);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a new master plan
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MasterPlan>> CreateMasterPlan([FromBody] MasterPlan masterPlan)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdPlan = await _masterPlanService.CreateMasterPlanAsync(masterPlan);
            return CreatedAtAction(nameof(GetMasterPlan), new { id = createdPlan.MasterPlanId }, createdPlan);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Update an existing master plan
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MasterPlan>> UpdateMasterPlan(Guid id, [FromBody] MasterPlan masterPlan)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedPlan = await _masterPlanService.UpdateMasterPlanAsync(id, masterPlan);
            if (updatedPlan == null)
                return NotFound($"Master plan with ID {id} not found");

            return Ok(updatedPlan);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a master plan
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteMasterPlan(Guid id)
    {
        try
        {
            var deleted = await _masterPlanService.DeleteMasterPlanAsync(id);
            if (!deleted)
                return NotFound($"Master plan with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
