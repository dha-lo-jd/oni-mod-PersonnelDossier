using System;
using System.Collections.Generic;
using System.Text;

namespace PersonnelDossier;

// Builds the record text of one duplicant. A pure function of the duplicant's id and traits: nothing is stored,
// the same duplicant always gets the same text, and two duplicants with the same trait can get different wording.
public static class Dossier
{
    private const int MaxLinesPerField = 2;
    public const string GrantPrefix = "GrantSkill_";

    // skillName maps a skill id (the part after "GrantSkill_") to its display name, or null when unknown.
    public static string Compose(DossierData data, int seed, IList<string> traitIds, Func<string, string> skillName = null)
    {
        var l = data.labels;
        var good = new List<string>();
        var bad = new List<string>();
        var note = new List<string>();
        var memoCandidates = new List<string>();

        foreach (var id in traitIds)
        {
            string line;
            var kind = "good";
            if (id.StartsWith(GrantPrefix, StringComparison.Ordinal))
            {
                var name = skillName?.Invoke(id.Substring(GrantPrefix.Length));
                if (string.IsNullOrEmpty(name) || data.grantSkill.Count == 0)
                {
                    continue;
                }

                line = string.Format(data.grantSkill[(int)(Hash(seed, id) % (uint)data.grantSkill.Count)], name);
            }
            else if (data.traits.TryGetValue(id, out var text))
            {
                kind = text.kind;
                line = Hash(seed, id) % 2 == 0 ? text.a : text.b;
                if (string.IsNullOrEmpty(line))
                {
                    line = text.a ?? text.b;
                }
            }
            else
            {
                continue;
            }

            var target = kind == "good" ? good : kind == "bad" ? bad : note;
            if (!string.IsNullOrEmpty(line) && target.Count < MaxLinesPerField)
            {
                target.Add(line);
            }

            if (data.memos.ContainsKey(id))
            {
                memoCandidates.Add(id);
            }
        }

        var sb = new StringBuilder();
        sb.Append(l.record).Append(": ").AppendLine(RecordNumber(seed));
        Field(sb, l.recommend, good, l.none);
        Field(sb, l.concerns, bad, l.none);
        if (note.Count > 0)
        {
            Field(sb, l.notes, note, null);
        }

        sb.Append(l.reason).Append(": ").Append(l.blank);

        var memo = Memo(data, seed, memoCandidates);
        if (memo != null)
        {
            sb.AppendLine().Append(l.memo).Append(": ").Append(memo);
        }

        return sb.ToString();
    }

    public static string RecordNumber(int seed) => "P-" + (Hash(seed, "record") % 1000000).ToString("D6");

    // Roughly one dossier in three carries a handler's note: a trait-bound one when the duplicant has such a trait, else a generic one.
    private static string Memo(DossierData data, int seed, List<string> candidates)
    {
        if (Hash(seed, "memo") % 3 != 0)
        {
            return null;
        }

        if (candidates.Count > 0)
        {
            var id = candidates[(int)(Hash(seed, "memo-pick") % (uint)candidates.Count)];
            return data.memos[id];
        }

        return data.genericMemos.Count > 0 ? data.genericMemos[(int)(Hash(seed, "memo-generic") % (uint)data.genericMemos.Count)] : null;
    }

    private static void Field(StringBuilder sb, string label, List<string> lines, string fallback)
    {
        sb.Append(label).Append(": ");
        sb.AppendLine(lines.Count == 0 ? fallback ?? "" : string.Join("", lines));
    }

    // FNV-1a over the seed and a salt: stable across runs (string.GetHashCode is not).
    private static uint Hash(int seed, string salt)
    {
        unchecked
        {
            var h = 2166136261u;
            for (var i = 0; i < 4; i++)
            {
                h = (h ^ (uint)((seed >> (i * 8)) & 0xFF)) * 16777619u;
            }

            foreach (var c in salt)
            {
                h = (h ^ c) * 16777619u;
            }

            return h;
        }
    }
}
