# ブラウザ自動スクショによる動作確認

デザインファイルを変更した後、実際に画面がどう表示されるかを Claude Code から Playwright 経由でスクショして確認する手順。

## 前提

- サーバー（Blazor WebAssembly アプリ）が `https://localhost:7169/` で起動していること
  - 通常は Visual Studio からデバッグ起動する
  - 対象のデザインデータが読み込まれているサーバーを起動すること（空のテンプレートだとサイドバーが空になる）
- Node.js がインストール済みであること

## 初回セットアップ

### 1. settings.local.json に許可を追加

Claude Code は `.claude/settings.local.json` の `permissions.allow` にないコマンドを実行できない。
以下を追加する（動作確認が終わったら削除する運用でよい）：

```json
"Bash(node:*)",
"Bash(npm install:*)",
"Bash(npx playwright:*)"
```

### 2. Playwright をインストール

```bash
npm install playwright
npx playwright install chromium
```

## スクショ撮影スクリプトの雛形

以下を `.js` ファイルに保存して `node スクリプト名.js` で実行する。

```javascript
const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch();
  const page = await browser.newPage({
    viewport: { width: 1400, height: 2500 },
    ignoreHTTPSErrors: true,
  });

  // 初回ロード（Blazor WASM 初期化に 15 秒程度必要）
  await page.goto('https://localhost:7169/', {
    ignoreHTTPSErrors: true,
    timeout: 60000,
  });
  await page.waitForTimeout(15000);

  // サイドバーのリンクをクリックして対象ページへ遷移
  const link = page
    .locator('.sidebar-nav a', { hasText: '対象ページ名' })
    .first();
  await link.click();
  await page.waitForTimeout(3000);

  await page.screenshot({ path: 'output.png' });
  await browser.close();
})();
```

### ポイント

- **WASM の初回ロードは時間がかかる** — `waitForTimeout(15000)` で待つ
- **自己署名証明書** — `ignoreHTTPSErrors: true` を必ず付ける
- **viewport の height を大きくする** — 縦に長いページでも `fullPage: true` なしで収まる
- **ページ遷移後も待機を入れる** — クリック後に 2〜3 秒待つと描画が安定する

## デザインデータ変更時の反映方法

| 変更内容 | サーバー再起動 |
|---|---|
| `*.mod.json` / `*.frm.json` / `*.sql` 等のデザインファイル | **不要**（デザイナーから送るだけで反映される） |
| `app.css` の変更 | 必要（再ビルド＆再起動） |
| C# スクリプト `*.mod.cs` の変更 | 必要（再ビルド＆再起動） |

デザインファイルだけいじっている場合は、サーバーは立てっぱなしでスクショを撮り直せる。

## 結果の確認

スクショは必ず PIL などでピクセル単位で測定してから判断すること。
目視での「だいたい合ってる」は誤判定の元。要素の位置・サイズが数値で期待通りになっているかを確認する。
