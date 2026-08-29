// 経路マスタ (経路 / ステップ / ステップ承認者) から承認経路 (ApprovalRouteData) を組み立てる共通処理。
// 経路マスタはただのユーザー定義モジュールで、承認フロー側 (ApprovalFlowField / エンジン) はこの形を知らない。
// 申請書のスクリプト (OnBuildRoute) から `new ApprovalRoute().Load("経費ルート")` のように呼ぶ。
// 役職や部署から承認者を決めたい場合はこの関数を書き換えれば全申請書に効く

// 経路名でマスタを読んで経路を返す。申請できない経路 (マスタに無い / 申請者自身が承認者に含まれる) は
// エラーを表示して null を返す (OnBuildRoute がそのまま返せば申請中止)
ApprovalRouteData Load(string routeName)
{
    var routes = new ModuleSearcher<ApprovalRoute>();
    routes.AddEquals(r => r.RouteName.Value, routeName);
    var master = routes.ExecuteFirstOrDefault();
    if (master == null)
    {
        Logger.Error("経路マスタに『" + routeName + "』がありません");
        return null;
    }

    var steps = new ModuleSearcher<ApprovalRouteStep>();
    steps.AddEquals(s => s.Route.Value, master.Id.Value);
    steps.OrderBy(s => s.StepNo.Value);

    var route = new ApprovalRouteData();
    foreach (var s in steps.Execute())
    {
        var step = route.AddStep(s.StepName.Value);
        if (s.StepType.Value != null && s.StepType.Value != "") step.StepType = s.StepType.Value;
        if (s.CompletionPolicy.Value != null && s.CompletionPolicy.Value != "") step.CompletionPolicy = s.CompletionPolicy.Value;
        if (s.ReturnScope.Value != null && s.ReturnScope.Value != "") step.ReturnScope = s.ReturnScope.Value;
        step.IsCommentRequiredOnReject = s.IsCommentRequiredOnReject.Value ?? true;

        var members = new ModuleSearcher<ApprovalRouteStepMember>();
        members.AddEquals(m => m.Step.Value, s.Id.Value);
        foreach (var m in members.Execute())
        {
            if (m.ApproverUser.Value == null) continue;
            if (m.ApproverUser.Value == CurrentUser.Id.Value)
            {
                Logger.Error("申請者自身が承認者に含まれる経路 (" + routeName + ") では申請できません");
                return null;
            }
            step.AddMember(m.ApproverUser.Value, m.IsRequired.Value ?? true);
        }
    }
    return route;
}
