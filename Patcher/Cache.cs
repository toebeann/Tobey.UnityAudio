using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Tobey.UnityAudio;
internal class Cache
{
    private static readonly JsonSerializer serializer = new();

    internal class GlobalGameManagersCache
    {
        public long LastWriteTimestampTicks { get; set; }
        public bool UnityAudioDisabled { get; set; }
    }

    public GlobalGameManagersCache GlobalGameManagers { get; set; } = new();

    public void SaveToBsonFile(string path)
    {
        using FileStream fileStream = File.OpenWrite(path);
#pragma warning disable CS0618 // Type or member is obsolete
        using BsonWriter bsonWriter = new(fileStream);
#pragma warning restore CS0618 // Type or member is obsolete
        serializer.Serialize(bsonWriter, this);
    }

    public void SaveToJsonFile(string path)
    {
        using StreamWriter streamWriter = File.CreateText(path);
        serializer.Serialize(streamWriter, this);
    }

    public void SaveToFile(string path)
    {
        if (Path.GetExtension(path).ToLowerInvariant() == ".json")
        {
            SaveToJsonFile(path);
        }
        else
        {
            SaveToBsonFile(path);
        }
    }

    public static Cache LoadFromBsonFile(string path)
    {
        using FileStream fileStream = File.OpenRead(path);
#pragma warning disable CS0618 // Type or member is obsolete
        using BsonReader bsonReader = new(fileStream);
#pragma warning restore CS0618 // Type or member is obsolete
        return serializer.Deserialize<Cache>(bsonReader);
    }

    public static Cache LoadFromJsonFile(string path)
    {
        using StreamReader streamReader = File.OpenText(path);
        using JsonReader jsonReader = new JsonTextReader(streamReader);
        return serializer.Deserialize<Cache>(jsonReader);
    }

    public static Cache LoadFromFile(string path)
    {
        if (Path.GetExtension(path).ToLowerInvariant() == ".json")
        {
            try
            {
                return LoadFromJsonFile(path);
            }
            catch { }
        }

        return LoadFromBsonFile(path);
    }
}
