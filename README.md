# Personnel Dossier

An add-on for [Duplicant Surveillance](../DuplicantSurveillance): adds a "Personnel Record" block to its window, written from each duplicant's traits in the voice of a Gravitas printing-pod personnel file.

Text only. No gameplay effect, no notifications, nothing is written to the save file: the record is a function of the duplicant's id and traits, so it is always the same for the same duplicant.

Requires Duplicant Surveillance. Without it this mod does nothing.

## Text

`PersonnelDossier/data/<language code>.json` (falls back to `en.json`). A new language is a copy of `en.json` with the text replaced.

- `traits`: per trait id, a `kind` (`good` / `bad` / `note`) and two wordings `a` / `b`; which one a duplicant gets is decided by their id.
- `memos`: a trait-bound handler's note. About one record in three carries a note (a trait-bound one if the duplicant has such a trait, else one of `genericMemos`).
- Traits without an entry (including other mods' traits) are left out of the record.

## Building

Copy `PersonnelDossier/Local.Build.props.example` to `Local.Build.props` and fill it in (`SurveillanceDllPath` must point at a Duplicant Surveillance build that has `SurveillanceApi`), then `dotnet build`.
