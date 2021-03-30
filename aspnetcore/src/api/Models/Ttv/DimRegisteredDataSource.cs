﻿using System;
using System.Collections.Generic;

#nullable disable

namespace api.Models.Ttv
{
    public partial class DimRegisteredDataSource
    {
        public DimRegisteredDataSource()
        {
            BrFieldDisplaySettingsDimRegisteredDataSources = new HashSet<BrFieldDisplaySettingsDimRegisteredDataSource>();
        }

        public int Id { get; set; }
        public int DimOrganizationId { get; set; }
        public string Name { get; set; }
        public DateTime? Modified { get; set; }
        public DateTime? Created { get; set; }

        public virtual ICollection<BrFieldDisplaySettingsDimRegisteredDataSource> BrFieldDisplaySettingsDimRegisteredDataSources { get; set; }
    }
}
