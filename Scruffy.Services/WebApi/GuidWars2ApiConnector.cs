﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Scruffy.Data.Json.GuildWars2.Account;
using Scruffy.Data.Json.GuildWars2.Characters;
using Scruffy.Data.Json.GuildWars2.Core;
using Scruffy.Data.Json.GuildWars2.Guild;
using Scruffy.Data.Json.GuildWars2.Items;
using Scruffy.Data.Json.GuildWars2.Quaggans;
using Scruffy.Data.Json.GuildWars2.TradingPost;
using Scruffy.Data.Json.GuildWars2.Upgrades;
using Scruffy.Data.Json.GuildWars2.World;

namespace Scruffy.Services.WebApi
{
    /// <summary>
    /// Accessing the Guild Wars 2 WEB API
    /// </summary>
    public sealed class GuidWars2ApiConnector : IAsyncDisposable,
                                                IDisposable
    {
        /// <summary>
        /// Lock
        /// </summary>
        private static object _lock = new object();

        /// <summary>
        /// Current minute
        /// </summary>
        private static Stopwatch _stopWatch = Stopwatch.StartNew();

        /// <summary>
        /// Current count
        /// </summary>
        private static int _currentCount;

        #region Fields

        /// <summary>
        /// Api key
        /// </summary>
        private string _apiKey;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiKey">Api Key</param>
        public GuidWars2ApiConnector(string apiKey)
        {
            _apiKey = apiKey;
        }

        #endregion // Constructor

        #region IAsyncDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public async ValueTask DisposeAsync()
        {
            await Task.Run(Dispose)
                      .ConfigureAwait(false);
        }

        #endregion // IAsyncDisposable

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion // IDisposable

        #region Methods

        /// <summary>
        /// Request the token information
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<TokenInformation> GetTokenInformationAsync()
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest("https://api.guildwars2.com/v2/tokeninfo")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<TokenInformation>(jsonResult);
                                  }
                              }
                          });
        }

        /// <summary>
        /// Request the account information
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<AccountInformation> GetAccountInformationAsync()
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest("https://api.guildwars2.com/v2/account?v=latest")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<AccountInformation>(jsonResult);
                                  }
                              }
                          });
        }

        /// <summary>
        /// Request the characters information
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<Character>> GetCharactersAsync()
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest("https://api.guildwars2.com/v2/characters?ids=all")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<List<Character>>(jsonResult)
                                                        .Where(obj => obj.Flags?.Contains("Beta") != true)
                                                        .ToList();
                                  }
                              }
                          });
        }

        /// <summary>
        /// Request the character names
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<string>> GetCharacterNamesAsync()
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest("https://api.guildwars2.com/v2/characters")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<List<string>>(jsonResult);
                                  }
                              }
                          });
        }

        /// <summary>
        /// Request the character information
        /// </summary>
        /// <param name="characterName">Character name</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Character> GetCharacterAsync(string characterName)
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest($"https://api.guildwars2.com/v2/characters/{Uri.EscapeUriString(characterName)}?v=latest")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<Character>(jsonResult);
                                  }
                              }
                          });
        }

        /// <summary>
        /// Request the guild information
        /// </summary>
        /// <param name="id">Id of the guild</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<GuildInformation> GetGuildInformation(string id)
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest($"https://api.guildwars2.com/v2/guild/{id}?v=latest")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<GuildInformation>(jsonResult);
                                  }
                              }
                          });
        }

        /// <summary>
        /// Request the guild log
        /// </summary>
        /// <param name="guildId">Id of the log</param>
        /// <param name="sinceId">Since id</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<GuildLogEntry>> GetGuildLogEntries(string guildId, long sinceId)
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest($"https://api.guildwars2.com/v2/guild/{guildId}/log?since={sinceId}?v=latest")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<List<GuildLogEntry>>(jsonResult);
                                  }
                              }
                          });
        }

        /// <summary>
        /// Request all available guild emblem foregrounds
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<long>> GetGuildEmblemForegrounds()
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest("https://api.guildwars2.com/v2/emblem/foregrounds")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<List<long>>(jsonResult);
                                  }
                              }
                          });
        }

        /// <summary>
        /// Request all available guild emblem backgrounds
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<long>> GetGuildEmblemBackgrounds()
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest("https://api.guildwars2.com/v2/emblem/backgrounds")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<List<long>>(jsonResult);
                                  }
                              }
                          });
        }

        /// <summary>
        /// Request the layers of a guild emblem background
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<GuildEmblemLayerData> GetGuildEmblemBackgroundLayer(long id)
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest($"https://api.guildwars2.com/v2/emblem/backgrounds?ids={id}")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<List<GuildEmblemLayerData>>(jsonResult)
                                                        .FirstOrDefault();
                                  }
                              }
                          });
        }

        /// <summary>
        /// Request the layers of a guild emblem foreground
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<GuildEmblemLayerData> GetGuildEmblemForegroundLayer(long id)
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest($"https://api.guildwars2.com/v2/emblem/foregrounds?ids={id}")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<List<GuildEmblemLayerData>>(jsonResult)
                                                        .FirstOrDefault();
                                  }
                              }
                          });
        }

        /// <summary>
        /// Request the guild stash
        /// </summary>
        /// <param name="guildId">Id of the guild</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<GuildStash>> GetGuildVault(string guildId)
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest($"https://api.guildwars2.com/v2/guild/{guildId}/stash")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<List<GuildStash>>(jsonResult);
                                  }
                              }
                          });
        }

        /// <summary>
        /// Request the list of all items
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<int>> GetAllItemIds()
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest("https://api.guildwars2.com/v2/items")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<List<int>>(jsonResult);
                                  }
                              }
                          });
        }

        /// <summary>
        /// Request the item data
        /// </summary>
        /// <param name="itemId">Id of the item</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Item> GetItem(int itemId)
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest($"https://api.guildwars2.com/v2/items/{itemId}")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<Item>(jsonResult);
                                  }
                              }
                          });
        }

        /// <summary>
        /// Get items
        /// </summary>
        /// <param name="itemIds">Item ids</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<Item>> GetItems(List<int?> itemIds)
        {
            return Invoke(async () =>
                          {
                              var pageCount = (int)Math.Ceiling(itemIds.Count / 200.0);

                              var items = new List<Item>();

                              for (var i = 0; i < pageCount; i++)
                              {
                                  var ids = string.Join(",", itemIds.Skip(i * 200).Take(200).Select(obj => obj.ToString()));

                                  using (var response = await CreateRequest("https://api.guildwars2.com/v2/items?ids=" + ids)
                                                              .GetResponseAsync()
                                                              .ConfigureAwait(false))
                                  {
                                      using (var reader = new StreamReader(response.GetResponseStream()))
                                      {
                                          var jsonResult = await reader.ReadToEndAsync()
                                                                       .ConfigureAwait(false);

                                          items.AddRange(JsonConvert.DeserializeObject<List<Item>>(jsonResult));
                                      }
                                  }
                              }

                              return items;
                          });
        }

        /// <summary>
        /// Get upgrades
        /// </summary>
        /// <param name="upgradeIds">Upgrade ids</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<Upgrade>> GetUpgrades(List<int?> upgradeIds)
        {
            return Invoke(async () =>
                          {
                              var pageCount = (int)Math.Ceiling(upgradeIds.Count / 200.0);

                              var upgrades = new List<Upgrade>();

                              for (var i = 0; i < pageCount; i++)
                              {
                                  var ids = string.Join(",", upgradeIds.Skip(i * 200).Take(200).Select(obj => obj.ToString()));

                                  using (var response = await CreateRequest("https://api.guildwars2.com/v2/guild/upgrades?ids=" + ids)
                                                              .GetResponseAsync()
                                                              .ConfigureAwait(false))
                                  {
                                      using (var reader = new StreamReader(response.GetResponseStream()))
                                      {
                                          var jsonResult = await reader.ReadToEndAsync()
                                                                       .ConfigureAwait(false);

                                          upgrades.AddRange(JsonConvert.DeserializeObject<List<Upgrade>>(jsonResult));
                                      }
                                  }
                              }

                              return upgrades;
                          });
        }

        /// <summary>
        /// Request the list of quaggans
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<string>> GetQuaggans()
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest("https://api.guildwars2.com/v2/quaggans")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<List<string>>(jsonResult);
                                  }
                              }
                          });
        }

        /// <summary>
        /// Request the quaggan data
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<QuagganData> GetQuaggan(string name)
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest($"https://api.guildwars2.com/v2/quaggans/{name}")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<QuagganData>(jsonResult);
                                  }
                              }
                          });
        }

        /// <summary>
        /// Request the worlds
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<WorldData>> GetWorlds()
        {
            return Invoke(async () =>
                          {
                              using (var response = await CreateRequest("https://api.guildwars2.com/v2/worlds?ids=all")
                                                          .GetResponseAsync()
                                                          .ConfigureAwait(false))
                              {
                                  using (var reader = new StreamReader(response.GetResponseStream()))
                                  {
                                      var jsonResult = await reader.ReadToEndAsync()
                                                                   .ConfigureAwait(false);

                                      return JsonConvert.DeserializeObject<List<WorldData>>(jsonResult);
                                  }
                              }
                          });
        }

        /// <summary>
        /// Get trading post values
        /// </summary>
        /// <param name="itemIds">Item ids</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<TradingPostItemPrice>> GetTradingPostPrices(List<int?> itemIds)
        {
            return Invoke(async () =>
                          {
                              var pageCount = (int)Math.Ceiling(itemIds.Count / 200.0);

                              var prices = new List<TradingPostItemPrice>();

                              for (var i = 0; i < pageCount; i++)
                              {
                                  var ids = string.Join(",", itemIds.Skip(i * 200).Take(200).Select(obj => obj.ToString()));

                                  try
                                  {
                                      using (var response = await CreateRequest("https://api.guildwars2.com/v2/commerce/prices?ids=" + ids)
                                                                  .GetResponseAsync()
                                                                  .ConfigureAwait(false))
                                      {
                                          using (var reader = new StreamReader(response.GetResponseStream()))
                                          {
                                              var jsonResult = await reader.ReadToEndAsync()
                                                                           .ConfigureAwait(false);

                                              prices.AddRange(JsonConvert.DeserializeObject<List<TradingPostItemPrice>>(jsonResult));
                                          }
                                      }
                                  }
                                  catch (WebException ex) when (ex.Response is HttpWebResponse { StatusCode: HttpStatusCode.NotFound })
                                  {
                                  }
                              }

                              return prices;
                          });
        }

        /// <summary>
        /// Invoke the web api
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="func">Func</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task<T> Invoke<T>(Func<Task<T>> func)
        {
            await CheckRateLimit()
                .ConfigureAwait(false);

            try
            {
                return await func()
                           .ConfigureAwait(false);
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse response && response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(5000)
                          .ConfigureAwait(false);

                return await func()
                           .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Checking the rate limit
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static Task CheckRateLimit()
        {
            return Task.Run(() =>
                            {
                                lock (_lock)
                                {
                                    if (_stopWatch.ElapsedMilliseconds >= 60_000)
                                    {
                                        _stopWatch.Restart();
                                        _currentCount = 0;
                                    }
                                    else
                                    {
                                        _currentCount++;

                                        if (_currentCount >= 300)
                                        {
                                            Thread.Sleep(TimeSpan.FromMinutes(1));
                                        }
                                    }
                                }
                            });
        }

        /// <summary>
        /// Create a new request
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns>The created request</returns>
        private HttpWebRequest CreateRequest(string uri)
        {
            var request = WebRequest.CreateHttp(uri);

            if (_apiKey != null)
            {
                request.Headers.Add("Authorization", "Bearer " + _apiKey);
            }

            return request;
        }

        #endregion // Methods
    }
}