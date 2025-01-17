﻿using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Valour.Client.Channels;
using Valour.Shared;
using Valour.Shared.Channels;
using Valour.Shared.Planets;
using Valour.Shared.Categories;
using Valour.Shared.Roles;
using Valour.Client.Categories;

namespace Valour.Client.Planets
{
    /*  Valour - A free and secure chat client
     *  Copyright (C) 2021 Vooper Media LLC
     *  This program is subject to the GNU Affero General Public license
     *  A copy of the license should be included - if not, see <http://www.gnu.org/licenses/>
     */

    /// <summary>
    /// This class exists to add client funtionality to the Planet
    /// class. It does not, and should not, have any extra fields or properties.
    /// Just helper methods.
    /// </summary>
    public class ClientPlanet : Planet 
    {

        /// <summary>
        /// Returns the generic planet object
        /// </summary>
        public Planet Planet
        {
            get
            {
                return (Planet)this;
            }
        }

        // Cached values
        private List<ClientPlanetChatChannel> _channels = null;
        private List<ClientPlanetCategory> _categories = null;

        /// <summary>
        /// Returns a ServerPlanet using a Planet as a base
        /// </summary>
        public static ClientPlanet FromBase(Planet planet, IMapper mapper)
        {
            return mapper.Map<ClientPlanet>(planet);
        }

        public async Task NotifyUpdateChannel(ClientPlanetChatChannel channel)
        {
            if (_channels == null)
            {
                await RequestChannelsAsync();
            }

            int index = _channels.FindIndex(x => x.Id == channel.Id);

            if (index == -1) {
                // add to cache
                _channels.Add(channel);
            }
            else {
                // replace
                _channels.RemoveAt(index);
                _channels.Insert(index, channel);
            }
        }

        public void NotifyDeleteChannel(ClientPlanetChatChannel channel)
        {
            int index = _channels.FindIndex(x => x.Id == channel.Id);

            if (index != -1)
            {
                _channels.RemoveAt(index);
            }
        }

        public async Task NotifyUpdateCategory(ClientPlanetCategory category)
        {
            if (_categories == null)
            {
                await RequestCategoriesAsync();
            }

            int index = _categories.FindIndex(x => x.Id == category.Id);

            if (index == -1)
            {
                // add to cache
                _categories.Add(category);
            }
            else
            {
                // replace
                _categories.RemoveAt(index);
                _categories.Insert(index, category);
            }
        }

        public void NotifyDeleteCategory(ClientPlanetCategory category)
        {
            int index = _categories.FindIndex(x => x.Id == category.Id);

            if (index != -1)
            {
                _categories.RemoveAt(index);
            }
        }

        /// <summary>
        /// Returns the primary channel of the planet
        /// </summary>
        public async Task<ClientPlanetChatChannel> GetPrimaryChannelAsync(IMapper mapper)
        {
            string json = await ClientUserManager.Http.GetStringAsync($"Planet/GetPrimaryChannel?planet_id={Id}" +
                                                                                              $"&user_id={ClientUserManager.User.Id}" +
                                                                                              $"&token={ClientUserManager.UserSecretToken}");

            TaskResult<PlanetChatChannel> channelResult = JsonConvert.DeserializeObject<TaskResult<PlanetChatChannel>>(json);

            if (channelResult == null)
            {
                Console.WriteLine($"Failed to retrieve primary channel for planet {Id}.");
                return null;
            }

            if (!channelResult.Success)
            {
                Console.WriteLine($"Failed to retrieve primary channel for planet {Id}: {channelResult.Message}");
            }

            // Map to new
            ClientPlanetChatChannel channel = mapper.Map<ClientPlanetChatChannel>(channelResult.Data);

            return channel;
        }

        /// <summary>
        /// Retrieves and returns categories of a planet by requesting from the server
        /// </summary>
        public async Task<List<ClientPlanetCategory>> GetCategoriesAsync(bool force_refresh = false)
        {

            if (_categories == null || force_refresh)
            {
                await RequestCategoriesAsync();
            }

            return _categories;
        }

        /// <summary>
        /// Requests and caches categories from the server
        /// </summary>
        public async Task RequestCategoriesAsync()
        {
            string json = await ClientUserManager.Http.GetStringAsync($"Category/GetPlanetCategories?planet_id={Id}" +
                                                                                                  $"&token={ClientUserManager.UserSecretToken}");

            TaskResult<List<ClientPlanetCategory>> result = JsonConvert.DeserializeObject<TaskResult<List<ClientPlanetCategory>>>(json);

            if (result == null)
            {
                Console.WriteLine("A fatal error occurred retrieving a planet from the server.");
            }

            if (!result.Success)
            {
                Console.WriteLine(result.ToString());
            }

            _categories = result.Data;
        }

        /// <summary>
        /// Retrieves and returns channels of a planet by requesting from the server
        /// </summary>
        public async Task<List<ClientPlanetChatChannel>> GetChannelsAsync(bool force_refresh = false)
        {

            if (_channels == null || force_refresh)
            {
                await RequestChannelsAsync();
            }

            return _channels;
        }

        /// <summary>
        /// Requests and caches channels from the server
        /// </summary>
        public async Task RequestChannelsAsync()
        {
            string json = await ClientUserManager.Http.GetStringAsync($"Channel/GetPlanetChannels?planet_id={Id}" +
                                                                                   $"&token={ClientUserManager.UserSecretToken}");

            TaskResult<List<ClientPlanetChatChannel>> result = JsonConvert.DeserializeObject<TaskResult<List<ClientPlanetChatChannel>>>(json);

            if (result == null)
            {
                Console.WriteLine("A fatal error occurred retrieving a planet from the server.");
            }

            if (!result.Success)
            {
                Console.WriteLine(result.ToString());
            }

            _channels = result.Data;
        }

        /// <summary>
        /// Attempts to set the name of the planet
        /// </summary>
        public async Task<TaskResult> SetName(string name)
        {
            string json = await ClientUserManager.Http.GetStringAsync($"Planet/SetName?planet_id={Id}&name={name}&token={ClientUserManager.UserSecretToken}");

            TaskResult result = JsonConvert.DeserializeObject<TaskResult>(json);

            if (result.Success)
            {
                Name = name;
            }

            return result;
        }

        /// <summary>
        /// Attempts to set the description of the planet
        /// </summary>
        public async Task<TaskResult> SetDescription(string description)
        {
            string json = await ClientUserManager.Http.GetStringAsync($"Planet/SetDescription?planet_id={Id}&description={description}&token={ClientUserManager.UserSecretToken}");

            TaskResult result = JsonConvert.DeserializeObject<TaskResult>(json);

            if (result.Success)
            {
                Description = description;
            }

            return result;
        }

        /// <summary>
        /// Attempts to set the public value of the planet
        /// </summary>
        public async Task<TaskResult> SetPublic(bool ispublic)
        {
            string json = await ClientUserManager.Http.GetStringAsync($"Planet/SetPublic?planet_id={Id}&ispublic={ispublic}&token={ClientUserManager.UserSecretToken}");

            TaskResult result = JsonConvert.DeserializeObject<TaskResult>(json);

            if (result.Success)
            {
                Public = ispublic;
            }

            return result;
        }

        /// <summary>
        /// Retrieves and returns a client planet by requesting from the server
        /// </summary>
        public static async Task<ClientPlanet> GetClientPlanetAsync(ulong id)
        {
            string json = await ClientUserManager.Http.GetStringAsync($"Planet/GetPlanet?planet_id={id}&auth={ClientUserManager.UserSecretToken}");

            TaskResult<ClientPlanet> result = JsonConvert.DeserializeObject<TaskResult<ClientPlanet>>(json);

            if (result == null)
            {
                Console.WriteLine("A fatal error occurred retrieving a planet from the server.");
                return null;
            }

            if (!result.Success)
            {
                Console.WriteLine(result.ToString());
                return null;
            }

            return result.Data;
        }

        /// <summary>
        /// Deserializes json
        /// </summary>
        public static ClientPlanet Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<ClientPlanet>(json);
        }

        /// <summary>
        /// Returns every planet member
        /// </summary>
        public async Task<List<ClientPlanetMember>> GetCachedMembers()
        {
            return await ClientPlanetManager.Current.GetCachedPlanetMembers(this);

            //return await ClientPlanetManager.Current.GetPlanetMemberInfoAsync(Id);
        }

        /// <summary>
        /// Returns every planet role
        /// </summary>
        public async Task<List<PlanetRole>> GetRolesAsync()
        {
            return await ClientPlanetManager.Current.GetPlanetRoles(Id);
        }

        /// <summary>
        /// Returns the member for a given user id
        /// </summary>
        public async Task<ClientPlanetMember> GetMemberAsync(ulong user_id)
        {
            return await ClientPlanetMember.GetClientPlanetMemberAsync(user_id, Id);
        }
    }
}
