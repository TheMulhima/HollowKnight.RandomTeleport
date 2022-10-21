using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing;
using System.Linq;
using System.Text;
using HKMirror;
using HKMirror.Reflection;
using Modding;
using Newtonsoft.Json;

namespace RandomTeleport.Utils
{
    public class RandomConverter : JsonConverter<Random>
    {
        public override void WriteJson(JsonWriter writer, Random value, JsonSerializer serializer)
        {
            var random = value.Reflect();
            writer.WriteStartObject();
            
            /* System.Random has 3 const int we dont care about to serialize because it is const
             * the 3 fields we do care about is inext (int), inextp(int), SeedArray(int[])
             * so we get these and write them in the json. */
            
            writer.WritePropertyName(nameof(random.inext));
            writer.WriteValue(random.inext);

            writer.WritePropertyName(nameof(random.inextp));
            writer.WriteValue(random.inextp);

            writer.WritePropertyName(nameof(random.SeedArray));
            writer.WriteStartArray();
            random.SeedArray.ToList().ForEach(writer.WriteValue);
            writer.WriteEndArray();
            
            writer.WriteEndObject();
        }

        public override Random ReadJson(JsonReader reader, Type objectType, Random existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var random = new System.Random().Reflect();
            
            reader.Read(); //property name
            reader.Read(); //inext value
            random.inext = Convert.ToInt32(reader.Value);
            
            reader.Read(); //property name
            reader.Read(); //inextp value
            random.inextp = Convert.ToInt32(reader.Value);

            reader.Read(); //property name
            reader.Read(); //Writer.WriteStartArray() token
            
            List<int> SeedArrayList = new List<int>();//so it is easier to add elements in while loop
            
            //read until we read Writer.WriteEndArray();
            while (reader.TokenType != JsonToken.EndArray)
            {
                reader.Read(); //value of element in array
                SeedArrayList.Add(Convert.ToInt32(reader.Value));
            }

            random.SeedArray = SeedArrayList.ToArray();

            return random.orig;
        }
    }
}