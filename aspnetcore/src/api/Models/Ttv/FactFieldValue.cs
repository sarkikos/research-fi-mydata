﻿using System;
using System.Collections.Generic;

namespace api.Models.Ttv
{
    public partial class FactFieldValue
    {
        public int DimUserProfileId { get; set; }
        public int DimFieldDisplaySettingsId { get; set; }
        public int DimNameId { get; set; }
        public int DimWebLinkId { get; set; }
        public int DimFundingDecisionId { get; set; }
        public int DimPublicationId { get; set; }
        public int DimPidId { get; set; }
        public int DimPidIdOrcidPutCode { get; set; }
        public int DimResearchActivityId { get; set; }
        public int DimEventId { get; set; }
        public int DimEducationId { get; set; }
        public int DimCompetenceId { get; set; }
        public int DimResearchCommunityId { get; set; }
        public int DimTelephoneNumberId { get; set; }
        public int DimEmailAddrressId { get; set; }
        public int DimResearcherDescriptionId { get; set; }
        public int DimIdentifierlessDataId { get; set; }
        public int DimProfileOnlyPublicationId { get; set; }
        public int DimKeywordId { get; set; }
        public int DimAffiliationId { get; set; }
        public int DimResearcherToResearchCommunityId { get; set; }
        public bool? Show { get; set; }
        public bool? PrimaryValue { get; set; }
        public string SourceId { get; set; }
        public string SourceDescription { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public int DimResearchDatasetId { get; set; }
        public int DimRegisteredDataSourceId { get; set; }
        public int DimReferencedataFieldOfScienceId { get; set; }
        public int DimProfileOnlyResearchActivityId { get; set; }
        public int DimReferencedataActorRoleId { get; set; }
        public int DimProfileOnlyDatasetId { get; set; }
        public int DimProfileOnlyFundingDecisionId { get; set; }

        public virtual DimAffiliation DimAffiliation { get; set; }
        public virtual DimCompetence DimCompetence { get; set; }
        public virtual DimEducation DimEducation { get; set; }
        public virtual DimEmailAddrress DimEmailAddrress { get; set; }
        public virtual DimEvent DimEvent { get; set; }
        public virtual DimFieldDisplaySetting DimFieldDisplaySettings { get; set; }
        public virtual DimFundingDecision DimFundingDecision { get; set; }
        public virtual DimIdentifierlessDatum DimIdentifierlessData { get; set; }
        public virtual DimKeyword DimKeyword { get; set; }
        public virtual DimName DimName { get; set; }
        public virtual DimPid DimPid { get; set; }
        public virtual DimPid DimPidIdOrcidPutCodeNavigation { get; set; }
        public virtual DimProfileOnlyDataset DimProfileOnlyDataset { get; set; }
        public virtual DimProfileOnlyFundingDecision DimProfileOnlyFundingDecision { get; set; }
        public virtual DimProfileOnlyPublication DimProfileOnlyPublication { get; set; }
        public virtual DimProfileOnlyResearchActivity DimProfileOnlyResearchActivity { get; set; }
        public virtual DimPublication DimPublication { get; set; }
        public virtual DimReferencedatum DimReferencedataActorRole { get; set; }
        public virtual DimReferencedatum DimReferencedataFieldOfScience { get; set; }
        public virtual DimRegisteredDataSource DimRegisteredDataSource { get; set; }
        public virtual DimResearchActivity DimResearchActivity { get; set; }
        public virtual DimResearchCommunity DimResearchCommunity { get; set; }
        public virtual DimResearchDataset DimResearchDataset { get; set; }
        public virtual DimResearcherDescription DimResearcherDescription { get; set; }
        public virtual DimResearcherToResearchCommunity DimResearcherToResearchCommunity { get; set; }
        public virtual DimTelephoneNumber DimTelephoneNumber { get; set; }
        public virtual DimUserProfile DimUserProfile { get; set; }
        public virtual DimWebLink DimWebLink { get; set; }
    }
}
