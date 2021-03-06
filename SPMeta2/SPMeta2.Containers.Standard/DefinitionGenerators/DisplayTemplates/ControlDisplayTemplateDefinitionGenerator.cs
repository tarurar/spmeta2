﻿using System;
using System.Text;
using SPMeta2.BuiltInDefinitions;
using SPMeta2.Containers.Services.Base;
using SPMeta2.Definitions;
using SPMeta2.Standard.Definitions;
using SPMeta2.Standard.Definitions.DisplayTemplates;
using SPMeta2.Syntax.Default;

namespace SPMeta2.Containers.Standard.DefinitionGenerators.DisplayTemplates
{
    public class ControlDisplayTemplateDefinitionGenerator : TypedDefinitionGeneratorServiceBase<ControlDisplayTemplateDefinition>
    {
        public override DefinitionBase GenerateRandomDefinition(Action<DefinitionBase> action)
        {
            return WithEmptyDefinition(def =>
            {
                def.FileName = string.Format("{0}.html", Rnd.String());
                def.Title = Rnd.String();

                def.Content = Rnd.Content();
            });
        }

        public override DefinitionBase GetCustomParenHost()
        {
            return BuiltInListDefinitions.Calalogs.MasterPage.Inherit<ListDefinition>(def =>
            {
                def.RequireSelfProcessing = false;
            });
        }
    }
}
