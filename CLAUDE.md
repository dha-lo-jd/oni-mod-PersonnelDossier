# CLAUDE.md

ONI mod。`DuplicantSurveillance` のアドオン（人事ファイル）。共通事項は HQ（親フォルダ）の `CLAUDE.md`。Build: `dotnet build PersonnelDossier/PersonnelDossier.csproj`。

- サーベイランスの `SurveillanceApi.RegisterSection` に `ISection`（`DossierSection`）を `OnAllModsLoaded` で登録する。参照は `SurveillanceDllPath` の DLL（`Private=false`）。呼び出しは別メソッド（`Register`）に分けてあり、サーベイランスが無くても読み込める。
- 文章は `PersonnelDossier/data/<言語>.json`。`ja.json` が本編、`en.json` は対訳。特性は ID（`TUNING.DUPLICANTSTATS` の `id`）で引く。表示名ではない（例: Gourmet = `Foodie`、Rainmaker = `DataRainer`、Yokel = `CantResearch`）。
- 文面は複製人間の `KPrefabID.InstanceID` から決める（`Dossier.Compose`）。保存しない・同じ人は同じ文面。
- サーベイランスの `UiFactory` は `internal` なので、ラベルは `DossierSection.Build` で自前に作る。
- 方針: 効果の数値は書かない、ゲームプレイに影響しない、通知しない。文体は官僚調で、末尾の「担当者メモ」だけ一言を許す。

## 実機検証
`python C:/Users/dha_1/ONI-Modding/GravitasTestHarness/Gravitas.TestProbe/probe.py run --project . --tests '{"tags":["dossier"]}'`（ゲームを起動し、本番の `mods.json` を書き換える。実行前にユーザーに確認）。初回登録（`--setup`）は 2026-11-14 に済み。
見た目: `--steps '[["POST","/invoke",{"method":"PersonnelDossier.HarnessTests.DemoOpen"}],["POST","/screenshot",{"name":"dossier"}]]'`（ウィンドウを開くとゲームが止まるので `/time/advance` は挟めない）。
