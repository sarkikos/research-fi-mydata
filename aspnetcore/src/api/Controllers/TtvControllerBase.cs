﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

/*
 * TtvControllerBase implements utility methods which can be used by all controllers. 
 */
public abstract class TtvControllerBase : ControllerBase
{
    // Get ORCID ID from user claims
    protected string GetOrcidId()
    {
        return User.Claims.FirstOrDefault(x => x.Type == "orcid")?.Value;
    }

    // Get timestamp for logging
    public string GetLogTimestamp()
    {
        return DateTime.UtcNow.ToString("s");
    }
}