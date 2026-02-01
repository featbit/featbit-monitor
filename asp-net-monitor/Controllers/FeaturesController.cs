using FeatBit.Sdk.Server;
using FeatBit.Sdk.Server.Model;
using Microsoft.AspNetCore.Mvc;

namespace DotNetServerMonitor.Controllers;

/// <summary>
/// FeatBit feature flag management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FeaturesController : ControllerBase
{
    private readonly IFbClient _fbClient;
    private readonly ILogger<FeaturesController> _logger;

    public FeaturesController(IFbClient fbClient, ILogger<FeaturesController> logger)
    {
        _fbClient = fbClient;
        _logger = logger;
    }

    /// <summary>
    /// Get the value of a feature flag
    /// </summary>
    /// <param name="flagKey">The feature flag key</param>
    /// <param name="type">The flag type: bool, string, int, double (default: bool)</param>
    /// <returns>The evaluated flag value</returns>
    [HttpGet("{flagKey}")]
    public IActionResult GetFlag(string flagKey, [FromQuery] string type = "bool")
    {
        // Use Session ID as user identifier
        var sessionId = HttpContext.Session.Id;
        var user = FbUser.Builder(sessionId).Build();

        _logger.LogInformation("Evaluating feature flag {FlagKey} (type: {Type}) for session {SessionId}", flagKey, type, sessionId);

        // Evaluate and log value before returning
        object evaluationValue = type.ToLower() switch
        {
            "string" => _fbClient.StringVariation(flagKey, user, defaultValue: ""),
            "int" => _fbClient.IntVariation(flagKey, user, defaultValue: 0),
            "double" => _fbClient.DoubleVariation(flagKey, user, defaultValue: 0.0),
            _ => _fbClient.BoolVariation(flagKey, user, defaultValue: false)
        };

        _logger.LogInformation("Feature flag {FlagKey} evaluated to: {Value}", flagKey, evaluationValue);

        return Ok(evaluationValue);
    }
}
