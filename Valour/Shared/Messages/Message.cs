﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

/*  Valour - A free and secure chat client
 *  Copyright (C) 2021 Vooper Media LLC
 *  This program is subject to the GNU Affero General Public license
 *  A copy of the license should be included - if not, see <http://www.gnu.org/licenses/>
 */

namespace Valour.Shared.Messages
{
    public class Message
    {

        /// <summary>
        /// The Id of the message
        /// </summary>
        [Key]
        public ulong Id { get; set; }

        /// <summary>
        /// The user's ID
        /// </summary>
        public ulong Author_Id { get; set; }

        /// <summary>
        /// String representation of message
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The time the message was sent (in UTC)
        /// </summary>
        public DateTime TimeSent { get; set; }

        /// <summary>
        /// Id of the channel this message belonged to
        /// </summary>
        public ulong Channel_Id { get; set; }

        /// <summary>
        /// Index of the message
        /// </summary>
        public ulong Message_Index { get; set; }

        /// <summary>
        /// Data for representing an embed
        /// </summary>
        public string Embed_Data { get; set; }

        /// <summary>
        /// Data for representing mentions in a message
        /// </summary>
        public string Mentions_Data { get; set; }

        /// <summary>
        /// Used to identify a message returned from the server 
        /// </summary>
        [JsonProperty]
        [System.Text.Json.Serialization.JsonInclude]
        public string Fingerprint;

        /// <summary>
        /// Returns the hash for a message. Cannot be used in browser/client!
        /// </summary>
        public byte[] GetHash()
        {
            using (SHA256 sha = SHA256.Create())
            {
                string conc = $"{Author_Id}{Content}{TimeSent}{Channel_Id}";

                byte[] buffer = Encoding.Unicode.GetBytes(conc);

                return sha.ComputeHash(buffer);
            }
        }
        /// <summary>
        /// Returns true if the message is a embed
        /// </summary>
        public bool IsEmbed()
        {
            if (Embed_Data != null) {
                return true;
            }
            else {
                return false;
            }
        }
    }
}
