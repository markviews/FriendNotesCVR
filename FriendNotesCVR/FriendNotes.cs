﻿using MelonLoader;
using UnityEngine;
using FriendNotesCVR;
using cohtml;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;

[assembly: MelonInfo(typeof(FriendNotes), "FriendNotes", "1.0.0", "MarkViews")]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]

namespace FriendNotesCVR {
    internal class FriendNotes : MelonMod {

        /* TODO
         * 
         * Add 'Edit Note' button to social page
         * Add note to social page
         * Add note to nameplate
         * Log add Friend event
         * Log friend display names
         * 
         * possibly add support for coloring notes / date added friend on nameplate.. low priority
         */

        public static FileInfo notesFile = new FileInfo("UserData/FriendNotes.json");
        public static Dictionary<string, UserNote> notes;
        public static bool showNotesOnNameplates;
        public static bool logDate;
        public static bool showDateOnNameplates;
        public static bool logName;
        public static string dateFormat;

        public static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } },
            Formatting = Formatting.Indented
        };

        public override void OnApplicationStart() {
            MelonPreferences_Category cat = MelonPreferences.CreateCategory("FriendNotes", "Friend Notes");
            cat.CreateEntry("showNotesOnNameplates", true, "Show notes on nameplates");
            cat.CreateEntry("showDateOnNameplates", true, "Show date on nameplates");
            cat.CreateEntry("logDate", true, "Log date you add friends");
            cat.CreateEntry("logName", true, "Log friend display names");
            cat.CreateEntry("dateFormat", "M/d/yy - hh:mm tt");

            notes = loadNotes();
        }

        public override void OnPreferencesSaved() {
            showNotesOnNameplates = MelonPreferences.GetEntryValue<bool>("FriendNotes", "showNotesOnNameplates");
            logDate = MelonPreferences.GetEntryValue<bool>("FriendNotes", "logDate");
            logName = MelonPreferences.GetEntryValue<bool>("FriendNotes", "logName");
            showDateOnNameplates = MelonPreferences.GetEntryValue<bool>("FriendNotes", "showDateOnNameplates");
            dateFormat = MelonPreferences.GetEntryValue<string>("FriendNotes", "dateFormat");
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
            if (!notesFile.Exists) notesFile.Create().Close();
            File.WriteAllText(notesFile.FullName, JsonConvert.SerializeObject(notes, JsonSettings));
        }

        public static Dictionary<string, UserNote> loadNotes() {
            if (notesFile.Exists) {
                try {
                    return JsonConvert.DeserializeObject<Dictionary<string, UserNote>>(File.ReadAllText(notesFile.FullName), JsonSettings);
                } catch (Exception ex) {
                    MelonLogger.Error($"Failed to load notes from {notesFile.FullName}:\n\t{ex.Message}");
                    //notesFile.Backup(true, ".corrupt");
                }
            }
            return new Dictionary<string, UserNote>();
        }

    }
}
