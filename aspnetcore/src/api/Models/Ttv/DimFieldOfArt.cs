﻿using System;
using System.Collections.Generic;

namespace api.Models.Ttv
{
    public partial class DimFieldOfArt
    {
        public DimFieldOfArt()
        {
            DimFundingDecisions = new HashSet<DimFundingDecision>();
            DimPublications = new HashSet<DimPublication>();
        }

        public int Id { get; set; }
        public string FieldId { get; set; }
        public string NameFi { get; set; }
        public string NameEn { get; set; }
        public string NameSv { get; set; }
        public string SourceId { get; set; }
        public string SourceDescription { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }

        public virtual ICollection<DimFundingDecision> DimFundingDecisions { get; set; }
        public virtual ICollection<DimPublication> DimPublications { get; set; }
    }
}
