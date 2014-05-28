﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPMeta2.Definitions;

namespace SPMeta2.CSOM.Tests.Models
{
    public static class AppFolders
    {
        #region properties

        public static FolderDefinition Year2012 = new FolderDefinition
        {
            Name = "2012"
        };

        public static FolderDefinition Year2013 = new FolderDefinition
        {
            Name = "2013"
        };

        public static FolderDefinition Year2014 = new FolderDefinition
        {
            Name = "2014"
        };

        public static FolderDefinition January = new FolderDefinition
        {
            Name = "January"
        };

        public static FolderDefinition Febuary = new FolderDefinition
        {
            Name = "Febuary"
        };

        public static FolderDefinition March = new FolderDefinition
        {
            Name = "March"
        };

        #endregion
    }
}