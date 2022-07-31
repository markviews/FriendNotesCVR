using MelonLoader;
using UnityEngine;
using FriendNotesCVR;
using cohtml;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System;
using ABI_RC.Core.InteractionSystem;
using HarmonyLib;
using ABI_RC.Core.Networking.IO.Social;
using System.Reflection;

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

        public static Dictionary<string, UserNote> notes;
        public static bool showNotesOnNameplates;
        public static bool logDate;
        public static bool showDateOnNameplates;
        public static bool logName;
        public static string dateFormat;
        private static string LastSelectedGUID = "";

        public static JsonSerializerSettings JsonSettings = new JsonSerializerSettings {
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
            OnPreferencesSaved();

            HarmonyInstance.Patch(typeof(ViewManager).GetMethod(nameof(ViewManager.OnUserDetailsRequestReady), AccessTools.all), null, new HarmonyMethod(typeof(FriendNotes).GetMethod(nameof(UserRequested), BindingFlags.NonPublic | BindingFlags.Static)));
        }

        private static void UserRequested() {
            LastSelectedGUID = Users.Requested.UserId;
            MelonLogger.Msg(LastSelectedGUID);
        }

        public override void OnPreferencesSaved() {
            showNotesOnNameplates = MelonPreferences.GetEntryValue<bool>("FriendNotes", "showNotesOnNameplates");
            logDate = MelonPreferences.GetEntryValue<bool>("FriendNotes", "logDate");
            logName = MelonPreferences.GetEntryValue<bool>("FriendNotes", "logName");
            showDateOnNameplates = MelonPreferences.GetEntryValue<bool>("FriendNotes", "showDateOnNameplates");
            dateFormat = MelonPreferences.GetEntryValue<string>("FriendNotes", "dateFormat");
        }

        /*
        public void addMenuButton() {
            GameObject obj = GameObject.Find("Cohtml/CohtmlWorldView");
            if (obj == null) {
                MelonLogger.Error("obj null");
                return;
            }

            CohtmlView view = obj.GetComponent<CohtmlView>();
            if (view == null) {
                MelonLogger.Error("view null");
                return;
            }

            string js = 
                "var button = document.createElement('div'); " +
                "button.innerHTML = 'Edit Note';" +
                "button.style = 'position: absolute; top: 1%; right: 25%; border-radius: 0.25em; border: 3px solid rgb(89, 136, 93); padding: 0.5em; width: 8em; text-align: center;" +
                "button.onclick = function(event) { engine.trigger('FriendNotesClickEdit') };" +
                "document.getElementById('user-detail').appendChild(button);";

            view.View.ExecuteScript(js);
            MelonLogger.Msg("ran js");
        }
        */

        private void openKeyboard() {
            bool menuOpen = ViewManager.Instance.isGameMenuOpen();
            if (!menuOpen) return;
            ViewManager.Instance.openMenuKeyboard("");
        }

        private string getKeyboardText() {
            GameObject worldView = GameObject.Find("Cohtml/CohtmlWorldView");
            if (worldView == null) return "";

            CohtmlView htmlView = worldView.GetComponent<CohtmlView>();
            if (htmlView == null) return "";

            return htmlView.TextInputHandler.InputText;
        }

        public static void setNote(string userID, string newNote) {
            if (notes.ContainsKey(userID)) notes[userID].Note = newNote;
            else notes[userID] = new UserNote() { Note = newNote };

            saveNotes();

            //TODO update nameplate
        }

        public static void saveNotes() {
            FileInfo notesFile = new FileInfo("UserData/FriendNotes.json");

            if (!notesFile.Exists) notesFile.Create().Close();
            File.WriteAllText(notesFile.FullName, JsonConvert.SerializeObject(notes, JsonSettings));
        }

        public static Dictionary<string, UserNote> loadNotes() {
            FileInfo notesFile = new FileInfo("UserData/FriendNotes.json");
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
