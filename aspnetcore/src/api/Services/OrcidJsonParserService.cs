﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using api.Models.Common;
using api.Models.Orcid;

namespace api.Services
{
    /*
     * OrcidJsonParserService parses elements from user's ORCID json record.
     * 
     * ORCID record schema
     * https://info.orcid.org/documentation/integration-guide/orcid-record/
     */
    public class OrcidJsonParserService : IOrcidJsonParserService
    {
        /*
         * Put code
         */
        private OrcidPutCode GetOrcidPutCode(JsonElement orcidJsonElement)
        {
            OrcidPutCode putCode = new(null);

            // put code
            if (orcidJsonElement.TryGetProperty("put-code", out JsonElement putCodeElement))
            {
                if (putCodeElement.ValueKind == JsonValueKind.Number)
                {
                    putCodeElement.TryGetInt32(out Int32 putCodeParsed);
                    putCode.Value = putCodeParsed;
                }
            }

            return putCode;
        }

        /*
         * Date
         */
        private OrcidDate GetOrcidDate(JsonElement orcidJsonDateElement)
        {
            OrcidDate orcidDate = new();

            if (orcidJsonDateElement.ValueKind != JsonValueKind.Null)
            {
                // Year
                orcidJsonDateElement.TryGetProperty("year", out JsonElement yearElement);
                if (yearElement.ValueKind != JsonValueKind.Null)
                {
                    orcidDate.Year = int.Parse(yearElement.GetProperty("value").GetString());
                }

                // Month
                orcidJsonDateElement.TryGetProperty("month", out JsonElement monthElement);
                if (monthElement.ValueKind != JsonValueKind.Null)
                {
                    orcidDate.Month = int.Parse(monthElement.GetProperty("value").GetString());
                }

                // Day
                orcidJsonDateElement.TryGetProperty("day", out JsonElement dayElement);
                if (dayElement.ValueKind != JsonValueKind.Null)
                {
                    orcidDate.Day = int.Parse(dayElement.GetProperty("value").GetString());
                }
            }

            return orcidDate;
        }

        /*
         * Get DOI from ORCID publication
         */
        private string GetPublicationDoi(JsonElement externalIdsElement)
        {
            string doi = "";
            if (externalIdsElement.ValueKind != JsonValueKind.Null && externalIdsElement.TryGetProperty("external-id", out JsonElement externalIdElement))
            {
                foreach (JsonElement idElement in externalIdElement.EnumerateArray())
                {
                    if (idElement.GetProperty("external-id-type").GetString() == "doi")
                    {
                        doi = idElement.GetProperty("external-id-value").GetString();
                        break;
                    }
                }
            }
            return doi;
        }

        /*
         * Get publication year from ORCID publication
         */
        private int? GetPublicationYear(JsonElement workElement)
        {
            int? publicationYear = null;
            if (workElement.TryGetProperty("publication-date", out JsonElement publicationDateElement))
            {
                if (publicationDateElement.ValueKind != JsonValueKind.Null && publicationDateElement.TryGetProperty("year", out JsonElement yearElement))
                {
                    if (yearElement.ValueKind != JsonValueKind.Null && yearElement.TryGetProperty("value", out JsonElement valueElement))
                    {
                        publicationYear = Int32.Parse(valueElement.GetString());
                    }
                }
            }
            return publicationYear;
        }

        /*
         * Check if Json document is full ORCID record
         */
        private Boolean IsFullRecord(JsonDocument orcidJsonDocument)
        {
            //var myValue = new System.Text.Json.JsonElement();
            //return orcidJsonDocument.RootElement.TryGetProperty("person", out myValue);

            return orcidJsonDocument.RootElement.TryGetProperty("person", out _);
        }

        /*
         * Given names
         */
        public OrcidGivenNames GetGivenNames(String json)
        {
            using JsonDocument document = JsonDocument.Parse(json);

            // Get name element
            JsonElement nameElement = IsFullRecord(document)
                ? document.RootElement.GetProperty("person").GetProperty("name")
                : document.RootElement.GetProperty("name");

            JsonElement givenNamesElement;
            // Check if name element is null
            if (nameElement.ValueKind == JsonValueKind.Null)
            {
                return new OrcidGivenNames("");
            }
            else
            {
                givenNamesElement = nameElement.GetProperty("given-names");
            }

            // Get value
            if (givenNamesElement.ValueKind != JsonValueKind.Null && givenNamesElement.TryGetProperty("value", out JsonElement valueElement))
            {
                return new OrcidGivenNames(
                    valueElement.GetString()
                );
            }
            else
            {
                return new OrcidGivenNames("");
            }
        }

        /*
         * Family name
         */
        public OrcidFamilyName GetFamilyName(String json)
        {
            using JsonDocument document = JsonDocument.Parse(json);

            // Get name element
            JsonElement nameElement = IsFullRecord(document)
                ? document.RootElement.GetProperty("person").GetProperty("name")
                : document.RootElement.GetProperty("name");

            JsonElement familyNameElement;
            // Check if name element is null
            if (nameElement.ValueKind == JsonValueKind.Null)
            {
                return new OrcidFamilyName("");
            }
            else
            {
                familyNameElement = nameElement.GetProperty("family-name");
            }

            // Get value
            if (familyNameElement.ValueKind != JsonValueKind.Null && familyNameElement.TryGetProperty("value", out JsonElement valueElement))
            {
                return new OrcidFamilyName(
                    valueElement.GetString()
                );
            }
            else
            {
                return new OrcidFamilyName("");
            }
        }

        /*
         * Credit name
         */
        public OrcidCreditName GetCreditName(String json)
        {
            using JsonDocument document = JsonDocument.Parse(json);

            // Get name element
            JsonElement nameElement = IsFullRecord(document)
                ? document.RootElement.GetProperty("person").GetProperty("name")
                : document.RootElement.GetProperty("name");

            JsonElement creditNameElement;
            // Check if name element is null
            if (nameElement.ValueKind == JsonValueKind.Null)
            {
                return new OrcidCreditName("");
            }
            else
            {
                creditNameElement = nameElement.GetProperty("credit-name");
            }

            // Get value
            if (creditNameElement.ValueKind != JsonValueKind.Null && creditNameElement.TryGetProperty("value", out JsonElement valueElement))
            {
                return new OrcidCreditName(
                    valueElement.GetString()
                );
            }
            else
            {
                return new OrcidCreditName("");
            }
        }

        /*
         * Other names
         */
        public List<OrcidOtherName> GetOtherNames(String json)
        {
            List<OrcidOtherName> otherNames = new() { };
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                JsonElement otherNamesElement = IsFullRecord(document)
                    ? document.RootElement.GetProperty("person").GetProperty("other-names")
                    : document.RootElement.GetProperty("other-names");

                foreach (JsonElement element in otherNamesElement.GetProperty("other-name").EnumerateArray())
                {
                    string value = element.GetProperty("content").GetString();
                    OrcidPutCode putCode = this.GetOrcidPutCode(element);
                    otherNames.Add(
                        new OrcidOtherName(value, putCode)
                    );
                }
            }
            return otherNames;
        }

        /*
         * Biography
         */
        public OrcidBiography GetBiography(String json)
        {
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement biographyElement = IsFullRecord(document)
                ? document.RootElement.GetProperty("person").GetProperty("biography")
                : document.RootElement.GetProperty("biography");

            if (biographyElement.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            else
            {
                biographyElement.TryGetProperty("content", out JsonElement contentElement);

                if (contentElement.ValueKind.Equals(null))
                {
                    return null;
                }
                else
                {
                    return new OrcidBiography(
                        value: contentElement.GetString()
                    );
                }
            }
        }

        /*
         * Researcher urls
         */
        public List<OrcidResearcherUrl> GetResearcherUrls(String json)
        {
            List<OrcidResearcherUrl> urls = new() { };
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                foreach (JsonElement element in document.RootElement.GetProperty("person").GetProperty("researcher-urls").GetProperty("researcher-url").EnumerateArray())
                {
                    urls.Add(
                        new OrcidResearcherUrl(
                            urlName: element.GetProperty("url-name").GetString(),
                            url: element.GetProperty("url").GetProperty("value").GetString(),
                            putCode: this.GetOrcidPutCode(element)
                        )
                    );
                }
            }
            return urls;
        }

        /*
         * Emails
         */
        public List<OrcidEmail> GetEmails(String json)
        {
            List<OrcidEmail> emails = new() { };
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                foreach (JsonElement element in document.RootElement.GetProperty("person").GetProperty("emails").GetProperty("email").EnumerateArray())
                {
                    emails.Add(
                        new OrcidEmail(
                            value: element.GetProperty("email").GetString(),
                            putCode: this.GetOrcidPutCode(element)
                        )
                    );
                }
            }
            return emails;
        }

        /*
         * Keywords
         */
        public List<OrcidKeyword> GetKeywords(String json)
        {
            List<OrcidKeyword> keywords = new() { };
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                foreach (JsonElement element in document.RootElement.GetProperty("person").GetProperty("keywords").GetProperty("keyword").EnumerateArray())
                {
                    keywords.Add(
                        new OrcidKeyword(
                            value: element.GetProperty("content").GetString(),
                            putCode: this.GetOrcidPutCode(element)
                        )
                    );
                }
            }
            return keywords;
        }

        /*
         * External identifiers
         */
        public List<OrcidExternalIdentifier> GetExternalIdentifiers(String json)
        {
            List<OrcidExternalIdentifier> externalIdentifiers = new() { };
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                foreach (JsonElement element in document.RootElement.GetProperty("person").GetProperty("external-identifiers").GetProperty("external-identifier").EnumerateArray())
                {
                    externalIdentifiers.Add(
                        new OrcidExternalIdentifier(
                            externalIdType: element.GetProperty("external-id-type").GetString(),
                            externalIdValue: element.GetProperty("external-id-value").GetString(),
                            externalIdUrl: element.GetProperty("external-id-url").GetProperty("value").GetString(),
                            putCode: this.GetOrcidPutCode(element)
                        )
                    );
                }
            }
            return externalIdentifiers;
        }

        /*
         * Educations
         */
        public List<OrcidEducation> GetEducations(String json)
        {
            List<OrcidEducation> educations = new() { };
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                JsonElement educationsElement = document.RootElement.GetProperty("activities-summary").GetProperty("educations");

                if (educationsElement.TryGetProperty("affiliation-group", out JsonElement affiliationGroupsElement))
                {
                    foreach (JsonElement affiliationGroupElement in affiliationGroupsElement.EnumerateArray())
                    {

                        if (affiliationGroupElement.TryGetProperty("summaries", out JsonElement summariesElement))
                        {
                            foreach (JsonElement summaryElement in summariesElement.EnumerateArray())
                            {
                                if (summaryElement.TryGetProperty("education-summary", out JsonElement educationSummaryElement))
                                {
                                    string disambiguatedOrganizationIdentifier = "";
                                    string disambiguationSource = "";
                                    if (educationSummaryElement.GetProperty("organization").TryGetProperty("disambiguated-organization", out JsonElement disambiguatedOrganizationElement))
                                    {
                                        if (disambiguatedOrganizationElement.ValueKind != JsonValueKind.Null)
                                        {
                                            disambiguatedOrganizationIdentifier = disambiguatedOrganizationElement.GetProperty("disambiguated-organization-identifier").GetString();
                                            disambiguationSource = disambiguatedOrganizationElement.GetProperty("disambiguation-source").GetString();
                                        }
                                    }

                                    educations.Add(
                                        new OrcidEducation(
                                            organizationName: educationSummaryElement.GetProperty("organization").GetProperty("name").GetString(),
                                            departmentName: educationSummaryElement.GetProperty("department-name").GetString(),
                                            roleTitle: educationSummaryElement.GetProperty("role-title").GetString(),
                                            startDate: GetOrcidDate(educationSummaryElement.GetProperty("start-date")),
                                            endDate: GetOrcidDate(educationSummaryElement.GetProperty("end-date")),
                                            putCode: this.GetOrcidPutCode(educationSummaryElement)
                                        )
                                    );
                                }
                            }
                        }
                    }
                }
            }
            return educations;
        }

        /*
         * Employments
         */
        public List<OrcidEmployment> GetEmployments(String json)
        {
            List<OrcidEmployment> employments = new() { };
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                JsonElement employmentsElement = document.RootElement.GetProperty("activities-summary").GetProperty("employments");

                if (employmentsElement.TryGetProperty("affiliation-group", out JsonElement affiliationGroupsElement))
                {
                    foreach (JsonElement affiliationGroupElement in affiliationGroupsElement.EnumerateArray())
                    {

                        if (affiliationGroupElement.TryGetProperty("summaries", out JsonElement summariesElement))
                        {
                            foreach (JsonElement summaryElement in summariesElement.EnumerateArray())
                            {
                                if (summaryElement.TryGetProperty("employment-summary", out JsonElement employmentSummaryElement))
                                {
                                    string disambiguatedOrganizationIdentifier = "";
                                    string disambiguationSource = "";
                                    if (employmentSummaryElement.GetProperty("organization").TryGetProperty("disambiguated-organization", out JsonElement disambiguatedOrganizationElement))
                                    {
                                        if (disambiguatedOrganizationElement.ValueKind != JsonValueKind.Null)
                                        {
                                            disambiguatedOrganizationIdentifier = disambiguatedOrganizationElement.GetProperty("disambiguated-organization-identifier").GetString();
                                            disambiguationSource = disambiguatedOrganizationElement.GetProperty("disambiguation-source").GetString();
                                        }
                                    }

                                    employments.Add(
                                      new OrcidEmployment(
                                          organizationName: employmentSummaryElement.GetProperty("organization").GetProperty("name").GetString(),
                                          disambiguatedOrganizationIdentifier: disambiguatedOrganizationIdentifier,
                                          disambiguationSource: disambiguationSource,
                                          departmentName: employmentSummaryElement.GetProperty("department-name").GetString(),
                                          roleTitle: employmentSummaryElement.GetProperty("role-title").GetString(),
                                          startDate: GetOrcidDate(employmentSummaryElement.GetProperty("start-date")),
                                          endDate: GetOrcidDate(employmentSummaryElement.GetProperty("end-date")),
                                          putCode: this.GetOrcidPutCode(employmentSummaryElement)
                                      )
                                  );
                                }
                            }
                        }
                    }
                }
            }
            return employments;
        }

        /*
         * Publications
         */
        public List<OrcidPublication> GetPublications(String json)
        {
            List<OrcidPublication> publications = new() { };
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                JsonElement publicationsElement = document.RootElement.GetProperty("activities-summary").GetProperty("works");

                if (publicationsElement.TryGetProperty("group", out JsonElement groupsElement))
                {
                    foreach (JsonElement groupElement in groupsElement.EnumerateArray())
                    {
                        /*
                         *  Elements in "group" can contain "external-ids" and "work-summary".
                         *  "work-summary" can contain multiple entries of the same publication.
                         *  Get DOI from "external-ids" and the other properties from the first element in "work-summary".
                         */
                        string DOI = "";

                        // Get publication DOI from "external-ids" array.
                        if (groupElement.TryGetProperty("external-ids", out JsonElement externalIdsElement))
                        {
                            DOI = GetPublicationDoi(externalIdsElement);
                        }

                        // Get publication properties from "work-summary" array.
                        if (groupElement.TryGetProperty("work-summary", out JsonElement workSummariesElement))
                        {
                            foreach (JsonElement workElement in workSummariesElement.EnumerateArray())
                            {
                                publications.Add(
                                    new OrcidPublication()
                                    {
                                        PublicationName = workElement.GetProperty("title").GetProperty("title").GetProperty("value").GetString(),
                                        Doi = DOI,
                                        PublicationYear = this.GetPublicationYear(workElement),
                                        Type = workElement.GetProperty("type").GetString(),
                                        PutCode = this.GetOrcidPutCode(workElement)
                                    }
                                );

                                // Import only one element from "work-summary" array.
                                break;
                            }
                        }
                    }
                }
            }
            return publications;
        }

        /*
         * Distinctions, invited positions, peer reviews, memberships, qualifications and services.
         * Map to profile only research activity in TTV database.
         */
        public List<OrcidResearchActivity> GetProfileOnlyResearchActivityItems(String json)
        {
            List<OrcidResearchActivity> profileOnlyResearchActivityItems = new() { };
           
            using (JsonDocument document = JsonDocument.Parse(json))
            {       
                // Distinctions
                JsonElement distinctionsElement = document.RootElement.GetProperty("activities-summary").GetProperty("distinctions");
                if (distinctionsElement.TryGetProperty("affiliation-group", out JsonElement affiliationGroupsElement_distinctions))
                {
                    foreach (JsonElement affiliationGroupElement in affiliationGroupsElement_distinctions.EnumerateArray())
                    {
                        if (affiliationGroupElement.TryGetProperty("summaries", out JsonElement summariesElement))
                        {
                            foreach (JsonElement summaryElement in summariesElement.EnumerateArray())
                            {
                                if (summaryElement.TryGetProperty("distinction-summary", out JsonElement distinctionSummaryElement))
                                {
                                    string disambiguatedOrganizationIdentifier = "";
                                    string disambiguationSource = "";
                                    if (distinctionSummaryElement.GetProperty("organization").TryGetProperty("disambiguated-organization", out JsonElement disambiguatedOrganizationElement))
                                    {
                                        if (disambiguatedOrganizationElement.ValueKind != JsonValueKind.Null)
                                        {
                                            disambiguatedOrganizationIdentifier = disambiguatedOrganizationElement.GetProperty("disambiguated-organization-identifier").GetString();
                                            disambiguationSource = disambiguatedOrganizationElement.GetProperty("disambiguation-source").GetString();
                                        }
                                    }

                                    string url = (distinctionSummaryElement.GetProperty("url").ValueKind == JsonValueKind.Null) ?
                                        "" : distinctionSummaryElement.GetProperty("url").GetProperty("value").GetString();

                                    profileOnlyResearchActivityItems.Add(
                                      new OrcidResearchActivity(
                                          orcidActivityType: Constants.OrcidResearchActivityTypes.DISTINCTION,
                                          organizationName: distinctionSummaryElement.GetProperty("organization").GetProperty("name").GetString(),
                                          disambiguatedOrganizationIdentifier: disambiguatedOrganizationIdentifier,
                                          disambiguationSource: disambiguationSource,
                                          departmentName: distinctionSummaryElement.GetProperty("department-name").GetString(),
                                          roleTitle: distinctionSummaryElement.GetProperty("role-title").GetString(),
                                          startDate: GetOrcidDate(distinctionSummaryElement.GetProperty("start-date")),
                                          endDate: GetOrcidDate(distinctionSummaryElement.GetProperty("end-date")),
                                          putCode: this.GetOrcidPutCode(distinctionSummaryElement),
                                          url: url
                                      )
                                  );
                                }
                            }
                        }
                    }
                }

                // Invited positions
                JsonElement invitedPositionsElement = document.RootElement.GetProperty("activities-summary").GetProperty("invited-positions");
                if (invitedPositionsElement.TryGetProperty("affiliation-group", out JsonElement affiliationGroupsElement_invitedPositions))
                {
                    foreach (JsonElement affiliationGroupElement in affiliationGroupsElement_invitedPositions.EnumerateArray())
                    {
                        if (affiliationGroupElement.TryGetProperty("summaries", out JsonElement summariesElement))
                        {
                            foreach (JsonElement summaryElement in summariesElement.EnumerateArray())
                            {
                                if (summaryElement.TryGetProperty("invited-position-summary", out JsonElement invitedPositionsSummaryElement))
                                {
                                    string disambiguatedOrganizationIdentifier = "";
                                    string disambiguationSource = "";
                                    if (invitedPositionsSummaryElement.GetProperty("organization").TryGetProperty("disambiguated-organization", out JsonElement disambiguatedOrganizationElement))
                                    {
                                        if (disambiguatedOrganizationElement.ValueKind != JsonValueKind.Null)
                                        {
                                            disambiguatedOrganizationIdentifier = disambiguatedOrganizationElement.GetProperty("disambiguated-organization-identifier").GetString();
                                            disambiguationSource = disambiguatedOrganizationElement.GetProperty("disambiguation-source").GetString();
                                        }
                                    }

                                    string url = (invitedPositionsSummaryElement.GetProperty("url").ValueKind == JsonValueKind.Null) ?
                                        "" : invitedPositionsSummaryElement.GetProperty("url").GetProperty("value").GetString();

                                    profileOnlyResearchActivityItems.Add(
                                      new OrcidResearchActivity(
                                          orcidActivityType: Constants.OrcidResearchActivityTypes.INVITED_POSITION,
                                          organizationName: invitedPositionsSummaryElement.GetProperty("organization").GetProperty("name").GetString(),
                                          disambiguatedOrganizationIdentifier: disambiguatedOrganizationIdentifier,
                                          disambiguationSource: disambiguationSource,
                                          departmentName: invitedPositionsSummaryElement.GetProperty("department-name").GetString(),
                                          roleTitle: invitedPositionsSummaryElement.GetProperty("role-title").GetString(),
                                          startDate: GetOrcidDate(invitedPositionsSummaryElement.GetProperty("start-date")),
                                          endDate: GetOrcidDate(invitedPositionsSummaryElement.GetProperty("end-date")),
                                          putCode: this.GetOrcidPutCode(invitedPositionsSummaryElement),
                                          url: url
                                      )
                                  );
                                }
                            }
                        }
                    }
                }

                // Memberships
                JsonElement membershipsElement = document.RootElement.GetProperty("activities-summary").GetProperty("memberships");
                if (membershipsElement.TryGetProperty("affiliation-group", out JsonElement affiliationGroupsElement_memberships))
                {
                    foreach (JsonElement affiliationGroupElement in affiliationGroupsElement_memberships.EnumerateArray())
                    {
                        if (affiliationGroupElement.TryGetProperty("summaries", out JsonElement summariesElement))
                        {
                            foreach (JsonElement summaryElement in summariesElement.EnumerateArray())
                            {
                                if (summaryElement.TryGetProperty("membership-summary", out JsonElement membershipSummaryElement))
                                {
                                    string disambiguatedOrganizationIdentifier = "";
                                    string disambiguationSource = "";
                                    if (membershipSummaryElement.GetProperty("organization").TryGetProperty("disambiguated-organization", out JsonElement disambiguatedOrganizationElement))
                                    {
                                        if (disambiguatedOrganizationElement.ValueKind != JsonValueKind.Null)
                                        {
                                            disambiguatedOrganizationIdentifier = disambiguatedOrganizationElement.GetProperty("disambiguated-organization-identifier").GetString();
                                            disambiguationSource = disambiguatedOrganizationElement.GetProperty("disambiguation-source").GetString();
                                        }
                                    }

                                    string url = (membershipSummaryElement.GetProperty("url").ValueKind == JsonValueKind.Null) ?
                                        "" : membershipSummaryElement.GetProperty("url").GetProperty("value").GetString();

                                    profileOnlyResearchActivityItems.Add(
                                      new OrcidResearchActivity(
                                          orcidActivityType: Constants.OrcidResearchActivityTypes.MEMBERSHIP,
                                          organizationName: membershipSummaryElement.GetProperty("organization").GetProperty("name").GetString(),
                                          disambiguatedOrganizationIdentifier: disambiguatedOrganizationIdentifier,
                                          disambiguationSource: disambiguationSource,
                                          departmentName: membershipSummaryElement.GetProperty("department-name").GetString(),
                                          roleTitle: membershipSummaryElement.GetProperty("role-title").GetString(),
                                          startDate: GetOrcidDate(membershipSummaryElement.GetProperty("start-date")),
                                          endDate: GetOrcidDate(membershipSummaryElement.GetProperty("end-date")),
                                          putCode: this.GetOrcidPutCode(membershipSummaryElement),
                                          url: url
                                      )
                                  );
                                }
                            }
                        }
                    }
                }

                // Peer reviews
                JsonElement peerReviewsElement = document.RootElement.GetProperty("activities-summary").GetProperty("peer-reviews");
                if (peerReviewsElement.TryGetProperty("group", out JsonElement groupsElement)) {
                    foreach (JsonElement groupElement in groupsElement.EnumerateArray())
                    {
                        if (groupElement.TryGetProperty("peer-review-group", out JsonElement peerReviewGroupsElement))
                        {
                            foreach (JsonElement peerReviewGroupElement in peerReviewGroupsElement.EnumerateArray())
                            {
                                if (peerReviewGroupElement.TryGetProperty("peer-review-summary", out JsonElement peerReviewSummariesElement))
                                {
                                    foreach (JsonElement peerReviewSummaryElement in peerReviewSummariesElement.EnumerateArray())
                                    {
                                            string disambiguatedOrganizationIdentifier = "";
                                            string disambiguationSource = "";
                                            if (peerReviewSummaryElement.GetProperty("convening-organization").TryGetProperty("disambiguated-organization", out JsonElement disambiguatedOrganizationElement))
                                            {
                                                if (disambiguatedOrganizationElement.ValueKind != JsonValueKind.Null)
                                                {
                                                    disambiguatedOrganizationIdentifier = disambiguatedOrganizationElement.GetProperty("disambiguated-organization-identifier").GetString();
                                                    disambiguationSource = disambiguatedOrganizationElement.GetProperty("disambiguation-source").GetString();
                                                }
                                            }

                                            string url = (peerReviewSummaryElement.GetProperty("review-url").ValueKind == JsonValueKind.Null) ?
                                                "" : peerReviewSummaryElement.GetProperty("review-url").GetProperty("value").GetString();

                                        profileOnlyResearchActivityItems.Add(
                                              new OrcidResearchActivity(
                                                  orcidActivityType: Constants.OrcidResearchActivityTypes.PEER_REVIEW,
                                                  organizationName: peerReviewSummaryElement.GetProperty("convening-organization").GetProperty("name").GetString(),
                                                  disambiguatedOrganizationIdentifier: disambiguatedOrganizationIdentifier,
                                                  disambiguationSource: disambiguationSource,
                                                  departmentName: "",
                                                  roleTitle: peerReviewSummaryElement.GetProperty("reviewer-role").GetString(),
                                                  startDate: GetOrcidDate(peerReviewSummaryElement.GetProperty("completion-date")),
                                                  endDate: new OrcidDate(), // TODO: Convert to nullable
                                                  putCode: this.GetOrcidPutCode(peerReviewSummaryElement),
                                                  url: url
                                              )
                                          );
                                    }
                                }
                            }
                        }
                    }
                }

                // Qualifications
                JsonElement qualificationsElement = document.RootElement.GetProperty("activities-summary").GetProperty("qualifications");
                if (qualificationsElement.TryGetProperty("affiliation-group", out JsonElement affiliationGroupsElement_qualifications))
                {
                    foreach (JsonElement affiliationGroupElement in affiliationGroupsElement_qualifications.EnumerateArray())
                    {
                        if (affiliationGroupElement.TryGetProperty("summaries", out JsonElement summariesElement))
                        {
                            foreach (JsonElement summaryElement in summariesElement.EnumerateArray())
                            {
                                if (summaryElement.TryGetProperty("qualification-summary", out JsonElement qualificationSummaryElement))
                                {
                                    string disambiguatedOrganizationIdentifier = "";
                                    string disambiguationSource = "";
                                    if (qualificationSummaryElement.GetProperty("organization").TryGetProperty("disambiguated-organization", out JsonElement disambiguatedOrganizationElement))
                                    {
                                        if (disambiguatedOrganizationElement.ValueKind != JsonValueKind.Null)
                                        {
                                            disambiguatedOrganizationIdentifier = disambiguatedOrganizationElement.GetProperty("disambiguated-organization-identifier").GetString();
                                            disambiguationSource = disambiguatedOrganizationElement.GetProperty("disambiguation-source").GetString();
                                        }
                                    }

                                    string url = (qualificationSummaryElement.GetProperty("url").ValueKind == JsonValueKind.Null) ?
                                        "" : qualificationSummaryElement.GetProperty("url").GetProperty("value").GetString();

                                    profileOnlyResearchActivityItems.Add(
                                      new OrcidResearchActivity(
                                          orcidActivityType: Constants.OrcidResearchActivityTypes.QUALIFICATION,
                                          organizationName: qualificationSummaryElement.GetProperty("organization").GetProperty("name").GetString(),
                                          disambiguatedOrganizationIdentifier: disambiguatedOrganizationIdentifier,
                                          disambiguationSource: disambiguationSource,
                                          departmentName: qualificationSummaryElement.GetProperty("department-name").GetString(),
                                          roleTitle: qualificationSummaryElement.GetProperty("role-title").GetString(),
                                          startDate: GetOrcidDate(qualificationSummaryElement.GetProperty("start-date")),
                                          endDate: GetOrcidDate(qualificationSummaryElement.GetProperty("end-date")),
                                          putCode: this.GetOrcidPutCode(qualificationSummaryElement),
                                          url: url
                                      )
                                  );
                                }
                            }
                        }
                    }
                }

                // Services
                JsonElement servicesElement = document.RootElement.GetProperty("activities-summary").GetProperty("services");
                if (servicesElement.TryGetProperty("affiliation-group", out JsonElement affiliationGroupsElement_services))
                {
                    foreach (JsonElement affiliationGroupElement in affiliationGroupsElement_services.EnumerateArray())
                    {
                        if (affiliationGroupElement.TryGetProperty("summaries", out JsonElement summariesElement))
                        {
                            foreach (JsonElement summaryElement in summariesElement.EnumerateArray())
                            {
                                if (summaryElement.TryGetProperty("service-summary", out JsonElement serviceSummaryElement))
                                {
                                    string disambiguatedOrganizationIdentifier = "";
                                    string disambiguationSource = "";
                                    if (serviceSummaryElement.GetProperty("organization").TryGetProperty("disambiguated-organization", out JsonElement disambiguatedOrganizationElement))
                                    {
                                        if (disambiguatedOrganizationElement.ValueKind != JsonValueKind.Null)
                                        {
                                            disambiguatedOrganizationIdentifier = disambiguatedOrganizationElement.GetProperty("disambiguated-organization-identifier").GetString();
                                            disambiguationSource = disambiguatedOrganizationElement.GetProperty("disambiguation-source").GetString();
                                        }
                                    }

                                    string url = (serviceSummaryElement.GetProperty("url").ValueKind == JsonValueKind.Null) ?
                                        "" : serviceSummaryElement.GetProperty("url").GetProperty("value").GetString();

                                    profileOnlyResearchActivityItems.Add(
                                      new OrcidResearchActivity(
                                          orcidActivityType: Constants.OrcidResearchActivityTypes.SERVICE,
                                          organizationName: serviceSummaryElement.GetProperty("organization").GetProperty("name").GetString(),
                                          disambiguatedOrganizationIdentifier: disambiguatedOrganizationIdentifier,
                                          disambiguationSource: disambiguationSource,
                                          departmentName: serviceSummaryElement.GetProperty("department-name").GetString(),
                                          roleTitle: serviceSummaryElement.GetProperty("role-title").GetString(),
                                          startDate: GetOrcidDate(serviceSummaryElement.GetProperty("start-date")),
                                          endDate: GetOrcidDate(serviceSummaryElement.GetProperty("end-date")),
                                          putCode: this.GetOrcidPutCode(serviceSummaryElement),
                                          url: url
                                      )
                                  );
                                }
                            }
                        }
                    }
                }
            }
            return profileOnlyResearchActivityItems;
        }
    }
}