using System.Linq;
using System.Text.RegularExpressions;
using DuplicantSurveillance;
using Klei.AI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PersonnelDossier;

// The block Duplicant Surveillance shows under the built-in ones: a personnel record built from the target's traits.
public sealed class DossierSection : ISection
{
    private LocText body;
    private MinionIdentity shown;
    private string shownText;

    public string Title => DossierData.Current.labels.title;

    public bool IsValid(MinionIdentity target) => target.GetComponent<KPrefabID>() != null && target.GetComponent<Traits>() != null;

    public void Build(RectTransform parent)
    {
        var go = new GameObject("Dossier text", typeof(RectTransform));
        go.SetActive(false);
        go.transform.SetParent(parent, false);

        // Awake runs on activation; configure first so LocText doesn't look up a null string key.
        body = go.AddComponent<LocText>();
        body.key = "";
        body.allowOverride = true;
        body.textStyleSetting = ToolTipScreen.Instance.defaultTooltipBodyStyle;
        go.SetActive(true);

        body.ApplySettings();
        body.alignment = TextAlignmentOptions.TopLeft;
        body.raycastTarget = false;
        body.textWrappingMode = TextWrappingModes.Normal;

        var element = go.AddComponent<LayoutElement>();
        element.flexibleWidth = 1f;
        element.minHeight = 20f;
    }

    // The game's names carry link markup (<link="MINING1">...</link>); the record is plain text.
    public static string SkillName(string skillId) => Regex.Replace(Db.Get().Skills.TryGet(skillId)?.Name ?? "", "<[^>]+>", "");

    public void Refresh(MinionIdentity target)
    {
        var seed = target.GetComponent<KPrefabID>().InstanceID;
        var ids = target.GetComponent<Traits>().TraitList.Select(t => t.Id).ToList();
        var text = Dossier.Compose(DossierData.Current, seed, ids, SkillName);
        if (shown != target || shownText != text)
        {
            body.text = text;
            shown = target;
            shownText = text;
        }
    }
}
