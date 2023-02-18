using System;
using Newtonsoft.Json;

string json = "[{ \"id\": 53, \"first\": \"Bill\", \"last\": \"Bryson\", \"age\":23, \"gender\":\"M\" }, { \"id\": 62, \"first\": \"John\", \"last\": \"Travolta\", \"age\":54, \"gender\":\"M\" }, { \"id\": 41, \"first\": \"Frank\", \"last\": \"Zappa\", \"age\":23, gender:\"T\" }, { \"id\": 31, \"first\": \"Jill\", \"last\": \"Scott\", \"age\":66, gender:\"Y\" }, { \"id\": 31, \"first\": \"Anna\", \"last\": \"Meredith\", \"age\":66, \"gender\":\"Y\" }, { \"id\": 31, \"first\": \"Janet\", \"last\": \"Jackson\", \"age\":66, \"gender\":\"F\" }]";
//List<User> users = JsonConvert.DeserializeObject<List<User>>(json, new TolerantEnumConverter(typeof(User)));
List<User> users = JsonConvert.DeserializeObject<List<User>>(json, new TolerantEnumConverter());

foreach (User element in users){
    Console.WriteLine("id: {0}", element.id );
    Console.WriteLine("first: {0}", element.first );
    Console.WriteLine("last: {0}", element.last );
    Console.WriteLine("age: {0}", element.age );
    Console.WriteLine("gender: {0}", element.gender.ToString() );
    Console.WriteLine("=====");
}


public enum gender
{
    M = 1,
    F = 2,
    Unknown = 3
}

public class User
{
    public Int32 id { get; set; }
    public string first { get; set; }
    public string last { get; set; }
    public Int32 age { get; set; }
    public gender gender { get; set; }
};

public class TolerantEnumConverter : JsonConverter
{
    /*
    private readonly Type[] _types;
    public TolerantEnumConverter(params Type[] types)
    {
        _types = types;
    }
    */

    public override bool CanConvert(Type objectType)
    {
        //return _types.Any(t => t == objectType);
        Type type = IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType;
        return type.IsEnum;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        bool isNullable = IsNullableType(objectType);
        Type enumType = isNullable ? Nullable.GetUnderlyingType(objectType) : objectType;

        string[] names = Enum.GetNames(enumType);

        if (reader.TokenType == JsonToken.String)
        {
            string enumText = reader.Value.ToString();

            if (!string.IsNullOrEmpty(enumText))
            {
                string match = names
                    .Where(n => string.Equals(n, enumText, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

                if (match != null)
                {
                    return Enum.Parse(enumType, match);
                }
            }
        }
        else if (reader.TokenType == JsonToken.Integer)
        {
            int enumVal = Convert.ToInt32(reader.Value);
            int[] values = (int[])Enum.GetValues(enumType);
            if (values.Contains(enumVal))
            {
                return Enum.Parse(enumType, enumVal.ToString());
            }
        }

        if (!isNullable)
        {
            string defaultName = names
                .Where(n => string.Equals(n, "Unknown", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (defaultName == null)
            {
                defaultName = names.First();
            }

            return Enum.Parse(enumType, defaultName);
        }

        return null;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
    }

    public override bool CanWrite
    {
        get { return false; }
    }

    private bool IsNullableType(Type t)
    {
        return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
    }
}
