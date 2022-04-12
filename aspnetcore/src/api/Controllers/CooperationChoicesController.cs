﻿using api.Services;
using api.Models;
using api.Models.ProfileEditor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using api.Models.Ttv;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace api.Controllers
{
    /*
     * CooperationChoicesController implements profile editor API for setting user choices for cooperation.
     */
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "RequireScopeApi1AndClaimOrcid")]
    public class CooperationChoicesController : TtvControllerBase
    {
        private readonly TtvContext _ttvContext;
        private readonly UserProfileService _userProfileService;
        private readonly TtvSqlService _ttvSqlService;
        private readonly ILogger<UserProfileController> _logger;
        private readonly UtilityService _utilityService;
        private readonly IMemoryCache _cache;

        public CooperationChoicesController(TtvContext ttvContext, UserProfileService userProfileService, TtvSqlService ttvSqlService, ILogger<UserProfileController> logger, UtilityService utilityService, IMemoryCache memoryCache)
        {
            _ttvContext = ttvContext;
            _userProfileService = userProfileService;
            _ttvSqlService = ttvSqlService;
            _utilityService = utilityService;
            _logger = logger;
            _cache = memoryCache;
        }

        /// <summary>
        /// Get cooperation selections.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseCooperationGet), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            // Get userprofile
            var orcidId = this.GetOrcidId();
            var userprofileId = await _userProfileService.GetUserprofileId(orcidId);
            if (userprofileId == -1)
            {
                // Userprofile not found
                return Ok(new ApiResponse(success: false, reason: "profile not found"));
            }

            // Send cached response, if exists. Cache key is ORCID ID + "_choices"
            var cacheKey = orcidId + "_choices";
            List<ProfileEditorCooperationItem> cachedResponse;
            if (_cache.TryGetValue(cacheKey, out cachedResponse))
            {
                return Ok(new ApiResponseCooperationGet(success: true, reason: "", data: cachedResponse, fromCache: true));
            }

            // Get choices from DimReferencedata by code scheme.
            // MUST NOT use AsNoTracking, because it is possible that DimUserChoise entities will be added.
            var dimReferenceDataUserChoices = await _ttvContext.DimReferencedata.Where(dr => dr.CodeScheme == Constants.CodeSchemes.USER_CHOICES)
                .Include(dr => dr.DimUserChoices.Where(duc => duc.DimUserProfileId == userprofileId)).ToListAsync();

            // Chech that all available choices have DimUserChoice for this user profile.
            foreach (DimReferencedatum dimReferenceDataUserChoice in dimReferenceDataUserChoices)
            {
                var dimUserChoice = dimReferenceDataUserChoice.DimUserChoices.FirstOrDefault();
                if (dimUserChoice == null)
                {
                    // Add new DimUserChoice
                    dimUserChoice = new DimUserChoice()
                    {
                        UserChoiceValue = false,
                        DimUserProfileId = userprofileId,
                        DimReferencedataIdAsUserChoiceLabelNavigation = dimReferenceDataUserChoice,
                        SourceId = Constants.SourceIdentifiers.PROFILE_API,
                        SourceDescription = Constants.SourceDescriptions.PROFILE_API,
                        Created = _utilityService.getCurrentDateTime(),
                        Modified = _utilityService.getCurrentDateTime()
                    };
                    _ttvContext.DimUserChoices.Add(dimUserChoice);
                }
            }
            await _ttvContext.SaveChangesAsync();


            // Collect data for API response.
            var cooperationItems = new List<ProfileEditorCooperationItem>();
            foreach (DimReferencedatum dimReferenceDataUserChoice in dimReferenceDataUserChoices)
            {
                var dimUserChoice = dimReferenceDataUserChoice.DimUserChoices.First();
                cooperationItems.Add(
                    new ProfileEditorCooperationItem()
                    {
                        Id = dimUserChoice.Id,
                        NameFi = dimUserChoice.DimReferencedataIdAsUserChoiceLabelNavigation.NameFi,
                        NameEn = dimUserChoice.DimReferencedataIdAsUserChoiceLabelNavigation.NameEn,
                        NameSv = dimUserChoice.DimReferencedataIdAsUserChoiceLabelNavigation.NameSv,
                        Selected = dimUserChoice.UserChoiceValue
                    }
                );
            }

            // Save response in cache
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                // Keep in cache for this time, reset time if accessed.
                .SetSlidingExpiration(TimeSpan.FromSeconds(Constants.Cache.MEMORY_CACHE_EXPIRATION_SECONDS));

            // Save data in cache. Cache key is ORCID ID.
            _cache.Set(cacheKey, cooperationItems, cacheEntryOptions);

            return Ok(new ApiResponseCooperationGet(success: true, reason: "", data: cooperationItems, fromCache: false));
        }


        /// <summary>
        /// Modify cooperation selections.
        /// </summary>
        [HttpPatch]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> PatchMany([FromBody] List<ProfileEditorCooperationItem> profileEditorCooperationItems)
        {
            if (!ModelState.IsValid)
            {
                return Ok(new ApiResponse(success: false, reason: "invalid request data"));
            }

            // Return immediately if there is nothing to modify.
            if (profileEditorCooperationItems.Count == 0)
            {
                return Ok(new ApiResponse(success: false, reason: "nothing to modify"));
            }

            // Check that user profile exists.
            var orcidId = this.GetOrcidId();
            var userprofileId = await _userProfileService.GetUserprofileId(orcidId);
            if (userprofileId == -1)
            {
                return Ok(new ApiResponse(success: false, reason: "profile not found"));
            }

            // Remove cached profile data response. Cache key is ORCID ID + "_choices"
            _cache.Remove(orcidId + "_choices");

            // Save cooperation selections
            foreach (ProfileEditorCooperationItem profileEditorCooperationItem in profileEditorCooperationItems)
            {
                var dimUserChoice = await _ttvContext.DimUserChoices.Where(duc => duc.DimUserProfileId == userprofileId && duc.Id == profileEditorCooperationItem.Id).FirstOrDefaultAsync();
                if (dimUserChoice != null)
                {
                    dimUserChoice.UserChoiceValue = profileEditorCooperationItem.Selected;
                }
            }

            await _ttvContext.SaveChangesAsync();

            return Ok(new ApiResponse(success: true));
        }
    }
}