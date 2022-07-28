
namespace FriendNotesCVR {

    public partial class UserNote {

        public string Note;
        public List<DisplayName> DisplayNames;

        public void LogDisplayName(string displayname) {
            if (displayname != null && FriendNotes.logName) {
                if (DisplayNames is null) DisplayNames = new List<DisplayName>();
                var names = DisplayNames.Where(f => f.Name == displayname);
                if (names.Count() < 1) DisplayNames.Add(new DisplayName(displayname));
            }
        }
    }

    public partial class DisplayName {
        public string Name;
        public DateTime DateFirstSeen;

        public DisplayName(string name) {
            Name = name;
            DateFirstSeen = DateTime.Now;
        }
    }

}
