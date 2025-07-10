using System;
using System.Collections.Generic;

namespace dotnet_rest_api.DTOs;

/// <summary>
/// Lightweight DTO for mobile project list view
/// Optimized for Flutter app consumption with minimal properties
/// </summary>
public class MobileProjectDto
{
    /// <summary>
    /// Unique identifier for the project
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Name of the project
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Current status of the project (Active, Planning, Completed, etc.)
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Start date of the project
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Estimated completion date
    /// </summary>
    public DateTime? EstimatedEndDate { get; set; }
    
    /// <summary>
    /// Project location
    /// </summary>
    public string Location { get; set; } = string.Empty;
    
    /// <summary>
    /// Client name
    /// </summary>
    public string ClientName { get; set; } = string.Empty;
    
    /// <summary>
    /// URL to project thumbnail image
    /// </summary>
    public string? ThumbnailUrl { get; set; }
    
    /// <summary>
    /// Overall completion percentage (0-100)
    /// </summary>
    public int CompletionPercentage { get; set; }
}

/// <summary>
/// Detailed project information for mobile app
/// Used for project detail view in Flutter app
/// </summary>
public class MobileProjectDetailDto
{
    /// <summary>
    /// Unique identifier for the project
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Name of the project
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Current status of the project
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the project
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Project start date
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Projected end date
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Project location
    /// </summary>
    public string Location { get; set; } = string.Empty;
    
    /// <summary>
    /// Client name
    /// </summary>
    public string ClientName { get; set; } = string.Empty;
    
    /// <summary>
    /// Total budget for the project
    /// </summary>
    public decimal BudgetTotal { get; set; }
    
    /// <summary>
    /// Overall completion percentage (0-100)
    /// </summary>
    public int CompletionPercentage { get; set; }
    
    /// <summary>
    /// When the project was last updated
    /// </summary>
    public DateTime LastUpdated { get; set; }
    
    /// <summary>
    /// List of project image URLs
    /// </summary>
    public List<string> ImageUrls { get; set; } = new List<string>();
    
    /// <summary>
    /// Number of currently active tasks
    /// </summary>
    public int ActiveTasks { get; set; }
    
    /// <summary>
    /// Number of completed tasks
    /// </summary>
    public int CompletedTasks { get; set; }
}

/// <summary>
/// Mobile dashboard summary data for Flutter app
/// Used for at-a-glance overview in mobile dashboard
/// </summary>
public class MobileDashboardDto
{
    /// <summary>
    /// Total number of projects user has access to
    /// </summary>
    public int ProjectCount { get; set; }
    
    /// <summary>
    /// Number of currently active projects
    /// </summary>
    public int ActiveProjectCount { get; set; }
    
    /// <summary>
    /// Number of completed projects
    /// </summary>
    public int CompletedProjectCount { get; set; }
    
    /// <summary>
    /// List of recent projects for quick access
    /// </summary>
    public List<MobileProjectDto> RecentProjects { get; set; } = new List<MobileProjectDto>();
    
    /// <summary>
    /// Timestamp when this data was last synchronized
    /// </summary>
    public DateTime LastSyncTimestamp { get; set; }
}
