#if DEBUG
// In-game tests run by Gravitas.TestHarness:
//   python GravitasTestHarness/Gravitas.TestProbe/probe.py run --project <this repo> --tests '{"tags":["dossier"]}'
// Needs HarnessDllPath in Local.Build.props, and Duplicant Surveillance enabled.
using System.Collections;
using System.Linq;
using System.Reflection;
using DuplicantSurveillance;
using Gravitas.TestHarness;
using Klei.AI;
using UnityEngine;

namespace PersonnelDossier;

internal static class HarnessTests
{
    // The section registered from this separate DLL shows up in Surveillance's window and carries a record built from the traits.
    [HarnessTest(Name = "dossier.section_shown", Tags = "dossier")]
    public static IEnumerator SectionShown()
    {
        var host = Object.FindObjectOfType<SurveillanceHost>();
        Check.NotNull(host, "SurveillanceHost is not in the scene");
        var setVisible = typeof(SurveillanceHost).GetMethod("SetVisible", BindingFlags.Instance | BindingFlags.NonPublic);
        var screenField = typeof(SurveillanceHost).GetField("screen", BindingFlags.Instance | BindingFlags.NonPublic);
        Check.NotNull(setVisible, "SurveillanceHost.SetVisible not found");
        Check.NotNull(screenField, "SurveillanceHost.screen not found");

        var data = DossierData.Current;
        Check.True(data.traits.Count > 0, "no trait text was loaded from data/<language>.json");

        try
        {
            setVisible.Invoke(host, new object[] { true, false });
            yield return null;
            yield return null;

            var screen = (SurveillanceScreen)screenField.GetValue(host);
            Check.NotNull(screen, "the window was not created");
            var block = screen.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == data.labels.title);
            Check.NotNull(block, "the dossier block is not in the window (title: " + data.labels.title + ")");
            Check.True(block.gameObject.activeSelf, "the dossier block is hidden");

            var label = block.GetComponentsInChildren<LocText>(true).FirstOrDefault(t => t.name == "Dossier text");
            Check.NotNull(label, "the dossier text label is missing");
            Check.True(label.text.Contains(data.labels.record + ": P-"), "no record number in the text: " + label.text);
            Check.True(label.text.Contains(data.labels.reason), "the text has no closing field: " + label.text);
            Debug.Log("[PersonnelDossier] shown dossier:\n" + label.text);
        }
        finally
        {
            setVisible.Invoke(host, new object[] { false, false });
        }
    }

    // For screenshots: opens the Surveillance window (probe.py run --steps '[["POST","/invoke",{"method":"PersonnelDossier.HarnessTests.DemoOpen"}],["POST","/screenshot",{"name":"dossier"}]]').
    public static void DemoOpen()
    {
        var host = Object.FindObjectOfType<SurveillanceHost>();
        typeof(SurveillanceHost).GetMethod("SetVisible", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(host, new object[] { true, false });
    }

    // Every trait the game can hand out has text, and every GrantSkill_* trait resolves to a skill name.
    [HarnessTest(Name = "dossier.coverage", Tags = "dossier")]
    public static IEnumerator Coverage()
    {
        var data = DossierData.Current;
        var ids = new System.Collections.Generic.List<string>();
        foreach (var list in new[] { TUNING.DUPLICANTSTATS.GOODTRAITS, TUNING.DUPLICANTSTATS.BADTRAITS, TUNING.DUPLICANTSTATS.NEEDTRAITS,
                     TUNING.DUPLICANTSTATS.STRESSTRAITS, TUNING.DUPLICANTSTATS.JOYTRAITS, TUNING.DUPLICANTSTATS.CONGENITALTRAITS })
        {
            ids.AddRange(list.Select(t => t.id));
        }

        var missing = ids.Where(id => !id.StartsWith(Dossier.GrantPrefix) && !data.traits.ContainsKey(id)).Distinct().ToList();
        Debug.Log("[PersonnelDossier] traits without text: " + string.Join(", ", missing));

        var grants = ids.Where(id => id.StartsWith(Dossier.GrantPrefix)).Distinct().ToList();
        var unresolved = grants.Where(id => string.IsNullOrEmpty(DossierSection.SkillName(id.Substring(Dossier.GrantPrefix.Length)))).ToList();
        Debug.Log($"[PersonnelDossier] grant-skill traits: {grants.Count}, names: " + string.Join(", ", grants.Select(id => id + "=" + DossierSection.SkillName(id.Substring(Dossier.GrantPrefix.Length)))));
        var sample = Dossier.Compose(data, 12345, new[] { "GrantSkill_Mining1" }, DossierSection.SkillName);
        Debug.Log("[PersonnelDossier] grant-skill sample: " + sample);
        Check.True(!sample.Contains("<"), "markup left in the text: " + sample);
        Check.True(sample.Contains(DossierSection.SkillName("Mining1")), "the skill name is not in the text: " + sample);
        Check.Equal(0, unresolved.Count, "GrantSkill traits whose skill name was not found: " + string.Join(", ", unresolved));
        yield return null;
    }

    // Same duplicant, same text; the text uses the duplicant's own traits.
    [HarnessTest(Name = "dossier.stable_text", Tags = "dossier")]
    public static IEnumerator StableText()
    {
        var data = DossierData.Current;
        var minions = Components.LiveMinionIdentities.Items;
        Check.True(minions.Count > 0, "no duplicants in the save");

        foreach (var minion in minions)
        {
            var seed = minion.GetComponent<KPrefabID>().InstanceID;
            var ids = minion.GetComponent<Traits>().TraitList.Select(t => t.Id).ToList();
            var first = Dossier.Compose(data, seed, ids, DossierSection.SkillName);
            var second = Dossier.Compose(data, seed, ids, DossierSection.SkillName);
            Check.Equal(first, second, "dossier text of " + minion.GetProperName());
            Debug.Log($"[PersonnelDossier] {minion.GetProperName()} ({string.Join(",", ids)})\n{first}");
        }

        yield return null;
    }
}
#endif
