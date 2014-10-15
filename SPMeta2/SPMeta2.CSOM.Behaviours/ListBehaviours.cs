using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;

namespace SPMeta2.CSOM.Behaviours
{
    public static class ListBehaviours
    {
        #region common extensions

        public static List MakeFolderCreationAvailable(this List list, bool available)
        {
            list.EnableFolderCreation = available;

            return list;
        }

        #endregion
    }
}
