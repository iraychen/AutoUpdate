using AutoUpdateServer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Model
{
    [Serializable]
    public class RoleModel
    {
        [PrimaryKey]
        public int ID { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public string ParentName { get; set; }

        [NotNull]
        public string Permission { get; set; }
    }
}