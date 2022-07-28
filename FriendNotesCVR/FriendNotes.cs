using MelonLoader;
using UnityEngine;
using FriendNotesCVR;
using cohtml;
using Newtonsoft.Json;

[assembly: MelonInfo(typeof(FriendNotes), "FriendNotes", "1.0.0", "MarkViews")]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]

namespace FriendNotesCVR {
    internal class FriendNotes : MelonMod {

        /* TODO
         * 
         * Can publish mod after these 2 work, the more features the better tho:
         * Add 'Edit Note' button to social page
         * Add note to social page
         * 
         * Add note to nameplate
         * Log add Friend event
         * Log friend display names
         * 
         * Review code.. I copy pasted from my old mod. ofc I will credit Bluscream in the new mod
         * (old FriendNotes had 'Date' and 'DateFirstSeen' for each logged DisplayName which was kinda redundent, lets get rid of one of them. This was probably a bug)
         * 
         * possibly add support for coloring notes / date added friend on nameplate.. low priority
         */

        public static FileInfo notesFile = new FileInfo("UserData/FriendNotes.json");
        public static MelonPreferences_Category cat;
        public static Dictionary<string, UserNote> notes;

        public static bool showNotesOnNameplates;
        public static bool logDate;
        public static bool showDateOnNameplates;
        public static bool logName;
        public static string dateFormat;
        public static bool notesAtBioTopNotBottom;

        public override void OnApplicationStart() {
            cat = MelonPreferences.CreateCategory("FriendNotes", "Friend Notes");
            cat.CreateEntry("showNotesOnNameplates", true, "Show notes on nameplates");
            cat.CreateEntry("showDateOnNameplates", true, "Show date on nameplates");
            cat.CreateEntry("logDate", true, "Log date you add friends");
            cat.CreateEntry("logName", true, "Log friend display names");
            cat.CreateEntry("dateFormat", "M/d/yy - hh:mm tt");



        }

        public override void OnPreferencesSaved() {
            showNotesOnNameplates = MelonPreferences.GetEntryValue<bool>(cat.Identifier, "showNotesOnNameplates");
            logDate = MelonPreferences.GetEntryValue<bool>(cat.Identifier, "logDate");
            logName = MelonPreferences.GetEntryValue<bool>(cat.Identifier, "logName");
            showDateOnNameplates = MelonPreferences.GetEntryValue<bool>(cat.Identifier, "showDateOnNameplates");
            dateFormat = MelonPreferences.GetEntryValue<string>(cat.Identifier, "dateFormat");
        }


        //just for testing cause I can't test when servers are down :)
        public override void OnUpdate() {

            if (Input.GetKeyDown(KeyCode.K) && Input.GetKey(KeyCode.LeftControl)) {
                openKeyboard();
            }

            if (Input.GetKeyDown(KeyCode.L) && Input.GetKey(KeyCode.LeftControl)) {
                MelonLogger.Msg("found text: " + getKeyboardText());
            }

        }

        private void openKeyboard() {
            bool menuOpen = ABI_RC.Core.InteractionSystem.ViewManager.Instance.isGameMenuOpen();
            if (!menuOpen) return;
            ABI_RC.Core.InteractionSystem.ViewManager.Instance.openMenuKeyboard("");
        }

        //There's got to be a simpler way to get keyboard text..
        private string getKeyboardText() {
            Transform CohtmlWorldView = ABI_RC.Core.InteractionSystem.ViewManager.Instance.gameObject.transform.Find("CohtmlWorldView");
            if (CohtmlWorldView == null) return "";

            TextInputHandler handler = CohtmlWorldView.GetComponent<TextInputHandler>();
            if (handler == null) return "";

            string text = handler.InputText;
            return text;
        }


        public static void setNote(string userID, string newNote) {
            if (notes.ContainsKey(userID)) notes[userID].Note = newNote;
            else notes[userID] = new UserNote() { Note = newNote };

            saveNotes();

            //TODO update nameplate
        }

        public static void saveNotes() {
            notesFile.Directory.Create();
            if (!notesFile.Exists) notesFile.Create().Close();
            File.WriteAllText(notesFile.FullName, JsonConvert.SerializeObject(notes, FriendNotesCVR.Converter.Settings));
        }

        public static Dictionary<string, UserNote> loadNotes()
        {
            if (notesFile.Exists)
            {
                try
                {
                    notes = UserNotes.FromFile(notesFile);
                    return notes;
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Failed to load notes from {notesFile.FullName}:\n\t{ex.Message}");
                    //notesFile.Backup(true, ".corrupt");
                }
            }
            notes = new Dictionary<string, UserNote>();
            return notes;
        }


    }
}
