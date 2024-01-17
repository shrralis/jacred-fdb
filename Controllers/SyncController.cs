﻿using System;
using System.Collections.Generic;
using System.Linq;
using JacRed.Engine;
using JacRed.Engine.CORE;
using JacRed.Models;
using JacRed.Models.Details;
using JacRed.Models.Sync.v2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace JacRed.Controllers
{
    public class SyncController : BaseController
    {
        [Route("/sync/conf")]
        public JsonResult Configuration()
        {
            return Json(new 
            {
                fbd = true,
                version = 2
            });
        }

        [Route("/sync/fdb")]
        public ActionResult FdbKey(string key)
        {
            if (!AppInit.conf.opensync)
                return Content("[]", "application/json; charset=utf-8");

            return Content(JsonConvert.SerializeObject(FileDB.masterDb.Where(i => i.Key.Contains(key)).Take(20).Select(i => new
            {
                i.Key,
                i.Value.updateTime,
                i.Value.fileTime,
                path = $"Data/fdb/{HashTo.md5(i.Key).Substring(0, 2)}/{HashTo.md5(i.Key).Substring(2)}",
                value = FileDB.OpenRead(i.Key)
            })), "application/json; charset=utf-8");
        }

        [Route("/sync/fdb/torrents")]
        public ActionResult FdbTorrents(long time)
        {
            if (!AppInit.conf.opensync || time == 0)
                return Json(new { nextread = false, collections = new List<Collection>() });

            bool nextread = false;
            int take = 7_000, countread = 0;
            var collections = new List<Collection>(take);

            if (!memoryCache.TryGetValue("sync:masterDb", out Dictionary<string, TorrentInfo> masterDb))
            {
                masterDb = FileDB.masterDb.OrderBy(i => i.Value.fileTime).ToDictionary(k => k.Key, v => v.Value);
                memoryCache.Set("sync:masterDb", masterDb, DateTime.Now.AddMinutes(10));
            }

            foreach (var item in masterDb.Where(i => i.Value.fileTime > time))
            {
                var torrent = new Dictionary<string, TorrentDetails>();

                foreach (var t in FileDB.OpenRead(item.Key))
                {
                    if (t.Value.ffprobe == null || t.Value.languages == null)
                    {
                        var _t = (TorrentDetails)t.Value.Clone();

                        var streams = TracksDB.Get(_t.magnet, _t.types);
                        if (streams != null)
                        {
                            _t.ffprobe = streams;
                            _t.languages = TracksDB.Languages(_t, streams);
                        }

                        torrent.TryAdd(t.Key, _t);
                    }
                    else
                    {
                        torrent.TryAdd(t.Key, t.Value);
                    }
                }

                countread = countread + torrent.Count;

                collections.Add(new Collection() 
                {
                    Key = item.Key,
                    Value = new Value() 
                    {
                        time = item.Value.updateTime,
                        fileTime = item.Value.fileTime,
                        torrents = torrent
                    }
                });

                if (countread > take)
                {
                    nextread = true;
                    break;
                }
            }

            return Json(new { nextread, collections });
        }


        [Route("/sync/torrents")]
        public JsonResult Torrents(long time)
        {
            if (!AppInit.conf.opensync_v1 || time == 0)
                return Json(new List<string>());

            int take = 10_000;
            var torrents = new Dictionary<string, TorrentDetails>();

            if (!memoryCache.TryGetValue("sync:masterDb", out Dictionary<string, TorrentInfo> masterDb))
            {
                masterDb = FileDB.masterDb.OrderBy(i => i.Value.fileTime).ToDictionary(k => k.Key, v => v.Value);
                memoryCache.Set("sync:masterDb", masterDb, DateTime.Now.AddMinutes(10));
            }

            foreach (var item in masterDb.Where(i => i.Value.fileTime > time))
            {
                foreach (var torrent in FileDB.OpenRead(item.Key))
                {
                    var _t = (TorrentDetails)torrent.Value.Clone();

                    var streams = TracksDB.Get(_t.magnet, _t.types);
                    if (streams != null)
                    {
                        _t.ffprobe = streams;
                        _t.languages = TracksDB.Languages(_t, streams);
                    }

                    if (torrents.TryGetValue(torrent.Key, out TorrentDetails val))
                    {
                        if (torrent.Value.updateTime > val.updateTime)
                            torrents[torrent.Key] = _t;
                    }
                    else
                    {
                        torrents.TryAdd(torrent.Key, _t);
                    }
                }

                if (torrents.Count > take)
                {
                    take = torrents.Count;
                    break;
                }
            }

            return Json(new { take, torrents = torrents.OrderBy(i => i.Value.updateTime) });
        }
    }
}
