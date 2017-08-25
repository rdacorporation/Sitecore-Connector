﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web;
using Brightcove.MediaFramework.Brightcove.Entities;
using Sitecore.Data.Items;
using Sitecore.MediaFramework.Brightcove;
using Sitecore.MediaFramework.Entities;
using VideoSearchResult = Brightcove.MediaFramework.Brightcove.Indexing.Entities.VideoSearchResult;

namespace Brightcove.MediaFramework.Brightcove.Synchronize
{
    public class VideoSynchronizer : AssetSynchronizer
    {
        public override Item SyncItem(object entity, Item accountItem)
        {
            Entities.ItemState? itemState = ((Video)entity).ItemState;
            if ((itemState.GetValueOrDefault() != Entities.ItemState.INACTIVE ? 0 : (itemState.HasValue ? 1 : 0)) != 0)
                return (Item)null;
            return base.SyncItem(entity, accountItem);
        }

        public override Item UpdateItem(object entity, Item accountItem, Item item)
        {
            Video video = (Video)entity;
            using (new EditContext(item))
            {
                item.Name = ItemUtil.ProposeValidItemName(video.Name);
                item[Sitecore.MediaFramework.Brightcove.FieldIDs.MediaElement.Id] = video.Id;
                item[Sitecore.MediaFramework.Brightcove.FieldIDs.MediaElement.Name] = video.Name;
                item[Sitecore.MediaFramework.Brightcove.FieldIDs.MediaElement.ReferenceId] = video.ReferenceId;
                item[Sitecore.MediaFramework.Brightcove.FieldIDs.MediaElement.ThumbnailUrl] = (video.Images != null && video.Images.Thumbnail != null) ? video.Images.Thumbnail.Src : null;
                item[Sitecore.MediaFramework.Brightcove.FieldIDs.MediaElement.ShortDescription] = video.ShortDescription;
                item[Sitecore.MediaFramework.Brightcove.FieldIDs.Video.CreationDate] = video.CreationDate.ToString();
                item[Sitecore.MediaFramework.Brightcove.FieldIDs.Video.LongDescription] = video.LongDescription;
                item[Sitecore.MediaFramework.Brightcove.FieldIDs.Video.PublishedDate] = video.PublishedDate.ToString();
                item[Sitecore.MediaFramework.Brightcove.FieldIDs.Video.LastModifiedDate] = video.LastModifiedDate.ToString();
                item[Sitecore.MediaFramework.Brightcove.FieldIDs.Video.Economics] = video.Economics.ToString();
                item[Sitecore.MediaFramework.Brightcove.FieldIDs.Video.LinkUrl] = video.Link != null ? video.Link.URL : null;
                item[Sitecore.MediaFramework.Brightcove.FieldIDs.Video.LinkText] = video.Link != null ? video.Link.Text : null;
                item[Sitecore.MediaFramework.Brightcove.FieldIDs.Video.VideoStillUrl] = (video.Images != null && video.Images.Poster != null) ? video.Images.Poster.Src : null;
                item[Sitecore.MediaFramework.Brightcove.FieldIDs.Video.CustomFields] = this.GetCustomFields(video);
                item[FieldIDs.Video.Duration] = video.Duration.HasValue ? video.Duration.ToString() : string.Empty;
                item[FieldIDs.Video.IngestJobId] = video.IngestJobId;

                // Sharing section starts
                if (video.Sharing != null)
                {
                    item[FieldIDs.Video.ByExternalAcct] = video.Sharing.ByExternalAcct ? "1" : "0";
                    item[FieldIDs.Video.ById] = video.Sharing.ById;
                    item[FieldIDs.Video.SourceId] = video.Sharing.SourceId;
                    item[FieldIDs.Video.ToExternalAcct] = video.Sharing.ToExternalAcct ? "1" : "0";
                    item[FieldIDs.Video.ByReference] = video.Sharing.ByReference ? "1" : "0";
                }
            }
            return item;
        }

        public override bool NeedUpdate(object entity, Item accountItem, MediaServiceSearchResult searchResult)
        {
            Video video = (Video)entity;
            VideoSearchResult videoSearchResult = (VideoSearchResult)searchResult;
            var thumbnailSrc = video.Images.Thumbnail != null ? video.Images.Thumbnail.Src : null;
            var posterSrc = video.Images.Poster != null ? video.Images.Poster.Src : null;
            if (Sitecore.Integration.Common.Utils.StringUtil.EqualsIgnoreNullEmpty(video.LastModifiedDate.ToString(), videoSearchResult.LastModifiedDate) && (Sitecore.Integration.Common.Utils.StringUtil.EqualsIgnoreNullEmpty(thumbnailSrc, videoSearchResult.ThumbnailUrl)))
                return !Sitecore.Integration.Common.Utils.StringUtil.EqualsIgnoreNullEmpty(posterSrc, videoSearchResult.VideoStillURL);
            return true;
        }

        public override MediaServiceSearchResult GetSearchResult(object entity, Item accountItem)
        {
            Video video = (Video)entity;
            return (MediaServiceSearchResult)this.GetSearchResult<VideoSearchResult>(Sitecore.MediaFramework.Brightcove.Constants.IndexName, accountItem, (Expression<Func<VideoSearchResult, bool>>)(i => i.TemplateId == TemplateIDs.Video && i.Id == video.Id));
        }

        public override MediaServiceEntityData GetMediaData(object entity)
        {
            MediaServiceEntityData mediaData = base.GetMediaData(entity);
            mediaData.TemplateId = TemplateIDs.Video;
            return mediaData;
        }

        protected virtual string GetCustomFields(Video video)
        {
            if (video.CustomFields == null)
                return (string)null;
            return HttpUtility.UrlPathEncode(Sitecore.Integration.Common.Utils.StringUtil.DictionaryToString(video.CustomFields as Dictionary<string, string>, '=', '&'));
        }
    }
}