using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;

namespace SPMeta2.CSOM.Behaviours
{
    public static class ListViewBehaviours
    {
        #region common extensions

        public static View MakeFoldersInvisible(this View view)
        {
            view.Scope = ViewScope.Recursive;
            view.Aggregations = "Off";

            return view;
        }

        public static View MakeHidden(this View view)
        {
            view.Hidden = true;

            return view;
        }
        #endregion
    }
}
