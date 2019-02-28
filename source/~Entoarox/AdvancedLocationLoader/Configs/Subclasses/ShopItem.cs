using System.ComponentModel;
using Newtonsoft.Json;

namespace Entoarox.AdvancedLocationLoader.Configs
{
    internal class ShopItem
    {
        /*********
        ** Accessors
        *********/
#pragma warning disable CS0649
        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool BigCraftable;

        public string Conditions;
        public int Id;
        public int? Price;

        [DefaultValue(1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Stack;

        [DefaultValue(int.MaxValue)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Stock;
#pragma warning restore CS0649

        /*********
        ** Public methods
        *********/
        public override string ToString()
        {
            return $"ShopItem({this.Id}:{(this.BigCraftable ? "craftable" : "object")}){{Price={this.Price},Stack={this.Stack},Stock={this.Stock},Conditions={this.Conditions}}}";
        }
    }
}
