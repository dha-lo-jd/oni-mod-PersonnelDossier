using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace PersonnelDossier;

// The dossier's text: data/<language code>.json, falling back to en.json. Kept out of the DLL so a translation is one file.
public sealed class DossierData
{
    public sealed class LabelSet
    {
        public string title = "Personnel Record";
        public string record = "Record No.";
        public string recommend = "Recommendations";
        public string concerns = "Concerns";
        public string notes = "Remarks";
        public string reason = "Reason for leaving";
        public string memo = "Handler's note";
        public string none = "None on file (under review)";
        public string blank = "(left blank)";
    }

    public sealed class TraitText
    {
        public string kind; // good | bad | note
        public string a;
        public string b;
    }

    public LabelSet labels = new();
    public Dictionary<string, TraitText> traits = new();
    public Dictionary<string, string> memos = new();
    public List<string> genericMemos = new();

    private static DossierData current;

    public static DossierData Current => current ??= Load();

    private static DossierData Load()
    {
        var code = Localization.GetLocale()?.Code;
        foreach (var name in new[] { code, "en" })
        {
            if (string.IsNullOrEmpty(name) || PersonnelDossierMod.ModPath == null)
            {
                continue;
            }

            var file = Path.Combine(PersonnelDossierMod.ModPath, "data", name + ".json");
            if (!File.Exists(file))
            {
                continue;
            }

            try
            {
                var data = JsonConvert.DeserializeObject<DossierData>(File.ReadAllText(file));
                if (data != null)
                {
                    Debug.Log($"[PersonnelDossier] Loaded {name}.json ({data.traits.Count} traits)");
                    return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[PersonnelDossier] Could not read {file}: {e.Message}");
            }
        }

        return new DossierData();
    }
}
