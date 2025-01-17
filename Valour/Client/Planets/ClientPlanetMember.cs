﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Valour.Client.Mapping;
using Valour.Shared;
using Valour.Shared.Planets;
using Valour.Shared.Roles;
using Valour.Shared.Users;

/*  Valour - A free and secure chat client
*  Copyright (C) 2021 Vooper Media LLC
*  This program is subject to the GNU Affero General Public license
*  A copy of the license should be included - if not, see <http://www.gnu.org/licenses/>
*/

namespace Valour.Client.Planets
{
    public class ClientPlanetMember : PlanetMember
    {
        // Client cached properties //

        /// <summary>
        /// Cached roles
        /// </summary>
        private List<ulong> _roleids = null;

        /// <summary>
        /// Cached status
        /// </summary>
        private string _status = null;

        /// <summary>
        /// Cached user
        /// </summary>
        private User _user = null;

        public void RemoveRoleId(ulong RoleId)
        {
            if (_roleids != null)
            {
                _roleids.Remove(RoleId);
            }
        }

        public ulong TryGetPrimaryRoleId()
        {
            if (_roleids == null)
            {
                return ulong.MaxValue;
            }

            return _roleids[0];
        }

        public async Task<List<ulong>> GetRoleIdsAsync()
        {
            if (_roleids == null)
            {
                await LoadRoleIdsAsync();
            }

            return _roleids;
        }
        public void SetCacheValues(PlanetMemberInfo info)
        {
            SetCacheValues(info.RoleIds.ToList(), info.State, info.User);
        }
        public void SetCacheValues(List<ulong> role_ids, string state, User user)
        {
            _roleids = role_ids;
            _status = state;
            _user = user;
        }

        public async Task<bool> HasRoleAsync(ulong role_id)
        {
            if (_roleids == null)
            {
                await LoadRoleIdsAsync();
            }

            return _roleids.Contains(role_id);
        }

        public async Task<List<PlanetRole>> GetPlanetRolesAsync()
        {
            if (_roleids == null)
            {
                await LoadRoleIdsAsync();
            }

            List<PlanetRole> roles = new List<PlanetRole>();

            foreach (ulong id in _roleids)
            {
                roles.Add(await ClientPlanetManager.Current.GetPlanetRole(id));
            }

            return roles;
        }

        public async Task<string> GetStatusAsync()
        {
            if (_status == null)
            {
                await LoadStatusAsync();
            }

            return _status;
        }

        public async Task<User> GetUserAsync()
        {
            if (_user == null)
            {
                await LoadUserAsync();
            }

            return _user;
        }

        /// <summary>
        /// Returns generic planet member object
        /// </summary>
        public PlanetMember GetPlanetMember()
        {
            return (PlanetMember)this;
        }

        /// <summary>
        /// Returns the client version from the base
        /// </summary>
        public static ClientPlanetMember FromBase(PlanetMember member)
        {
            return MappingManager.Mapper.Map<ClientPlanetMember>(member);
        }

        /// <summary>
        /// Returns a planet member
        /// </summary>
        public static async Task<ClientPlanetMember> GetClientPlanetMemberAsync(ulong user_id, ulong planet_id)
        {
            return await ClientPlanetManager.Current.GetPlanetMemberAsync(user_id, planet_id);
        }

        /// <summary>
        /// Returns a planet member given their id
        /// </summary>
        public static async Task<ClientPlanetMember> FindAsync(ulong member_id)
        {
            return await ClientPlanetManager.Current.GetPlanetMemberAsync(member_id);
        }

        /// <summary>
        /// Loads the user into cache, or reloads them if called again
        /// </summary>
        /// <returns></returns>
        public async Task LoadUserAsync()
        {
            string json = await ClientUserManager.Http.GetStringAsync($"User/GetUser?id={User_Id}");

            TaskResult<User> result = JsonConvert.DeserializeObject<TaskResult<User>>(json);

            if (result == null)
            {
                Console.WriteLine("A fatal error occurred retrieving a user from the server.");
                return;
            }

            if (!result.Success)
            {
                Console.WriteLine(result.ToString());
                return;
            }

            _user = result.Data;
        }

        /// <summary>
        /// Loads the user roles into cache, or reloads them if called again
        /// </summary>
        public async Task LoadRoleIdsAsync()
        {
            string json = await ClientUserManager.Http.GetStringAsync($"Planet/GetPlanetMemberRoleIds?user_id={User_Id}&planet_id={Planet_Id}&token={ClientUserManager.UserSecretToken}");

            Console.WriteLine(json);

            TaskResult<List<ulong>> result = JsonConvert.DeserializeObject<TaskResult<List<ulong>>>(json);

            if (result == null)
            {
                Console.WriteLine("A fatal error occurred retrieving planet user roles from the server.");
            }

            if (!result.Success)
            {
                Console.WriteLine(result.ToString());
                Console.WriteLine($"Failed for {Id} in {Planet_Id}");
            }

            _roleids = result.Data;
        }

        public async Task<ulong> GetAuthorityAsync()
        {
            string json = await ClientUserManager.Http.GetStringAsync($"Planet/GetMemberAuthority?member_id={Id}&token={ClientUserManager.UserSecretToken}");

            Console.WriteLine($"Got authority for {Id}: " + json);

            TaskResult<ulong> result = JsonConvert.DeserializeObject<TaskResult<ulong>>(json);

            if (result == null)
            {
                Console.WriteLine("A fatal error occurred retrieving member authority from the server.");
            }

            if (!result.Success)
            {
                Console.WriteLine(result.ToString());
                Console.WriteLine($"Failed for {Id} in {Planet_Id}");
            }

            return result.Data;
        }

        /// <summary>
        /// Loads the current user state from the server
        /// </summary>
        public async Task LoadStatusAsync()
        {
            // TODO: Make work
            _status = "Currently browsing";
        }

        /// <summary>
        /// Returns the top role for the planet user
        /// </summary>
        public async Task<PlanetRole> GetPrimaryRoleAsync()
        {
            if (_roleids == null)
            {
                await LoadRoleIdsAsync();
            }

            //Console.WriteLine($"PlanetManager null: {ClientPlanetManager.Current == null}");
            //Console.WriteLine($"Roleids null: {_roleids == null}");
            //Console.WriteLine($"Role null: {_roleids[0] == null}");

            return await ClientPlanetManager.Current.GetPlanetRole(_roleids[0]);
        }

        /// <summary>
        /// Returns a hex color code for the main role color
        /// </summary>
        public async Task<string> GetColorHexAsync()
        {
            return (await GetPrimaryRoleAsync()).GetColorHex();
        }

        /// <summary>
        /// Returns the member's pfp
        /// </summary>
        public async Task<string> GetPfpAsync()
        {
            if (!string.IsNullOrWhiteSpace(Member_Pfp))
            {
                return Member_Pfp;
            }

            return (await GetUserAsync()).Pfp_Url;
        }

        /// <summary>
        /// Returns the member's name
        /// </summary>
        public async Task<string> GetNameAsync()
        {
            if (!string.IsNullOrWhiteSpace(Nickname))
            {
                return Nickname;
            }

            return (await GetUserAsync()).Username;
        }

        /// <summary>
        /// Deserializes json
        /// </summary>
        public static ClientPlanetMember Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<ClientPlanetMember>(json);
        }
    }
}
