using Codeer.LowCode.Blazor.Extras.Server.Auth;
using LowCodeNativeSamples.Server.Services;

namespace LowCodeNativeSamples.Server
{
    /// <summary>
    /// 外部 IdP (Entra ID / Google / AWS Cognito / OIDC) が本人確認したユーザーを、ユーザーテーブルの 1 行に解決する。
    /// これはアプリのプロビジョニング方針なので、パッケージではなくここに置く:
    ///
    /// - 事前登録制 (この実装): IdP のユーザー名 (Entra = UPN、Google / Cognito = メール) がユーザーモジュールの
    ///   LoginAccountContractField の ExternalLoginName (空なら LoginName) の列と一致し、IsActive が偽でない行だけ許可する。行は作らない
    /// - 自動作成: 行が無ければ INSERT する (identity.LoginName / DisplayName / Email が使える)
    /// - Subject での紐付け: identity.Provider + identity.Subject を紐付けテーブルに持てば、メールが変わってもユーザーを維持できる
    ///
    /// null を返すとサインインしない (ログイン画面に user_not_registered で戻る)
    /// </summary>
    public class ExternalLoginUserResolver : IExternalLoginUserResolver
    {
        readonly DataService _dataService;

        public ExternalLoginUserResolver(DataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<ExternalLoginUser?> ResolveAsync(ExternalLoginIdentity identity)
        {
            var accounts = LoginAccountStore.Create(DesignerService.GetDesignData(), _dataService.DbAccess);
            var account = accounts == null ? null : await accounts.FindByExternalLoginNameAsync(identity.LoginName);
            return account == null ? null : new ExternalLoginUser(account.UserId, account.DisplayName);
        }
    }
}
