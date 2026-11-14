using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KMod;
using UnityEngine;

namespace PersonnelDossier;

public sealed class PersonnelDossierMod : UserMod2
{
    internal static string ModPath;

    public override void OnLoad(Harmony harmony)
    {
        base.OnLoad(harmony);
        ModPath = path;
    }

    // Duplicant Surveillance builds its window on first open, so registering here works whatever the load order.
    public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods)
    {
        base.OnAllModsLoaded(harmony, mods);
        if (mods.Any(m => m.staticID == "kawo.DuplicantSurveillance"))
        {
            Register();
        }
        else
        {
            Debug.Log("[PersonnelDossier] Duplicant Surveillance is not installed; nothing to add the dossier to.");
        }
    }

    // Kept in its own method so this mod still loads when Duplicant Surveillance isn't installed.
    private static void Register()
    {
        DuplicantSurveillance.SurveillanceApi.RegisterSection(new DossierSection());
    }
}
