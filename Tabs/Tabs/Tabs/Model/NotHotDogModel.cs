using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tabs.Model
{
        public class NotHotDogModel
        {
            [JsonProperty(PropertyName = "Id")]
            public string ID { get; set; }

            [JsonProperty(PropertyName = "Tag")]
            public string Tag { get; set; }

        }
    }
