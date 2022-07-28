using System.Globalization;
using System.Text;
using ABI_RC.Core.Player;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FriendNotesCVR {
    
    public static class UserNotes
    {
        public static Dictionary<string, UserNote> FromFile(FileInfo file) => FromJson(File.ReadAllText(file.FullName));
        public static Dictionary<string, UserNote> FromJson(string json) => JsonConvert.DeserializeObject<Dictionary<string, UserNote>>(json, Converter.Settings);
    }

    public partial class UserNote
    {

        [JsonIgnore]
        public string DisplayName { get { return DisplayNames.Last()?.Name; } }

        [JsonIgnore]
        public string FullText { get {
                StringBuilder sb = new StringBuilder();
                if (HasNote) sb.AppendLine(Note);
                if (HasDate) sb.AppendLine(DateAddedText);
                return sb.ToString();
            } }

        [JsonIgnore]
        public bool HasNote { get { return !string.IsNullOrWhiteSpace(Note); } }
        [JsonProperty("Note", NullValueHandling = NullValueHandling.Ignore)]
        public string Note { get; set; }

        [JsonIgnore]
        public bool HasDate { get { return DateAdded != null; } }
        [JsonIgnore]
        public string DateAddedText { get { return HasDate ? "Added: " + DateAdded?.ToString(FriendNotes.dateFormat) : string.Empty; } }
        [JsonProperty("DateAdded", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? DateAdded { get; set; }

        [JsonProperty("DateRequested", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? DateRequested { get; set; }

        [JsonProperty("DisplayNames", NullValueHandling = NullValueHandling.Ignore)]
        public List<DisplayName> DisplayNames { get; set; }

        public UserNote Update(string displayname = null)
        {
            if (displayname != null && FriendNotes.logName)
            {
                if (DisplayNames is null) DisplayNames = new List<DisplayName>();
                var names = DisplayNames.Where(f => f.Name == displayname);
                if (names.Count() < 1) DisplayNames.Add(new DisplayName(displayname, DateTime.Now));
                else names.Last().DateFirstSeen = DateTime.Now;
            }
            return this;
        }
    }

    public partial class DisplayName
    {
        [JsonProperty("Name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("Date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Date { get; set; }

        [JsonProperty("DateFirstSeen", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? DateFirstSeen { get; set; }

        public DisplayName(string name, DateTime dateFirstSeen)
        {
            Name = name;
            DateFirstSeen = dateFirstSeen;
            
        }

        public override string ToString()
        {
            return $"\"{Name}\" ({DateFirstSeen})";
        }
    }

    public static partial class Extensions
    {

        public static UserNote AddPlayer(this Dictionary<string, UserNote> list, CVRPlayerEntity player) => list.AddPlayer(player);
        public static UserNote AddPlayer(this Dictionary<string, UserNote> list, string userid, string displayName = null)
        {
            var player = new UserNote();
            if (displayName != null) player.DisplayNames = new List<DisplayName>() { new DisplayName(displayName, DateTime.Now) };
            list[userid] = player;
            return player;
        }
        public static UserNote AddOrUpdate(this Dictionary<string, UserNote> list, CVRPlayerEntity player) => list.AddOrUpdate(player);
        public static UserNote AddOrUpdate(this Dictionary<string, UserNote> list, string userid, string displayname = null)
        {
            if (list.ContainsKey(userid)) return list[userid].Update(displayname);
            return list.AddPlayer(userid, displayname);
        }

    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
            Formatting = Formatting.Indented
        };
    }
}
