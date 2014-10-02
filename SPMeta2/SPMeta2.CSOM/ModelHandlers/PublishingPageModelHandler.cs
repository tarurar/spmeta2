﻿using System;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Client;
using SPMeta2.CSOM.Extensions;
using SPMeta2.CSOM.ModelHosts;
using SPMeta2.Definitions;
using SPMeta2.ModelHandlers;
using SPMeta2.Utils;
using SPMeta2.Common;
using SPMeta2.Enumerations;
using SPMeta2.ModelHosts;

namespace SPMeta2.CSOM.ModelHandlers
{
    public class PublishingPageModelHandler : CSOMModelHandlerBase
    {
        #region properties

        public override Type TargetType
        {
            get { return typeof(PublishingPageDefinition); }
        }

        #endregion

        #region methods

        public override void WithResolvingModelHost(object modelHost, DefinitionBase model, Type childModelType, Action<object> action)
        {
            var folderModelHost = modelHost as FolderModelHost;
            var pageDefinition = model as PublishingPageDefinition;

            Folder folder = folderModelHost.CurrentLibraryFolder;

            if (folder != null && pageDefinition != null)
            {
                var context = folder.Context;
                var currentPage = GetCurrentPage(folder, GetSafePageFileName(pageDefinition));

                var currentListItem = currentPage.ListItemAllFields;
                context.Load(currentListItem);
                context.ExecuteQuery();

                if (childModelType == typeof(WebPartDefinition))
                {
                    var listItemHost = ModelHostBase.Inherit<ListItemModelHost>(folderModelHost, itemHost =>
                    {
                        itemHost.HostListItem = currentListItem;
                    });

                    action(listItemHost);

                    //currentListItem.Update();
                }

                //context.ExecuteQuery();
            }
            else
            {
                action(modelHost);
            }
        }

        protected string GetSafePageFileName(PageDefinitionBase page)
        {
            var fileName = page.FileName;
            if (!fileName.EndsWith(".aspx")) fileName += ".aspx";

            return fileName;
        }


        protected File GetCurrentPage(Folder folder, string pageName)
        {
            var context = folder.Context;

            var files = folder.Files;
            context.Load(files);
            context.ExecuteQuery();

            foreach (var file in files)
            {
                if (file.Name.ToUpper() == pageName.ToUpper())
                    return file;
            }

            return null;
        }

        protected ListItem FindPublishingPage(Folder folder, PublishingPageDefinition definition)
        {
            var pageName = GetSafePageFileName(definition);
            var file = GetCurrentPage(folder, pageName);

            if (file != null)
                return file.ListItemAllFields;

            return null;
        }

        public override void DeployModel(object modelHost, DefinitionBase model)
        {
            var folderModelHost = modelHost.WithAssertAndCast<FolderModelHost>("modelHost", value => value.RequireNotNull());

            var folder = folderModelHost.CurrentLibraryFolder;
            var list = folderModelHost.CurrentList;

            var publishingPageModel = model.WithAssertAndCast<PublishingPageDefinition>("model", value => value.RequireNotNull());

            var context = folder.Context;

            var pageName = GetSafePageFileName(publishingPageModel);
            var currentPageFile = GetCurrentPage(folder, pageName);

            InvokeOnModelEvent(this, new ModelEventArgs
            {
                CurrentModelNode = null,
                Model = null,
                EventType = ModelEventType.OnProvisioning,
                Object = currentPageFile,
                ObjectType = typeof(File),
                ObjectDefinition = publishingPageModel,
                ModelHost = modelHost
            });

            ModuleFileModelHandler.WithSafeFileOperation(list, currentPageFile, f =>
            {
                var file = new FileCreationInformation();
                var pageContent = PublishingPageTemplates.RedirectionPageMarkup;

                file.Url = pageName;
                file.Content = Encoding.UTF8.GetBytes(pageContent);
                file.Overwrite = publishingPageModel.NeedOverride;

                return folder.Files.Add(file);

            },
            newFile =>
            {
                var newFileItem = newFile.ListItemAllFields;
                context.Load(newFileItem);
                context.ExecuteQuery();

                var site = folderModelHost.HostSite;
                var currentPageLayoutItem = FindPageLayoutItem(site, publishingPageModel.PageLayoutFileName);

                var currentPageLayoutItemContext = currentPageLayoutItem.Context;
                var publishingFile = currentPageLayoutItem.File;

                currentPageLayoutItemContext.Load(currentPageLayoutItem);
                currentPageLayoutItemContext.Load(currentPageLayoutItem, i => i.DisplayName);
                currentPageLayoutItemContext.Load(publishingFile);

                currentPageLayoutItemContext.ExecuteQuery();

                newFileItem["Title"] = publishingPageModel.Title;
                newFileItem["Comments"] = publishingPageModel.Description;

                newFileItem["PublishingPageLayout"] = publishingFile.ServerRelativeUrl + ", " + currentPageLayoutItem.DisplayName;
                newFileItem["ContentTypeId"] = currentPageLayoutItem["PublishingAssociatedContentType"];

                newFileItem.Update();

                context.ExecuteQuery();
            });

            currentPageFile = GetCurrentPage(folder, pageName);

            InvokeOnModelEvent(this, new ModelEventArgs
            {
                CurrentModelNode = null,
                Model = null,
                EventType = ModelEventType.OnProvisioned,
                Object = currentPageFile,
                ObjectType = typeof(File),
                ObjectDefinition = publishingPageModel,
                ModelHost = modelHost
            });

            context.ExecuteQuery();
        }

        private ListItem FindPageLayoutItem(Site site, string pageLayoutFileName)
        {
            ListItem currentPageLayoutItem = null;

            var pageLayoutContentType = BuiltInPublishingContentTypeId.PageLayout.ToUpper();


            var rootWeb = site.RootWeb;
            var layoutsList = rootWeb.GetCatalog((int)ListTemplateType.MasterPageCatalog);

            // TODO, performance
            var pageLayouts = layoutsList.GetItems(CamlQuery.CreateAllItemsQuery());
            var context = layoutsList.Context;

            context.Load(pageLayouts);
            context.ExecuteQuery();

            var tmpPageLayouts = pageLayouts.ToList()
                                            .Where(i => i["ContentTypeId"].ToString().ToUpper().StartsWith(pageLayoutContentType));

            foreach (var pageLayout in tmpPageLayouts)
            {
                if (pageLayout["FileLeafRef"].ToString().ToUpper() == pageLayoutFileName.ToUpper())
                {
                    currentPageLayoutItem = pageLayout;
                    break;
                }
            }

            return currentPageLayoutItem;
        }

        #endregion
    }
}
