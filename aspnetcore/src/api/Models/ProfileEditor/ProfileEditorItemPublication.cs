﻿using System;
using System.Collections.Generic;
using api.Models;

namespace api.Models
{
    public partial class ProfileEditorItemPublication : ProfileEditorItem
    {
        public ProfileEditorItemPublication()
        {
            PublicationId = "";
            PublicationName = "";
            PublicationYear = null;
            DoiHandle = "";
        }

        public string PublicationId { get; set; }
        public string PublicationName { get; set; }
        public int? PublicationYear { get; set; }
        public string DoiHandle { get; set; }
    }
}
