// 承認フロー実機確認サンプル (高機能版)。
// 経路は経路マスタから金額で選ぶ (10万円未満 = 「購買ルート」、10万円以上 = 「購買ルート(高額)」)。
// マスタの読み込みと検証 (経路が無い / 自己承認) は経路マスタモジュール (ApprovalRoute.mod.cs) の Load に共通化してある。
// 申請時のデータ正当性チェック + 最終承認者だけが編集できる査定額フィールドのデモ。
// 申請・再申請ボタンはフィールドの標準 UI

// 経路を組み立てる (フィールドの「経路組み立て」に設定。null を返すと申請中止)
ApprovalRouteData OnBuildRoute()
{
    //申請時のデータ正当性チェック (null を返すと申請は中止され、保存もされない)
    if (Amount.Value == null || Amount.Value <= 0)
    {
        Logger.Error("金額は1円以上を入力してください");
        return null;
    }

    //金額で経路マスタの経路を選ぶ (経路が無い / 自己承認の検証は Load の中)
    var routeName = Amount.Value >= 100000 ? "購買ルート(高額)" : "購買ルート";
    return new ApprovalRoute().Load(routeName);
}
